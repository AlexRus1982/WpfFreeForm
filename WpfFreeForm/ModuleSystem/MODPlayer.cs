using System.Text;
using System.IO;

namespace ModuleSystem
{
	public class MODPattern : ModulePattern
	{
		private ModulePatternChannel createNewPatternChannel(Stream stream, int nSamples)
		{
			ModulePatternChannel channel = new ModulePatternChannel();
			int b0 = stream.ReadByte();
			int b1 = stream.ReadByte();
			int b2 = stream.ReadByte();
			int b3 = stream.ReadByte();

			channel.instrument = ((b0 & 0xF0) | ((b2 & 0xF0) >> 4)) & nSamples;
			channel.period = ((b0 & 0x0F) << 8) | b1;

			if (channel.period > 0) channel.noteIndex = ModuleConst.getNoteIndexForPeriod(channel.period);

			channel.effekt = b2 & 0x0F;
			channel.effektOp = b3;

			return channel;
		}
		public void readPatternData(Stream stream, string modID, int nChannels, int nSamples)
		{
			if (modID == "FLT8") // StarTrekker is slightly different
			{
				for (int row = 0; row < 64; row++)
				{
					ModulePatternRow pRow = new ModulePatternRow();
					for (int channel = 0; channel < 4; channel++)
					{
						pRow.patternChannels.Add(createNewPatternChannel(stream, nSamples));
					}
					for (int channel = 4; channel < 8; channel++) 
						pRow.patternChannels.Add(new ModulePatternChannel());
					
					patternRows.Add(pRow);
				}
				for (int row = 0; row < 64; row++)
					for (int channel = 4; channel < 8; channel++)
						patternRows[row].patternChannels[channel] =	createNewPatternChannel(stream, nSamples);
			}
			else
			{
				for (int row = 0; row < 64; row++)
				{
					ModulePatternRow pRow = new ModulePatternRow();
					for (int channel = 0; channel < nChannels; channel++)
						pRow.patternChannels.Add(createNewPatternChannel(stream, nSamples));

					patternRows.Add(pRow);
				}
			}
		}
	}
	public class MODMixer : ModuleMixer
    {
		private bool noEffect(ModuleMixerChannel mc)				// no effect
		{
			return false;
		}
		private bool noteEffect0(ModuleMixerChannel mc)				// Arpeggio
		{
            if (mc.effectArg == 0) return false;
            mc.arpeggioCount = 0;
            mc.arpeggioIndex = mc.noteIndex;
            mc.arpeggioX = mc.arpeggioIndex + mc.effectArgX;
            mc.arpeggioY = mc.arpeggioIndex + mc.effectArgY;
			mc.arpeggioPeriod0 = ModuleConst.getNotePeriod(mc.arpeggioIndex, /*mc.currentFineTune*/ 0);
			mc.period = mc.arpeggioPeriod0;
			mc.arpeggioPeriod1 = (mc.arpeggioX < 60) ? ModuleConst.getNotePeriod(mc.arpeggioX, /*mc.currentFineTune*/ 0) : mc.arpeggioPeriod0;
			mc.arpeggioPeriod2 = (mc.arpeggioY < 60) ? ModuleConst.getNotePeriod(mc.arpeggioY, /*mc.currentFineTune*/ 0) : mc.arpeggioPeriod0;
			return true;
		}
		private bool noteEffect1(ModuleMixerChannel mc)				// Slide up (Portamento Up)
		{
            mc.portamentoStart = mc.period;
            mc.portamentoStep = -mc.effectArg;
            return true;
		}
		private bool noteEffect2(ModuleMixerChannel mc)             // Slide down (Portamento Down)
		{
            mc.portamentoStart = mc.period;
            mc.portamentoStep = mc.effectArg;
            return true;
		}
		private bool noteEffect3(ModuleMixerChannel mc)             // Slide to note
		{
			if (mc.effectArg != 0)
			{
				mc.lastPortamentoStep = mc.portamentoStep;
				mc.portamentoStep = (mc.portamentoStart <= mc.portamentoEnd) ? mc.effectArg : -mc.effectArg;
			}
			else
				mc.portamentoStep = mc.lastPortamentoStep;
			
            return true;
		}
		private bool noteEffect4(ModuleMixerChannel mc)             // Vibrato
		{
            mc.vibratoStart = mc.period;
			mc.vibratoPeriod = mc.vibratoStart;
            if (mc.effectArgX != 0 && mc.effectArgY != 0)
            {
				mc.lastVibratoFreq = mc.effectArgX;
				mc.lastVibratoAmp = mc.effectArgY;
                mc.vibratoFreq = mc.effectArgX;
                mc.vibratoAmp = mc.effectArgY;
				mc.vibratoCount = 0;
			}
			else
            {
				mc.vibratoFreq = mc.lastVibratoFreq;
				mc.vibratoAmp = mc.lastVibratoAmp;
				mc.vibratoCount = 0;
			}
			return true;
		}
		private bool noteEffect5(ModuleMixerChannel mc)             // Continue Slide to note + Volume slide
		{
			noteEffectA(mc);
			return true;
		}
		private bool noteEffect6(ModuleMixerChannel mc)             // Continue Vibrato + Volume Slide
		{
            noteEffectA(mc);
			mc.effectArgX = 0;
			mc.effectArgY = 0;
			noteEffect4(mc);
            return true;
		}
		private bool noteEffect7(ModuleMixerChannel mc)             // Tremolo
		{
            mc.tremoloCount = (mc.tremoloType <= 0x03) ? 0 : mc.tremoloCount;
            mc.tremoloStart = mc.channelVolume;
			mc.tremoloFreq	= (mc.effectArgX != 0) ? mc.effectArgX : 0;
			mc.tremoloAmp	= (mc.effectArgY != 0) ? mc.effectArgY : 0;
            return true;
		}
		private bool noteEffect8(ModuleMixerChannel mc)             // Not Used
		{
			return true;
		}
		private bool noteEffect9(ModuleMixerChannel mc)             // Set Sample Offset
		{
            mc.instrumentPosition = mc.effectArg << 8;

			if ((mc.instrumentPosition >= mc.instrumentLength) && (!mc.instrumentLoopStart) && (mc.loopType == ModuleConst.LOOP_ON))
				mc.instrumentLoopStart = true;

			if ((mc.instrumentPosition >= mc.instrumentRepeatStop) && (mc.instrumentLoopStart))
				mc.instrumentPosition = mc.instrumentRepeatStart;

			return true;
		}
		private bool noteEffectA(ModuleMixerChannel mc)             // Volume Slide
		{
			mc.volumeSlideStep = 0;
			if (mc.effectArgX != 0)
                mc.volumeSlideStep = (float)mc.effectArgX / 0x40;   // Volume Slide up
			else if (mc.effectArgY != 0)
				mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;  // Volume Slide down
			mc.channelVolume -= mc.volumeSlideStep;
			return true;
		}
		private bool noteEffectB(ModuleMixerChannel mc)             // Position Jump
		{
            mc.patternNumToJump = mc.effectArgX * 16 + mc.effectArgY;
            if (mc.patternNumToJump > 0x7F) mc.patternNumToJump = 0;

            currentRow = 0;
            track = module.arrangement[mc.patternNumToJump];
            pattern = module.patterns[module.arrangement[track]];
			return true;
		}
		private bool noteEffectC(ModuleMixerChannel mc)             // Set Volume
		{
            mc.channelVolume = (float)mc.effectArg / 0x40;
            mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
            return true;
		}
		private bool noteEffectD(ModuleMixerChannel mc)             // Pattern Break
		{
            mc.positionToJump = mc.effectArgX * 10 + mc.effectArgY;
            if (mc.positionToJump > 0x3F) mc.positionToJump = 0;

            track++;
            pattern = module.patterns[module.arrangement[track]];
            currentRow = mc.positionToJump;

            return true;
		}
		private bool noteEffectE(ModuleMixerChannel mc)             // Extended Effects
		{
			effectsE[mc.effectArgX](mc);
			effectsEUsed[mc.effectArgX] = true;
			return true;
		}
		private bool noteEffectF(ModuleMixerChannel mc)             // SetSpeed
		{
            if ((mc.effectArg >= 0x20) && (mc.effectArg <= 0xFF))
            {
                BPM = mc.effectArg;
                setBPM();
            }
            if ((mc.effectArg > 0) && (mc.effectArg <= 0x1F))
                speed = mc.effectArg;

			//System.Diagnostics.Debug.WriteLine("Set BPM/speed -> " + BPM + " : " + speed);

			return true;
		}
		//----------------------------------------------------------------------------------------------
		private bool tickEffect0(ModuleMixerChannel mc)
		{
            if (mc.effectArg == 0) return false;
            int arpeggioPeriod = mc.arpeggioPeriod0;
			arpeggioPeriod = (mc.arpeggioCount == 1) ? mc.arpeggioPeriod1 : arpeggioPeriod;
			arpeggioPeriod = (mc.arpeggioCount == 2) ? mc.arpeggioPeriod2 : arpeggioPeriod;
			//mc.period = arpeggioPeriod;
			mc.periodInc = calcPeriodIncrement(arpeggioPeriod);
			mc.arpeggioCount = (mc.arpeggioCount + 1) % 4;
            return true;
		}
		private bool tickEffect1(ModuleMixerChannel mc)
		{
			mc.period = mc.portamentoStart;
			mc.periodInc = calcPeriodIncrement(mc.period);

			mc.portamentoStart += mc.portamentoStep;
			mc.portamentoStart = (mc.portamentoStart <= ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.portamentoStart;
			return true;
		}
		private bool tickEffect2(ModuleMixerChannel mc)
		{
			mc.period = mc.portamentoStart;
			mc.periodInc = calcPeriodIncrement(mc.period);

			mc.portamentoStart += mc.portamentoStep;
			mc.portamentoStart = (mc.portamentoStart >= ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.portamentoStart;
			return true;
		}
		private bool tickEffect3(ModuleMixerChannel mc)
		{
			
			mc.period = mc.portamentoStart;
            mc.periodInc = calcPeriodIncrement(mc.period);

            mc.portamentoStart += mc.portamentoStep;
			mc.portamentoStart = (mc.portamentoStart <= ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.portamentoStart;
			mc.portamentoStart = (mc.portamentoStart >= ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.portamentoStart;

            if (mc.portamentoStep < 0)
                mc.portamentoStart = (mc.portamentoStart > mc.portamentoEnd) ? mc.portamentoStart : mc.portamentoEnd;
            else
                mc.portamentoStart = (mc.portamentoStart < mc.portamentoEnd) ? mc.portamentoStart : mc.portamentoEnd;

            return true;
		}
		private bool tickEffect4(ModuleMixerChannel mc)
		{
			mc.vibratoAdd = (mc.vibratoType % 4 == 0) ? ModuleConst.ModSinusTable	[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 1) ? ModuleConst.ModRampDownTable[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 2) ? ModuleConst.ModSquareTable	[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 3) ? ModuleConst.ModRandomTable	[mc.vibratoCount] : mc.vibratoAdd;

			mc.periodInc     = calcPeriodIncrement(mc.vibratoPeriod);
            mc.vibratoPeriod = mc.vibratoStart + (int)(mc.vibratoAdd * ((float)mc.vibratoAmp / 128.0f));
			mc.vibratoPeriod = (mc.vibratoPeriod <= ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.vibratoPeriod;
			mc.vibratoPeriod = (mc.vibratoPeriod >= ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.vibratoPeriod;
            mc.vibratoCount  = (mc.vibratoCount + mc.vibratoFreq) & 0x3F;
            return true;
		}
		private bool tickEffect5(ModuleMixerChannel mc)
		{
			tickEffectA(mc);
			tickEffect3(mc);
			return true;
		}
		private bool tickEffect6(ModuleMixerChannel mc)
		{
			tickEffectA(mc);
			tickEffect4(mc);
            return true;
		}
		private bool tickEffect7(ModuleMixerChannel mc)
		{
			mc.tremoloAdd = (mc.tremoloType % 4 == 0) ? ModuleConst.ModSinusTable	 [mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 1) ? ModuleConst.ModRampDownTable [mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 2) ? ModuleConst.ModSquareTable	 [mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 3) ? ModuleConst.ModRandomTable	 [mc.tremoloCount] : mc.tremoloAdd;

            mc.channelVolume = mc.tremoloStart + ((float)mc.tremoloAdd / 64) * ((float)mc.tremoloAmp / 0x40);
            mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
            mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
            mc.tremoloCount	 = (mc.tremoloCount + mc.tremoloFreq) & 0x3F;
            return true;
		}
		private bool tickEffect8(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool tickEffect9(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectA(ModuleMixerChannel mc)             // Volume Slide tick
		{
            mc.channelVolume += mc.volumeSlideStep;
            mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
            mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
            return true;
		}
		private bool tickEffectB(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectC(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectD(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectE(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectF(ModuleMixerChannel mc)
		{
			return true;
		}
		//----------------------------------------------------------------------------------------------
		private bool effectE0(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool effectE1(ModuleMixerChannel mc)
		{
            mc.period -= mc.effectArgY;
            mc.period = (mc.period < ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.period;
            mc.periodInc = calcPeriodIncrement(mc.period);
            return true;
		}
		private bool effectE2(ModuleMixerChannel mc)
		{
            mc.period += mc.effectArgY;
            mc.period = (mc.period > ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.period;
            mc.periodInc = calcPeriodIncrement(mc.period);
            return true;
		}
		private bool effectE3(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool effectE4(ModuleMixerChannel mc)
		{
			mc.vibratoType = mc.effectArgY & 0x7;
			return true;
		}
		private bool effectE5(ModuleMixerChannel mc)
		{
			mc.currentFineTune = mc.effectArgY;
			return true;
		}
		private bool effectE6(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool effectE7(ModuleMixerChannel mc)
		{
			mc.tremoloType = mc.effectArgY & 0x7;			
			return true;
		}
		private bool effectE8(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool effectE9(ModuleMixerChannel mc)                // Retrigger Sample
		{
			return true;
		}
		private bool effectEA(ModuleMixerChannel mc)                // Fine Volume Slide Up
		{
			mc.effect = 0x0A;
			mc.volumeSlideStep = (float)mc.effectArgY / 0x40;
			return true;
		}
		private bool effectEB(ModuleMixerChannel mc)                // Fine Volume Slide Down
		{
			mc.effect = 0x0A;
			mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;
			return true;
		}
		private bool effectEC(ModuleMixerChannel mc)                // Cut Sample
		{
			return true;
		}
		private bool effectED(ModuleMixerChannel mc)
		{
			return true;
		}
		private bool effectEE(ModuleMixerChannel mc)                // Delay
		{
            patternDelay = mc.effectArgY;
            return true;
		}
		private bool effectEF(ModuleMixerChannel mc)
		{
			return true;
		}
		//----------------------------------------------------------------------------------------------
		public MODMixer(SoundModule module) : base(module)
        {
			/*
			for (int i = 0; i < 16; i++)
            {
				noteEffects.Add(noEffect);
				tickEffects.Add(noEffect);
				effectsE.Add(noEffect);
			}
			return; // without effects;
			*/

			noteEffects.Add(noteEffect0);				// 0. ARPEGGIO
			noteEffects.Add(noteEffect1);				// 1. PORTAMENTO UP 
			noteEffects.Add(noteEffect2);				// 2. PORTAMENTO DOWN
			noteEffects.Add(noteEffect3);				// 3. TONE PORTAMENTO
			noteEffects.Add(noteEffect4);				// 4. VIBRATO
			noteEffects.Add(noteEffect5);				// 5. TONE PORTAMENTO + VOLUME SLIDE
			noteEffects.Add(noteEffect6);				// 6. VIBRATO + VOLUME SLIDE
			noteEffects.Add(noteEffect7);				// 7. TREMOLO
			noteEffects.Add(/*noteEffect8*/noEffect);   // 8. PAN
			noteEffects.Add(/*noteEffect9*/noEffect);   // 9. SAMPLE OFFSET
			noteEffects.Add(noteEffectA);				// A. VOLUME SLIDE
			noteEffects.Add(/*noteEffectB*/noEffect);   // B. POSITION JUMP
			noteEffects.Add(noteEffectC);				// C. SET VOLUME
			noteEffects.Add(noteEffectD);				// D. PATTERN BREAK
			noteEffects.Add(noteEffectE);				// E. EXTEND EFFECTS
			noteEffects.Add(noteEffectF);				// F. SET SPEED

			tickEffects.Add(tickEffect0);				// tick effect 0. ARPEGGIO
			tickEffects.Add(tickEffect1);				// tick effect 1. PORTAMENTO UP 
			tickEffects.Add(tickEffect2);				// tick effect 2. PORTAMENTO DOWN
			tickEffects.Add(tickEffect3);				// tick effect 3. TONE PORTAMENTO
			tickEffects.Add(tickEffect4);				// tick effect 4. VIBRATO
			tickEffects.Add(tickEffect5);				// tick effect 5. TONE PORTAMENTO + VOLUME SLIDE
			tickEffects.Add(tickEffect6);				// tick effect 6. VIBRATO + VOLUME SLIDE
			tickEffects.Add(tickEffect7);				// tick effect 7. TREMOLO
			tickEffects.Add(/*tickEffect8*/noEffect);	// tick effect 8. PAN
			tickEffects.Add(/*tickEffect9*/noEffect);	// tick effect 9. SAMPLE OFFSET
			tickEffects.Add(tickEffectA);				// tick effect A. VOLUME SLIDE
			tickEffects.Add(/*tickEffectB*/noEffect);	// tick effect B. POSITION JUMP
			tickEffects.Add(/*tickEffectC*/noEffect);	// tick effect C. SET VOLUME
			tickEffects.Add(/*tickEffectD*/noEffect);	// tick effect D. PATTERN BREAK
			tickEffects.Add(/*tickEffectE*/noEffect);	// tick effect E. EXTEND EFFECTS
			tickEffects.Add(/*tickEffectF*/noEffect);	// tick effect F. SET SPEED

			effectsE.Add(/*effectE0*/noEffect);         // E0. SET FILTER
			effectsE.Add(/*effectE1*/noEffect);         // E1. FINE PORTAMENTO UP
			effectsE.Add(/*effectE2*/noEffect);         // E2. FINE PORTAMENTO DOWN
			effectsE.Add(/*effectE3*/noEffect);         // E3. GLISSANDO CONTROL
			effectsE.Add(/*effectE4*/noEffect);         // E4. VIBRATO WAVEFORM 
			effectsE.Add(/*effectE5*/noEffect);         // E5. SET FINETUNE
			effectsE.Add(/*effectE6*/noEffect);         // E6. PATTERN LOOP
			effectsE.Add(/*effectE7*/noEffect);         // E7. TREMOLO WAVEFORM
			effectsE.Add(/*effectE8*/noEffect);         // E8. 16 POSITION PAN
			effectsE.Add(/*effectE9*/noEffect);         // E9. RETRIG NOTE
			effectsE.Add(effectEA);						// EA. FINE VOLUME SLIDE UP
			effectsE.Add(effectEB);						// EB. FINE VOLUME SLIDE DOWN
			effectsE.Add(/*effectEC*/noEffect);         // EC. CUT NOTE
			effectsE.Add(/*effectED*/noEffect);         // ED. NOTE DELAY
			effectsE.Add(/*effectEE*/noEffect);         // EE. PATTERN DELAY
			effectsE.Add(/*effectEF*/noEffect);         // EF. INVERT LOOP
		}
	}
	public class MODSoundModule : SoundModule
    {
		private MODMixer mixer		= null;
		public MODSoundModule():base("MOD format")
		{
			DebugMes("MOD Sound Module created");			
		}
		private long getAllInstrumentsLength()
		{
			long allSamplesLength = 0;
			foreach (ModuleInstrument inst in instruments) allSamplesLength += inst.length;
			return allSamplesLength;
		}
		public string InstrumentsToString()
		{
			string res = "Samples info : \n";
			int i = 0;
			foreach (ModuleInstrument inst in instruments)	res += ModuleUtils.getAsHex(i++, 2) + ':' + inst.ToString();
			return res;
		}
		private void readInstruments(Stream stream)
		{
			foreach (ModuleInstrument inst in instruments) inst.Clear();				
			instruments.Clear();
			for (int i = 0; i < nSamples; i++)
			{
				ModuleInstrument inst = new ModuleInstrument();
				inst.number = i;
				instruments.Add(inst);
				inst.readInstrumentHeader(stream);
				DebugMes(inst.ToString());
			}
		}
		private void readArrangement(Stream stream)
		{
			songLength = stream.ReadByte(); // count of pattern in arrangement
			stream.ReadByte();              // skip, ood old CIAA

			arrangement.Clear();
			// always space for 128 pattern...
			for (int i = 0; i < 128; i++) arrangement.Add(stream.ReadByte());

		}
		private int calcPattCount()
		{
			int headerLen = 150; // Name+SongLen+CIAA+SongArrangement
			if (nSamples > 15) headerLen += 4;  // Kennung
						
			int sampleLen = 0;
			foreach (ModuleInstrument inst in instruments) sampleLen += 30 + inst.length;
			
			int spaceForPattern = (int)(fileLength - headerLen - sampleLen);
		
			// Lets find out about the highest Patternnumber used
			// in the song arrangement
			int maxPatternNumber = 0;
			for (int i = 0; i < songLength; i++)
			{
				int patternNumber = arrangement[i];
				if (patternNumber > maxPatternNumber && patternNumber < 0x80)
					maxPatternNumber = arrangement[i];
			}
			maxPatternNumber++; // Highest number becomes highest count 

			// It could be the WOW-Format:
			if (modID == "M.K.")
			{
				// so check for 8 channels:
				int totalPatternBytes = maxPatternNumber * 2048; //64*4*8
				// This mod has 8 channels! --> WOW
				if (totalPatternBytes == spaceForPattern)
				{
					isAmigaLike = true;
					nChannels = 8;
					trackerName = "Grave Composer";
				}
			}

			int bytesPerPattern = 256 * nChannels; //64*4*nChannels
			nPatterns = (int)(spaceForPattern / bytesPerPattern);
			int bytesLeft = (int)(spaceForPattern % bytesPerPattern);

			if (bytesLeft > 0) // It does not fit!
			{
				if (maxPatternNumber > nPatterns)
				{
					// The modfile is too short. The highest pattern is reaching into
					// the sampledata, but it has to be read!
					bytesLeft -= bytesPerPattern;
					nPatterns = maxPatternNumber + 1;
				}
				else
				{
					// The modfile is too long. Sometimes this happens if composer
					// add additional data to the modfile.
					bytesLeft += (nPatterns - maxPatternNumber) * bytesPerPattern;
					nPatterns = maxPatternNumber;
				}
				return bytesLeft;
			}
			
			return 0;
		}
		private void readPatterns(Stream stream)
		{
			foreach (MODPattern pat in patterns) pat.Clear();
			patterns.Clear();
			if (nSamples > 15) stream.Seek(4, SeekOrigin.Current);	// skip ModID, if not NoiseTracker:
																	
			bytesLeft = calcPattCount();
            for (int i = 0; i < nPatterns; i++)						// Read the patterndata
			{
                MODPattern pattern = new MODPattern();
                pattern.readPatternData(stream, modID, nChannels, nSamples);
                patterns.Add(pattern);
				DebugMes("Patern : " + i + pattern.ToString());
            }
        }
		private void readInstrumentsData(Stream stream)
		{
			// Sampledata: If the modfile was too short, we need to recalculate:
			if (bytesLeft < 0)
			{
				long calcSamplePos = getAllInstrumentsLength();
				calcSamplePos = fileLength - calcSamplePos;
				// do this only, if needed!
				if (calcSamplePos < stream.Position) stream.Seek(calcSamplePos, SeekOrigin.Begin);
			}
			for (int i = 0; i < nSamples; i++) instruments[i].readInstrumentData(stream);
		}
		public override bool readFromStream(Stream stream)
		{
			fileLength = stream.Length;
			baseVolume = 1.0f;
			BPMSpeed = 125;
			tempo = 6;

			if (!checkFormat(stream)) return false;
			setModType();

			stream.Seek(0, SeekOrigin.Begin);
			songName = ModuleUtils.readString0(stream, 20);
			nInstruments = nSamples;
			readInstruments(stream);		// read instruments		
			readArrangement(stream);		// read pattern order	
			readPatterns(stream);			// read patterns
			readInstrumentsData(stream);    // read samples data

			//mixer = new cMODMixer(this);
			//mixer.mixInfo.BPM = BPMSpeed;
			//mixer.mixInfo.speed = tempo;
			
			mixer = new MODMixer(this);
			//mixer.mixModule();
			//mixer.mixPattern(1);
			return true;
		}
		private void setModType()
		{
			isAmigaLike = false;
			nSamples = 31;
			trackerName = "StarTrekker";

			if (modID == "M.K." || modID == "M!K!" || modID == "M&K!" || modID == "N.T.")
			{
				isAmigaLike = true; 
				nChannels = 4;
				trackerName = "ProTracker";
			}
			
			if (modID.Substring(0, 3) == "FLT" || modID.Substring(0, 3) == "TDZ")
				nChannels = int.Parse(modID.Substring(3, 1));

			if (modID.Substring(1, 3) == "CHN")
				nChannels = int.Parse(modID.Substring(0, 1));

			if (modID == "CD81" || modID == "OKTA")
			{
				nChannels = 8;
				trackerName = "Atari Oktalyzer";
			}

			if (modID.Substring(2, 2) == "CH" || modID.Substring(2, 2) == "CN")
			{
				nChannels = int.Parse(modID.Substring(0, 2));
				trackerName = "TakeTracker";
			}	 
		}
		public override bool checkFormat(Stream stream)
		{
			byte[] bytesID = new byte[4];
			stream.Seek(1080, SeekOrigin.Begin);
			stream.Read(bytesID);
			modID = Encoding.ASCII.GetString(bytesID);
            return modID == "M.K." ||
                   modID == "M!K!" ||
                   modID == "M&K!" ||
                   modID == "N.T." ||
                   modID == "CD81" ||
                   modID == "OKTA" ||
                   modID.Substring(0, 3) == "FLT" ||
                   modID.Substring(0, 3) == "TDZ" ||
                   modID.Substring(1, 3) == "CHN" ||
                   modID.Substring(2, 2) == "CH"  ||
				   modID.Substring(2, 2) == "CN";
		}
		public override string ToString()
		{
			string modInfo = "";
			modInfo += "Mod Length " + this.fileLength + "\n";
			modInfo += "Mod with " + nSamples + " samples and " + nChannels + " channels using\n";
			modInfo += "Tracker : " + trackerName + " modID : " + modID + "\n";
			modInfo += (isAmigaLike) ? "Protracker" : "Fast Tracker log";
			modInfo += " frequency table\n";
			modInfo += "Song named : " + songName + "\n";
			modInfo += "SongLength : " + songLength + "\n";

			for (int i = 0; i < songLength; i++) modInfo += arrangement[i] + ((i < songLength - 1) ? "," : "");

			modInfo += "\n\n";
			return modInfo;
		}
		public override void PlayInstrument(int num)
        {
			if (num < 0 || num >= nSamples) return;

			soundSystem.Stop();
			//DebugMes("Instruments count - " + instruments.Count);
			ModuleInstrument inst = instruments[num];
			
			uint samplesPerSecond = 44100;
			float baseFreq = /*3546895.0f / */(float)(inst.baseFrequency * 2);
			float frqMul = baseFreq / (float)(samplesPerSecond);
			DebugMes("BaseFreq = " + inst.baseFrequency + " Freq mull = " + frqMul);
			uint soundBufferLen = (uint)(inst.length / frqMul); 
			DebugMes("SoundBufferLen = " + soundBufferLen + " Instrument len = " + inst.length);

			BinaryWriter buffer = soundSystem.getBuffer;
			soundSystem.SetBufferLen(soundBufferLen);
			float fpos = 0;
			for (int i = 0; i < soundBufferLen; i++)
			{
				if (fpos < inst.length) buffer.Write((short)(inst.instrumentData[(int)(fpos)]));
				fpos += frqMul;
			}
			soundSystem.Play();
		}
		public override void Play()
        {
			mixer.playModule(0);
        }
		public override void Stop()
		{
			mixer.Stop();
		}
		public override void Dispose()
		{
			mixer.Stop();
		}
	}
}

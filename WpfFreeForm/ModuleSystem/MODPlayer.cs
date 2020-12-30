using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Media;

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

			if (channel.period > 0) channel.noteIndex = ModuleConst.getNoteIndexForPeriod(channel.period) + 1;

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
		private bool noteEffect0(ModuleMixerChannel mc)
		{
            if (mc.effectArg == 0) return false;
            mc.arpeggioCount = 0;
            mc.arpeggioX = mc.effectArgX;
            mc.arpeggioY = mc.effectArgY;
            mc.arpeggioIndex = mc.noteIndex;
			mc.arpeggioPeriod0 = ModuleConst.getNotePeriod(mc.noteIndex - 1, mc.currentFineTune);
			mc.arpeggioPeriod1 = mc.arpeggioPeriod0;
			mc.arpeggioPeriod2 = mc.arpeggioPeriod0;

			if (mc.noteIndex + mc.effectArgX < 60)
				mc.arpeggioPeriod1 = ModuleConst.getNotePeriod(mc.noteIndex + mc.effectArgX - 1, mc.currentFineTune);

			if (mc.noteIndex + mc.effectArgY < 60)
				mc.arpeggioPeriod2 = ModuleConst.getNotePeriod(mc.noteIndex + mc.effectArgY - 1, mc.currentFineTune);

			return true;
		}

		private bool noteEffect1(ModuleMixerChannel mc)
		{
            mc.portamentoStart = mc.period;
            mc.periodInc = calcPeriodIncrement(mc.period);
            mc.portamentoStep = mc.effectArg;
            return true;
		}

		private bool noteEffect2(ModuleMixerChannel mc)
		{
            mc.portamentoStart = mc.period;
            mc.periodInc = calcPeriodIncrement(mc.period);
            mc.portamentoStep = mc.effectArg;
            return true;
		}

		private bool noteEffect3(ModuleMixerChannel mc)
		{
            if (!mc.noNote)
            {
                mc.portamentoStart = mc.lastPeriod;
                mc.portamentoEnd = mc.period;
            }
            if (mc.effectArg != 0) mc.portamentoStep = (mc.portamentoStart <= mc.portamentoEnd) ? mc.effectArg : -mc.effectArg;
            mc.periodInc = calcPeriodIncrement(mc.portamentoStart);
            return true;
		}

		private bool noteEffect4(ModuleMixerChannel mc)
		{
            if ((mc.vibratoType <= 0x03) && (mc.effectArg != 0)) mc.vibratoCount = 0;
            mc.vibratoStart = mc.period;
            if (mc.effectArgX != 0 && mc.effectArgY != 0)
            {
                mc.vibratoFreq = mc.effectArgX;
                mc.vibratoAmp = mc.effectArgY;
            }
            return true;
		}

		private bool noteEffect5(ModuleMixerChannel mc)
		{
            noteEffectA(mc);
            if (!mc.noNote)
            {
                mc.portamentoStart = mc.lastPeriod;
                mc.portamentoEnd = mc.period;
            }
            mc.periodInc = calcPeriodIncrement(mc.portamentoStart);
            return true;
		}

		private bool noteEffect6(ModuleMixerChannel mc)
		{
            noteEffectA(mc);
            if (mc.vibratoType <= 0x03) mc.vibratoCount = 0;
            mc.vibratoStart = mc.period;
            return true;
		}

		private bool noteEffect7(ModuleMixerChannel mc)
		{
            //if (mc.tremoloType <= 0x03) mc.tremoloCount = 0;
            //mc.tremoloStart = mc.channelVolume;
            //if (mc.effectArgX != 0) mc.vibratoFreq = mc.effectArgX;
            //if (mc.effectArgY != 0) mc.vibratoAmp = mc.effectArgY;
            return true;
		}

		private bool noteEffect8(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool noteEffect9(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool noteEffectA(ModuleMixerChannel mc)
		{
            mc.volumeSlideStart = false;
            if (mc.effectArgX != 0)
            {
                mc.volumeSlideX = (float)mc.effectArgX / 0x40;
                mc.volumeSlideY = 0;
            }

            if (mc.effectArgY != 0)
            {
                mc.volumeSlideX = 0;
                mc.volumeSlideY = (float)mc.effectArgY / 0x40;
            }
            /*
			mc.channelVolume += mc.volumeSlideX * (mixInfo.speed - 1);
			mc.channelVolume -= mc.volumeSlideY * (mixInfo.speed - 1);

			mc.channelVolume = (mc.channelVolume < 0.0) ? 0.0 : mc.channelVolume;
			mc.channelVolume = (mc.channelVolume > 1.0) ? 1.0 : mc.channelVolume;			
			*/

            return true;
		}

		private bool noteEffectB(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool noteEffectC(ModuleMixerChannel mc)
        {
            mc.channelVolume = (float)mc.effectArg / 0x40;
            mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
            return true;
		}

		private bool noteEffectD(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool noteEffectE(ModuleMixerChannel mc)
		{
			effectsE[mc.effectArgX](mc);
			return true;
		}

		private bool noteEffectF(ModuleMixerChannel mc)
		{
            if ((mc.effectArg >= 0x20) && (mc.effectArg <= 0xFF))
            {
                BPM = mc.effectArg;
                setBPM();
            }
            else if ((mc.effectArg > 0) && (mc.effectArg <= 0x1F))
                speed = mc.effectArg;

            return true;
		}

		//----------------------------------------------------------------------------------------------
		private bool tickEffect0(ModuleMixerChannel mc)
		{
            if (mc.effectArg == 0) return false;
            int arpeggioPeriod = 0;
            if (mc.arpeggioCount == 0) arpeggioPeriod = mc.arpeggioPeriod0;
            if (mc.arpeggioCount == 1) arpeggioPeriod = mc.arpeggioPeriod1;
			if (mc.arpeggioCount == 2) arpeggioPeriod = mc.arpeggioPeriod2;
            //if (mc.arpeggioIndex != 0) 
			mc.periodInc = calcPeriodIncrement(arpeggioPeriod);
            mc.arpeggioCount = (mc.arpeggioCount + 1) % 3;
            return true;
		}

		private bool tickEffect1(ModuleMixerChannel mc)
		{
            mc.period -= mc.portamentoStep;
            if (mc.period < 113) mc.period = 113;
            mc.periodInc = calcPeriodIncrement(mc.period);
            return true;
		}

		private bool tickEffect2(ModuleMixerChannel mc)
		{
            mc.period += mc.portamentoStep;
            if (mc.period > 856) mc.period = 856;
            mc.periodInc = calcPeriodIncrement(mc.period);
            return true;
		}

		private bool tickEffect3(ModuleMixerChannel mc)
		{
            if (mc.portamentoStart < 113) mc.portamentoStart = 113;
            if (mc.portamentoStart > 856) mc.portamentoStart = 856;

            mc.periodInc = calcPeriodIncrement(mc.portamentoStart);
            mc.period = mc.portamentoStart;

			mc.portamentoStart += mc.portamentoStep;

			if (Math.Abs(mc.portamentoEnd - mc.portamentoStart) < Math.Abs(mc.portamentoStep))
                mc.portamentoStart = mc.portamentoEnd;
            return true;
		}

		private bool tickEffect4(ModuleMixerChannel mc)
		{
            if ((mc.vibratoAmp > 0) && (mc.vibratoFreq > 0))
            {
                if (mc.vibratoType % 4 == 0) mc.vibratoAdd = ModuleConst.ModSinusTable[mc.vibratoCount];
                if (mc.vibratoType % 4 == 1) mc.vibratoAdd = ModuleConst.ModRampDownTable[mc.vibratoCount];
                if (mc.vibratoType % 4 == 2) mc.vibratoAdd = ModuleConst.ModSquareTable[mc.vibratoCount];
                if (mc.vibratoType % 4 == 3) mc.vibratoAdd = ModuleConst.ModRandomTable[mc.vibratoCount];
            }
            var period = mc.vibratoStart + (int)(mc.vibratoAdd * mc.vibratoAmp / 128);
            if (period < 113) period = 113;
            if (period > 856) period = 856;

            mc.periodInc = calcPeriodIncrement(period);
            mc.vibratoCount = (mc.vibratoCount + mc.vibratoFreq) & 0x3F;
            return true;
		}

		private bool tickEffect5(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool tickEffect6(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool tickEffect7(ModuleMixerChannel mc)
		{
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

		private bool tickEffectA(ModuleMixerChannel mc)
		{
            if (mc.volumeSlideStart)
            {
                mc.channelVolume += mc.volumeSlideX;
                mc.channelVolume -= mc.volumeSlideY;
                mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
                mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
            }
            else mc.volumeSlideStart = true;

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
			return true;
		}

		private bool effectE2(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectE3(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectE4(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectE5(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectE6(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectE7(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectE8(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectE9(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectEA(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectEB(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectEC(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectED(ModuleMixerChannel mc)
		{
			return true;
		}

		private bool effectEE(ModuleMixerChannel mc)
		{
            patternDelay = mc.effectArgY;
            mc.effect = mc.lastEffect;
            mc.effectArg = mc.lastEffectArg;
            mc.effectArgX = mc.lastEffectArgX;
            mc.effectArgY = mc.lastEffectArgY;
            return true;
		}

		private bool effectEF(ModuleMixerChannel mc)
		{
			return true;
		}

		//----------------------------------------------------------------------------------------------
		public MODMixer(SoundModule module) : base(module)
        {
			noteEffects.Add(noteEffect0); noteEffects.Add(noteEffect1); noteEffects.Add(noteEffect2);
			noteEffects.Add(noteEffect3); noteEffects.Add(noteEffect4); noteEffects.Add(noteEffect5);
			noteEffects.Add(noteEffect6); noteEffects.Add(noteEffect7); noteEffects.Add(noteEffect8);
			noteEffects.Add(noteEffect9); noteEffects.Add(noteEffectA); noteEffects.Add(noteEffectB);
			noteEffects.Add(noteEffectC); noteEffects.Add(noteEffectD); noteEffects.Add(noteEffectE);
			noteEffects.Add(noteEffectF);

			tickEffects.Add(tickEffect0); tickEffects.Add(tickEffect1); tickEffects.Add(tickEffect2);
			tickEffects.Add(tickEffect3); tickEffects.Add(tickEffect4); tickEffects.Add(tickEffect5);
			tickEffects.Add(tickEffect6); tickEffects.Add(tickEffect7); tickEffects.Add(tickEffect8);
			tickEffects.Add(tickEffect9); tickEffects.Add(tickEffectA); tickEffects.Add(tickEffectB);
			tickEffects.Add(tickEffectC); tickEffects.Add(tickEffectD); tickEffects.Add(tickEffectE);
			tickEffects.Add(tickEffectF);

			effectsE.Add(effectE0); effectsE.Add(effectE1); effectsE.Add(effectE2);
			effectsE.Add(effectE3); effectsE.Add(effectE4); effectsE.Add(effectE5);
			effectsE.Add(effectE6); effectsE.Add(effectE7); effectsE.Add(effectE8);
			effectsE.Add(effectE9); effectsE.Add(effectEA); effectsE.Add(effectEB);
			effectsE.Add(effectEC); effectsE.Add(effectED); effectsE.Add(effectEE);
			effectsE.Add(effectEF);
		}
	}

	public class MODSoundModule : SoundModule
    {
		private MODMixer mixer		= null;

		public MODSoundModule():base("MOD format")
		{
			DebugMes("MOD Sound Module created");
			mixer = new MODMixer(this);
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
			baseVolume = 128;
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
			readInstrumentsData(stream);	// read samples data

			//mixer = new cMODMixer(this);
			//mixer.mixInfo.BPM = BPMSpeed;
			//mixer.mixInfo.speed = tempo;

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
			
			uint samplesPerSecond = 22050;
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
				if (fpos < inst.length) buffer.Write((short)(32767 * inst.instrumentData[(int)(fpos)]));
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
	}
}

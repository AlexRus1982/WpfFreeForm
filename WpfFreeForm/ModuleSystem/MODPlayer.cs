using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModuleSystem
{
	public class MODMixerChannel : ModuleMixerChannel
	{
		public MODPatternChannel patternChannel = null;
		public MODInstrument instrument			= null;
		public MODInstrument lastInstrument		= null;
	}
	public class MODInstrument
	{
		public string name					= "";	// Name of the sample
		public int number					= 0;	// Number of the sample
		public int length					= 0;	// full length (already *2 --> Mod-Fomat)
		public int fineTune					= 0;	// Finetuning -8..+8
		public float volume					= 0;	// Basisvolume
		public float repeatStart			= 0;	// # of the loop start (already *2 --> Mod-Fomat)
		public float repeatStop				= 0;	// # of the loop end   (already *2 --> Mod-Fomat)
		public int repeatLength				= 0;	// length of the loop
		public int loopType					= 0;	// 0: no Looping, 1: normal, 2: pingpong, 3: backwards
		public int baseFrequency			= 0;	// BaseFrequency

		public List<float> instrumentData	= new List<float>();	// The sampledata, already converted to 16 bit (always)
																	// 8Bit: -128 to 127; 16Bit: -32768..0..+32767
		public MODInstrument()
		{
		}
		public override string ToString()
		{
			///*
			//if (length == 0) return this.name;
			string res = this.name;
			res += "(len:" + length + ","
					+ "number:" + number + ","
					+ "fTune:" + fineTune + ","
					//+ "transpose:"  + transpose + ","
					+ "baseFreq:" + baseFrequency + ","
					+ "vol:" + volume + ","
					//+ "panning:" + panning + ","
					+ "repStart:" + repeatStart + ","
					+ "repLen:" + repeatLength + ","
					+ "repStop:" + repeatStop + ")";

			return res;
			//*/
			//return this.toShortString();
		}
		public string toShortString()
		{
			return this.name;
		}
		public void readInstrumentHeader(Stream stream)
		{
			name = ModuleUtils.readString0(stream, 22);
			length = ModuleUtils.readWord(stream) * 2; // Length

			fineTune = stream.ReadByte() & 0xF; // finetune Value>7 means negative 8..15= -8..-1
												//fineTune = (fine > 7) ? fine - 16 : fine;

			//baseFrequency = ModuleConst.getNoteFreq(24, fine);

			int vol = stream.ReadByte(); // volume 64 is maximum
			volume = (vol > 64) ? 1.0f : (float)vol / 64.0f;

			//// Repeat start and stop
			repeatStart = ModuleUtils.readWord(stream) * 2;
			repeatLength = ModuleUtils.readWord(stream) * 2;
			repeatStop = repeatStart + repeatLength;

			if (length < 4) 
				length = 0;

			if (length > 0)
			{
				if (repeatStart > length)
					repeatStart = length;

				if (repeatStop > length)
					repeatStop = length;

				if (repeatStart >= repeatStop || repeatStop <= 8 || (repeatStop - repeatStart) <= 4)
				{
					repeatStart = repeatStop = 0;
					loopType = 0;
				}

				if (repeatStart < repeatStop)
					loopType = ModuleConst.LOOP_ON;
			}
			else
				loopType = 0;
			
			repeatLength = (int)(repeatStop - repeatStart);
		}
		public void readInstrumentData(Stream stream)
		{
			instrumentData.Clear();
			if (length > 0)
			{
				for (int s = 0; s < length; s++)
					instrumentData.Add((float)(ModuleUtils.readSignedByte(stream)) * 0.0078125f);  // 0.0078125f = 1/128
			}
		}
		public void Clear()
		{
			instrumentData.Clear();
		}
	}
	public class MODPatternChannel
	{
		public int period			= 0;
		public int noteIndex		= 0;
		public int instrument		= 0;
		public int effekt			= 0;
		public int effektOp			= 0;
		public int volumeEffekt		= 0;
		public int volumeEffektOp	= 0;
		public override string ToString()
		{
			string res = ModuleConst.getNoteNameToIndex(noteIndex);
			res += ((period == 0 && noteIndex != 0) || (period != 0 && noteIndex == 0)) ? "!" : " ";
			res += (instrument != 0) ? ModuleUtils.getAsHex(instrument, 2) : "..";
			res += " ";
			res += ((effekt != 0) || (effektOp != 0)) ? ModuleUtils.getAsHex(effekt, 1) + ModuleUtils.getAsHex(effektOp, 2) : "...";
			return res;
		}
	}
	public class MODPatternRow
	{
		public List<MODPatternChannel> patternChannels = new List<MODPatternChannel>();
		public override string ToString()
		{
			string res = "";
			for (int i = 0; i < patternChannels.Count; i++)
				res += patternChannels[i].ToString() + '|';
			return res;
		}
		public void Clear()
		{
			patternChannels.Clear();
		}
	}
	public class MODPattern
	{
		public List<MODPatternRow> patternRows = new List<MODPatternRow>();
		private MODPatternChannel createNewPatternChannel(Stream stream, int nSamples)
		{
			MODPatternChannel channel = new MODPatternChannel();
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
					MODPatternRow pRow = new MODPatternRow();
					for (int channel = 0; channel < 4; channel++)
					{
						pRow.patternChannels.Add(createNewPatternChannel(stream, nSamples));
					}
					for (int channel = 4; channel < 8; channel++) 
						pRow.patternChannels.Add(new MODPatternChannel());
					
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
					MODPatternRow pRow = new MODPatternRow();
					for (int channel = 0; channel < nChannels; channel++)
						pRow.patternChannels.Add(createNewPatternChannel(stream, nSamples));

					patternRows.Add(pRow);
				}
			}
		}
		public override string ToString()
		{
			string res = "\n";
			if (patternRows[0] != null)
			{
				string ln = "====";
				for (int i = 0; i < patternRows[0].patternChannels.Count; i++)
					ln += "===========";

				res += ln + "\n";

				for (int i = 0; i < patternRows.Count; i++)
					res += ModuleUtils.getAsHex(i, 2) + ":|" + patternRows[i] + "\n";

				res += ln + "\n";
			}
			else res += "empty pattern\n";
			return res;
		}
		public void Clear()
		{
			patternRows.Clear();
		}
	}
	public class MODModule : Module
    {
		public List<MODInstrument> instruments	= new List<MODInstrument>();
		public List<MODPattern> patterns		= new List<MODPattern>();
		public List<int> arrangement			= new List<int>();

		private int bytesLeft					= 0;
		private MODMixer mixer					= null;
		public MODModule():base("MOD format")
		{
			DebugMes("MOD Sound Module created");			
		}
		private long getAllInstrumentsLength()
		{
			long allSamplesLength = 0;
			foreach (MODInstrument inst in instruments) allSamplesLength += inst.length;
			return allSamplesLength;
		}
		public string InstrumentsToString()
		{
			string res = "Samples info : \n";
			int i = 0;
			foreach (MODInstrument inst in instruments)	
				res += ModuleUtils.getAsHex(i++, 2) + ':' + inst.ToString() + "\n";
			return res;
		}
		private void readInstruments(Stream stream)
		{
			foreach (MODInstrument inst in instruments) inst.Clear();				
			instruments.Clear();
			for (int i = 0; i < nSamples; i++)
			{
				MODInstrument inst = new MODInstrument();
				inst.number = i;
				instruments.Add(inst);
				inst.readInstrumentHeader(stream);
			}
			//DebugMes(InstrumentsToString());
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
			foreach (MODInstrument inst in instruments) 
				sampleLen += 30 + inst.length;
			
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
			foreach (MODPattern pat in patterns)
				pat.Clear();

			patterns.Clear();
			if (nSamples > 15)
				stream.Seek(4, SeekOrigin.Current);	// skip ModID, if not NoiseTracker:
																	
			bytesLeft = calcPattCount();
            for (int i = 0; i < nPatterns; i++)						// Read the patterndata
			{
                MODPattern pattern = new MODPattern();
                pattern.readPatternData(stream, modID, nChannels, nSamples);
                patterns.Add(pattern);
				//DebugMes("Patern : " + i + pattern.ToString());
            }
        }
		private void readInstrumentsData(Stream stream)
		{
			// Sampledata: If the modfile was too short, we need to recalculate:
			if (bytesLeft < 0)
			{
				long calcSamplePos = getAllInstrumentsLength();
				calcSamplePos = fileLength - calcSamplePos;
				if (calcSamplePos < stream.Position) stream.Seek(calcSamplePos, SeekOrigin.Begin); // do this only, if needed!
			}
			for (int i = 0; i < nSamples; i++) 
				instruments[i].readInstrumentData(stream);
		}
		public override bool readFromStream(Stream stream)
		{
			fileLength = stream.Length;
			baseVolume = 1.0f;
			BPM = 125;
			tempo = 6;

			if (!checkFormat(stream)) 
				return false;

			setModType();

			stream.Seek(0, SeekOrigin.Begin);
			songName = ModuleUtils.readString0(stream, 20);
			nInstruments = nSamples;
			readInstruments(stream);		// read instruments		
			readArrangement(stream);		// read pattern order	
			readPatterns(stream);			// read patterns
			readInstrumentsData(stream);    // read samples data

			mixer = new MODMixer(this);
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

			//soundSystem.Stop();
			////DebugMes("Instruments count - " + instruments.Count);
			//ModuleInstrument inst = instruments[num];
			
			//uint samplesPerSecond = 44100;
			//float baseFreq = /*3546895.0f / */(float)(inst.baseFrequency * 2);
			//float frqMul = baseFreq / (float)(samplesPerSecond);
			//DebugMes("BaseFreq = " + inst.baseFrequency + " Freq mull = " + frqMul);
			//uint soundBufferLen = (uint)(inst.length / frqMul); 
			//DebugMes("SoundBufferLen = " + soundBufferLen + " Instrument len = " + inst.length);

			//BinaryWriter buffer = soundSystem.getBuffer;
			//soundSystem.SetBufferLen(soundBufferLen);
			//float fpos = 0;
			//for (int i = 0; i < soundBufferLen; i++)
			//{
			//	if (fpos < inst.length) buffer.Write((short)(inst.instrumentData[(int)(fpos)]));
			//	fpos += frqMul;
			//}
			//soundSystem.Play();
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
	public class MODMixer : ModuleMixer
	{
		private MODModule module								= null;
		private MODPattern pattern								= null;
		private List<MODMixerChannel> mixChannels				= new List<MODMixerChannel>();

		private List<Func<MODMixerChannel, bool>> noteEffects	= new List<Func<MODMixerChannel, bool>>();
		private List<bool> noteEffectsUsed						= new List<bool>();
		private List<Func<MODMixerChannel, bool>> tickEffects	= new List<Func<MODMixerChannel, bool>>();
		private List<bool> tickEffectsUsed						= new List<bool>();
		private List<Func<MODMixerChannel, bool>> effectsE		= new List<Func<MODMixerChannel, bool>>();
		private List<bool> effectsEUsed							= new List<bool>();

		private WaveOutEvent waveOut							= null;
		private ModuleSoundStream waveStream					= null;
		private WaveFileReader waveReader						= null;
		private Task MixingTask									= null;

		#region Effects
		private bool noEffect(MODMixerChannel mc)               // no effect
		{
			return false;
		}
		private bool noteEffect0(MODMixerChannel mc)                // Arpeggio
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
		private bool noteEffect1(MODMixerChannel mc)                // Slide up (Portamento Up)
		{
			mc.portamentoStart = mc.period;
			mc.portamentoStep = -mc.effectArg;
			return true;
		}
		private bool noteEffect2(MODMixerChannel mc)             // Slide down (Portamento Down)
		{
			mc.portamentoStart = mc.period;
			mc.portamentoStep = mc.effectArg;
			return true;
		}
		private bool noteEffect3(MODMixerChannel mc)             // Slide to note
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
		private bool noteEffect4(MODMixerChannel mc)             // Vibrato
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
		private bool noteEffect5(MODMixerChannel mc)             // Continue Slide to note + Volume slide
		{
			noteEffectA(mc);
			return true;
		}
		private bool noteEffect6(MODMixerChannel mc)             // Continue Vibrato + Volume Slide
		{
			noteEffectA(mc);
			mc.effectArgX = 0;
			mc.effectArgY = 0;
			noteEffect4(mc);
			return true;
		}
		private bool noteEffect7(MODMixerChannel mc)             // Tremolo
		{
			mc.tremoloCount = (mc.tremoloType <= 0x03) ? 0 : mc.tremoloCount;
			mc.tremoloStart = mc.channelVolume;
			mc.tremoloFreq = (mc.effectArgX != 0) ? mc.effectArgX : 0;
			mc.tremoloAmp = (mc.effectArgY != 0) ? mc.effectArgY : 0;
			return true;
		}
		private bool noteEffect8(MODMixerChannel mc)             // Not Used
		{
			return true;
		}
		private bool noteEffect9(MODMixerChannel mc)             // Set Sample Offset
		{
			mc.instrumentPosition = mc.effectArg << 8;

			if ((mc.instrumentPosition >= mc.instrumentLength) && (!mc.instrumentLoopStart) && (mc.loopType == ModuleConst.LOOP_ON))
				mc.instrumentLoopStart = true;

			if ((mc.instrumentPosition >= mc.instrumentRepeatStop) && (mc.instrumentLoopStart))
				mc.instrumentPosition = mc.instrumentRepeatStart;

			return true;
		}
		private bool noteEffectA(MODMixerChannel mc)             // Volume Slide
		{
			mc.volumeSlideStep = 0;
			if (mc.effectArgX != 0)
				mc.volumeSlideStep = (float)mc.effectArgX / 0x40;   // Volume Slide up
			else if (mc.effectArgY != 0)
				mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;  // Volume Slide down
			mc.channelVolume -= mc.volumeSlideStep;
			return true;
		}
		private bool noteEffectB(MODMixerChannel mc)             // Position Jump
		{
			mc.patternNumToJump = mc.effectArgX * 16 + mc.effectArgY;
			if (mc.patternNumToJump > 0x7F) mc.patternNumToJump = 0;

			currentRow = 0;
			track = module.arrangement[mc.patternNumToJump];
			pattern = module.patterns[module.arrangement[track]];
			return true;
		}
		private bool noteEffectC(MODMixerChannel mc)             // Set Volume
		{
			mc.channelVolume = (float)mc.effectArg / 0x40;
			mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
			return true;
		}
		private bool noteEffectD(MODMixerChannel mc)             // Pattern Break
		{
			mc.positionToJump = mc.effectArgX * 10 + mc.effectArgY;
			if (mc.positionToJump > 0x3F) mc.positionToJump = 0;

			track++;
			pattern = module.patterns[module.arrangement[track]];
			currentRow = mc.positionToJump;

			return true;
		}
		private bool noteEffectE(MODMixerChannel mc)             // Extended Effects
		{
			effectsE[mc.effectArgX](mc);
			effectsEUsed[mc.effectArgX] = true;
			return true;
		}
		private bool noteEffectF(MODMixerChannel mc)             // SetSpeed
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
		private bool tickEffect0(MODMixerChannel mc)
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
		private bool tickEffect1(MODMixerChannel mc)
		{
			mc.period = mc.portamentoStart;
			mc.periodInc = calcPeriodIncrement(mc.period);

			mc.portamentoStart += mc.portamentoStep;
			mc.portamentoStart = (mc.portamentoStart <= ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.portamentoStart;
			return true;
		}
		private bool tickEffect2(MODMixerChannel mc)
		{
			mc.period = mc.portamentoStart;
			mc.periodInc = calcPeriodIncrement(mc.period);

			mc.portamentoStart += mc.portamentoStep;
			mc.portamentoStart = (mc.portamentoStart >= ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.portamentoStart;
			return true;
		}
		private bool tickEffect3(MODMixerChannel mc)
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
		private bool tickEffect4(MODMixerChannel mc)
		{
			mc.vibratoAdd = (mc.vibratoType % 4 == 0) ? ModuleConst.ModSinusTable[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 1) ? ModuleConst.ModRampDownTable[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 2) ? ModuleConst.ModSquareTable[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 3) ? ModuleConst.ModRandomTable[mc.vibratoCount] : mc.vibratoAdd;

			mc.periodInc = calcPeriodIncrement(mc.vibratoPeriod);
			mc.vibratoPeriod = mc.vibratoStart + (int)(mc.vibratoAdd * ((float)mc.vibratoAmp / 128.0f));
			mc.vibratoPeriod = (mc.vibratoPeriod <= ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.vibratoPeriod;
			mc.vibratoPeriod = (mc.vibratoPeriod >= ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.vibratoPeriod;
			mc.vibratoCount = (mc.vibratoCount + mc.vibratoFreq) & 0x3F;
			return true;
		}
		private bool tickEffect5(MODMixerChannel mc)
		{
			tickEffectA(mc);
			tickEffect3(mc);
			return true;
		}
		private bool tickEffect6(MODMixerChannel mc)
		{
			tickEffectA(mc);
			tickEffect4(mc);
			return true;
		}
		private bool tickEffect7(MODMixerChannel mc)
		{
			mc.tremoloAdd = (mc.tremoloType % 4 == 0) ? ModuleConst.ModSinusTable[mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 1) ? ModuleConst.ModRampDownTable[mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 2) ? ModuleConst.ModSquareTable[mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 3) ? ModuleConst.ModRandomTable[mc.tremoloCount] : mc.tremoloAdd;

			mc.channelVolume = mc.tremoloStart + ((float)mc.tremoloAdd / 64) * ((float)mc.tremoloAmp / 0x40);
			mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
			mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
			mc.tremoloCount = (mc.tremoloCount + mc.tremoloFreq) & 0x3F;
			return true;
		}
		private bool tickEffect8(MODMixerChannel mc)
		{
			return true;
		}
		private bool tickEffect9(MODMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectA(MODMixerChannel mc)             // Volume Slide tick
		{
			mc.channelVolume += mc.volumeSlideStep;
			mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
			mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
			return true;
		}
		private bool tickEffectB(MODMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectC(MODMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectD(MODMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectE(MODMixerChannel mc)
		{
			return true;
		}
		private bool tickEffectF(MODMixerChannel mc)
		{
			return true;
		}
		//----------------------------------------------------------------------------------------------
		private bool effectE0(MODMixerChannel mc)
		{
			return true;
		}
		private bool effectE1(MODMixerChannel mc)
		{
			mc.period -= mc.effectArgY;
			mc.period = (mc.period < ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.period;
			mc.periodInc = calcPeriodIncrement(mc.period);
			return true;
		}
		private bool effectE2(MODMixerChannel mc)
		{
			mc.period += mc.effectArgY;
			mc.period = (mc.period > ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.period;
			mc.periodInc = calcPeriodIncrement(mc.period);
			return true;
		}
		private bool effectE3(MODMixerChannel mc)
		{
			return true;
		}
		private bool effectE4(MODMixerChannel mc)
		{
			mc.vibratoType = mc.effectArgY & 0x7;
			return true;
		}
		private bool effectE5(MODMixerChannel mc)
		{
			mc.currentFineTune = mc.effectArgY;
			return true;
		}
		private bool effectE6(MODMixerChannel mc)
		{
			return true;
		}
		private bool effectE7(MODMixerChannel mc)
		{
			mc.tremoloType = mc.effectArgY & 0x7;
			return true;
		}
		private bool effectE8(MODMixerChannel mc)
		{
			return true;
		}
		private bool effectE9(MODMixerChannel mc)                // Retrigger Sample
		{
			return true;
		}
		private bool effectEA(MODMixerChannel mc)                // Fine Volume Slide Up
		{
			mc.effect = 0x0A;
			mc.volumeSlideStep = (float)mc.effectArgY / 0x40;
			return true;
		}
		private bool effectEB(MODMixerChannel mc)                // Fine Volume Slide Down
		{
			mc.effect = 0x0A;
			mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;
			return true;
		}
		private bool effectEC(MODMixerChannel mc)                // Cut Sample
		{
			return true;
		}
		private bool effectED(MODMixerChannel mc)
		{
			return true;
		}
		private bool effectEE(MODMixerChannel mc)                // Delay
		{
			patternDelay = mc.effectArgY;
			return true;
		}
		private bool effectEF(MODMixerChannel mc)
		{
			return true;
		}
		//----------------------------------------------------------------------------------------------
		#endregion
		public MODMixer(MODModule module)
		{
			this.module = module;
			for (int i = 0; i < 16; i++)
			{
				noteEffectsUsed.Add(false);
				tickEffectsUsed.Add(false);
				effectsEUsed.Add(false);
			}

			/*
			for (int i = 0; i < 16; i++)
            {
				noteEffects.Add(noEffect);
				tickEffects.Add(noEffect);
				effectsE.Add(noEffect);
			}
			return; // without effects;
			*/

			noteEffects.Add(noteEffect0);               // 0. ARPEGGIO
			noteEffects.Add(noteEffect1);               // 1. PORTAMENTO UP 
			noteEffects.Add(noteEffect2);               // 2. PORTAMENTO DOWN
			noteEffects.Add(noteEffect3);               // 3. TONE PORTAMENTO
			noteEffects.Add(noteEffect4);               // 4. VIBRATO
			noteEffects.Add(noteEffect5);               // 5. TONE PORTAMENTO + VOLUME SLIDE
			noteEffects.Add(noteEffect6);               // 6. VIBRATO + VOLUME SLIDE
			noteEffects.Add(noteEffect7);               // 7. TREMOLO
			noteEffects.Add(/*noteEffect8*/noEffect);   // 8. PAN
			noteEffects.Add(/*noteEffect9*/noEffect);   // 9. SAMPLE OFFSET
			noteEffects.Add(noteEffectA);               // A. VOLUME SLIDE
			noteEffects.Add(/*noteEffectB*/noEffect);   // B. POSITION JUMP
			noteEffects.Add(noteEffectC);               // C. SET VOLUME
			noteEffects.Add(noteEffectD);               // D. PATTERN BREAK
			noteEffects.Add(noteEffectE);               // E. EXTEND EFFECTS
			noteEffects.Add(noteEffectF);               // F. SET SPEED

			tickEffects.Add(tickEffect0);               // tick effect 0. ARPEGGIO
			tickEffects.Add(tickEffect1);               // tick effect 1. PORTAMENTO UP 
			tickEffects.Add(tickEffect2);               // tick effect 2. PORTAMENTO DOWN
			tickEffects.Add(tickEffect3);               // tick effect 3. TONE PORTAMENTO
			tickEffects.Add(tickEffect4);               // tick effect 4. VIBRATO
			tickEffects.Add(tickEffect5);               // tick effect 5. TONE PORTAMENTO + VOLUME SLIDE
			tickEffects.Add(tickEffect6);               // tick effect 6. VIBRATO + VOLUME SLIDE
			tickEffects.Add(tickEffect7);               // tick effect 7. TREMOLO
			tickEffects.Add(/*tickEffect8*/noEffect);   // tick effect 8. PAN
			tickEffects.Add(/*tickEffect9*/noEffect);   // tick effect 9. SAMPLE OFFSET
			tickEffects.Add(tickEffectA);               // tick effect A. VOLUME SLIDE
			tickEffects.Add(/*tickEffectB*/noEffect);   // tick effect B. POSITION JUMP
			tickEffects.Add(/*tickEffectC*/noEffect);   // tick effect C. SET VOLUME
			tickEffects.Add(/*tickEffectD*/noEffect);   // tick effect D. PATTERN BREAK
			tickEffects.Add(/*tickEffectE*/noEffect);   // tick effect E. EXTEND EFFECTS
			tickEffects.Add(/*tickEffectF*/noEffect);   // tick effect F. SET SPEED

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
			effectsE.Add(effectEA);                     // EA. FINE VOLUME SLIDE UP
			effectsE.Add(effectEB);                     // EB. FINE VOLUME SLIDE DOWN
			effectsE.Add(/*effectEC*/noEffect);         // EC. CUT NOTE
			effectsE.Add(/*effectED*/noEffect);         // ED. NOTE DELAY
			effectsE.Add(/*effectEE*/noEffect);         // EE. PATTERN DELAY
			effectsE.Add(/*effectEF*/noEffect);         // EF. INVERT LOOP
		}
		private void resetChannelInstrument(MODMixerChannel mc)
		{
			mc.instrumentPosition = 2;
			mc.instrumentLength = mc.instrument.length;
			mc.loopType = mc.instrument.loopType;
			mc.instrumentLoopStart = false;
			mc.instrumentRepeatStart = mc.instrument.repeatStart;
			mc.instrumentRepeatStop = mc.instrument.repeatStop;
			mc.currentFineTune = mc.instrument.fineTune;

			mc.vibratoCount = 0;
			mc.tremoloCount = 0;
			mc.arpeggioCount = 0;
		}
		private void updateNote()
		{
			patternDelay = 0;
			for (int ch = 0; ch < module.nChannels; ch++)
			{
				MODMixerChannel mc = mixChannels[ch];
				MODPatternChannel pe = pattern.patternRows[currentRow].patternChannels[ch];

				mc.patternChannel = pe;
				mc.effect = pe.effekt;
				mc.effectArg = pe.effektOp;
				mc.effectArgX = (mc.effectArg & 0xF0) >> 4;
				mc.effectArgY = (mc.effectArg & 0x0F);

				if (pe.instrument > 0 && pe.period > 0)
				{
					mc.lastInstrument = mc.instrument;
					mc.instrument = module.instruments[pe.instrument - 1];
					resetChannelInstrument(mc);
					mc.channelVolume = mc.instrument.volume;
					mc.portamentoStart = mc.period;
					mc.noteIndex = ModuleConst.getNoteIndexForPeriod(pe.period);
					mc.period = ModuleConst.getNotePeriod(mc.noteIndex, mc.currentFineTune);
					mc.portamentoEnd = mc.period;
					mc.periodInc = calcPeriodIncrement(mc.period);
				}

				if (pe.instrument > 0 && pe.period == 0 && module.instruments[pe.instrument - 1] != mc.lastInstrument)
				{
					mc.lastInstrument = mc.instrument;
					mc.instrument = module.instruments[pe.instrument - 1];
					resetChannelInstrument(mc);
					mc.channelVolume = mc.instrument.volume;
				}

				if (pe.instrument > 0 && pe.period == 0 && module.instruments[pe.instrument - 1] == mc.lastInstrument)
					mc.channelVolume = mc.instrument.volume;

				if (pe.instrument == 0 && pe.period > 0)
				{
					mc.portamentoStart = mc.period;
					mc.noteIndex = ModuleConst.getNoteIndexForPeriod(pe.period);
					mc.period = ModuleConst.getNotePeriod(mc.noteIndex, mc.currentFineTune);
					mc.portamentoEnd = mc.period;
					mc.periodInc = calcPeriodIncrement(mc.period);
					resetChannelInstrument(mc);
				}
			}

			currentRow++;
			if (currentRow >= maxPatternRows)
			{
				pattEnd = true;
				currentRow = 0;
				track++;
				if (track >= module.songLength)
				{
					moduleEnd = true;
					//if (!mixLoop) played = true;
					//else
					//{
					//    track = 0;
					//    pattern = module.patterns[module.arrangement[track]];
					//}
				}
				else pattern = module.patterns[module.arrangement[track]];
			}
		}
		private void updateNoteEffects()
		{
			for (int ch = 0; ch < module.nChannels; ch++)
			{
				noteEffects[mixChannels[ch].effect](mixChannels[ch]);
				if (mixChannels[ch].effect == 0 && mixChannels[ch].effectArg != 0)
					noteEffectsUsed[0] = true;
				if (mixChannels[ch].effect != 0)
					noteEffectsUsed[mixChannels[ch].effect] = true;
			}
		}
		private void updateTickEffects()
		{
			for (int ch = 0; ch < module.nChannels; ch++)
			{
				tickEffects[mixChannels[ch].effect](mixChannels[ch]);
				if (mixChannels[ch].effect == 0 && mixChannels[ch].effectArg != 0)
					tickEffectsUsed[0] = true;
				if (mixChannels[ch].effect != 0)
					tickEffectsUsed[mixChannels[ch].effect] = true;
			}
		}
		private void setBPM()
		{
			mixerPosition = 0;
			samplesPerTick = calcSamplesPerTick(BPM);
		}
		private void updateBPM()
		{
			if (counter >= ((1 + patternDelay) * speed))
			{
				counter = 0;
				updateNote();
				updateNoteEffects();
				updateTickEffects();
			}
			else updateTickEffects();
			counter++;
		}
		private void initModule(int startPosition = 0)
		{
			track = startPosition;
			counter = 0;
			patternDelay = 0;
			samplesPerTick = 0;
			mixerPosition = 0;
			currentRow = 0;
			pattern = module.patterns[module.arrangement[track]];
			BPM = module.BPM;
			speed = module.tempo;
			moduleEnd = false;

			mixChannels.Clear();
			for (int ch = 0; ch < module.nChannels; ch++)
			{
				MODMixerChannel mc = new MODMixerChannel();
				mc.instrument = module.instruments[0];
				mc.lastInstrument = module.instruments[0];
				mixChannels.Add(mc);
			}
		}
		public void playModule(int startPosition = 0)
		{
			playing = false;
			initModule(startPosition);

			waveOut?.Stop();
			MixingTask?.Wait();

			waveReader?.Dispose();
			waveStream?.Dispose();
			waveOut?.Dispose();
			MixingTask?.Dispose();

			waveStream = new ModuleSoundStream(ModuleConst.SOUNDFREQUENCY, ModuleConst.MIX_CHANNELS == ModuleConst.STEREO);
			waveOut = new WaveOutEvent();
			mixData();

			playing = true;
			waveReader = new WaveFileReader(waveStream);
			waveOut.Init(waveReader);
			waveOut.Play();

			MixingTask = Task.Factory.StartNew(() =>
			{
				while ((!moduleEnd) && playing)
				{
					mixData();
					while (waveStream.QueueLength > ModuleConst.MIX_WAIT && playing)
					{
						Thread.Sleep(100);
					}
					//DebugMes("Play position - " + waveOut.GetPosition() + " queue length - " + waveStream.QueueLength);
				}
				while (waveStream.QueueLength > 0 && playing)
				{
					Thread.Sleep(100);
					//DebugMes("Play position - " + waveOut.GetPosition() + " queue length - " + waveStream.QueueLength);
				}
			});
		}
		public void Stop()
		{
			playing = false;
			waveOut.Stop();
		}
		private float getSampleValueSimple(MODMixerChannel mc)
		{
			return mc.instrument.instrumentData[(int)mc.instrumentPosition] * mc.channelVolume;
		}
		private float getSampleValueLinear(MODMixerChannel mc)
		{
			int posInt = (int)mc.instrumentPosition;
			float posReal = mc.instrumentPosition - posInt;
			float a1 = mc.instrument.instrumentData[posInt];
			if ((posInt + 1) >= mc.instrumentLength) return a1 * mc.channelVolume;
			float a2 = mc.instrument.instrumentData[posInt + 1];
			return (a1 + (a2 - a1) * posReal) * mc.channelVolume;
		}
		private float getSampleValueSpline(MODMixerChannel mc)
		{
			int posInt = (int)mc.instrumentPosition;
			float posReal = mc.instrumentPosition - posInt;
			float a2 = mc.instrument.instrumentData[posInt];
			if (((posInt - 1) < 0) || ((posInt + 2) >= mc.instrumentLength)) return a2 * mc.channelVolume;
			float a1 = mc.instrument.instrumentData[posInt - 1];
			float a3 = mc.instrument.instrumentData[posInt + 1];
			float a4 = mc.instrument.instrumentData[posInt + 2];

			float b0 = a1 + a2 + a2 + a2 + a2 + a3;
			float b1 = -a1 + a3;
			float b2 = a1 - a2 - a2 + a3;
			float b3 = -a1 + a2 + a2 + a2 - a3 - a3 - a3 + a4;
			return (((b3 * posReal * 0.1666666f + b2 * 0.5f) * posReal + b1 * 0.5f) * posReal + b0 * 0.1666666f) * mc.channelVolume;
		}
		private void mixData()
		{
			//var startTime = System.Diagnostics.Stopwatch.StartNew();
			//*
			string ms = " channels " + module.nChannels + " ";
			for (int pos = 0; pos < ModuleConst.MIX_LEN; pos++)
			{
				float mixValueR = 0;
				float mixValueL = 0;
				float mixValue = 0;
				for (int ch = 0; ch < module.nChannels; ch++)
				{
					MODMixerChannel mc = mixChannels[ch];
					//if (ch != 1) mc.muted = true;
					if (!mc.muted)
					{
						if ((mc.instrumentPosition >= mc.instrumentLength) && (!mc.instrumentLoopStart) && (mc.loopType == ModuleConst.LOOP_ON))
							mc.instrumentLoopStart = true;

						if ((mc.instrumentPosition >= mc.instrumentRepeatStop) && (mc.instrumentLoopStart))
							mc.instrumentPosition = mc.instrumentRepeatStart;

						if (mc.instrumentPosition < mc.instrumentLength)
						{
							mixValue += getSampleValueSimple(mc);
							//mixValue += getSampleValueLinear(mc);
							//mixValue += getSampleValueSpline(mc);
							if (ModuleConst.MIX_CHANNELS == ModuleConst.STEREO)
							{
								mixValueL += (((ch & 0x03) == 0) || ((ch & 0x03) == 3)) ? mixValue : 0;
								mixValueR += (((ch & 0x03) == 1) || ((ch & 0x03) == 2)) ? mixValue : 0;
							}
						}
					}
					mc.instrumentPosition += mc.periodInc;
				}
				if (ModuleConst.MIX_CHANNELS != ModuleConst.STEREO)
					mixValue /= module.nChannels;
				else
				{
					mixValueL /= (module.nChannels << 1);
					mixValueR /= (module.nChannels << 1);
				}

				if (ModuleConst.MIX_CHANNELS != ModuleConst.STEREO)
					waveStream.Write((short)(mixValue * ModuleConst.SOUND_AMP));
				else
				{
					waveStream.Write((short)((mixValueR * 0.75f + mixValueL * 0.25f) * ModuleConst.SOUND_AMP));
					waveStream.Write((short)((mixValueL * 0.75f + mixValueR * 0.25f) * ModuleConst.SOUND_AMP));
				}

				mixerPosition++;
				if (mixerPosition >= samplesPerTick)
				{
					setBPM();
					updateBPM();
				}
			}
			//*/
			//startTime.Stop();
			//var resultTime = startTime.Elapsed;
			//string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
			//                                    resultTime.Hours,
			//                                    resultTime.Minutes,
			//                                    resultTime.Seconds,
			//                                    resultTime.Milliseconds);
			//System.Diagnostics.Debug.WriteLine("Mixing time = " + elapsedTime);
		}
	}
}

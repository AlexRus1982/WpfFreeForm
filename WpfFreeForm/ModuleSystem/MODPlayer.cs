using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Media;

namespace ModuleSystem
{
    public static class MODConst
    {

		public static string[] noteStrings = 
		{
		 	"C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-"
		};
	
		public static int[] ModSinusTable = 
		{
			   0,   24,   49,   74,   97,  120,  141,  161,  180,  197,  212,  224,  235,  244,  250,  253,
			 255,  253,  250,  244,  235,  224,  212,  197,  180,  161,  141,  120,   97,   74,   49,   24,
			   0,  -24,  -49,  -74,  -97, -120, -141, -161, -180, -197, -212, -224, -235, -244, -250, -253,
			-255, -253, -250, -244, -235, -224, -212, -197, -180, -161, -141, -120,  -97,  -74,  -49,  -24
		};

		public static int[] ModRampDownTable =
		{
			   0,   -8,  -16,  -24,  -32,  -40,  -48,  -56,  -64,  -72,  -80,  -88,  -96, -104, -112, -120,
			-128, -136, -144, -152, -160, -168, -176, -184, -192, -200, -208, -216, -224, -232, -240, -248,
			 255,  247,  239,  231,  223,  215,  207,  199,  191,  183,  175,  167,  159,  151,  143,  135,
			 127,  119,  113,  103,   95,   87,   79,   71,   63,   55,   47,   39,   31,   23,   15,    7
		};

		public static int[] ModSquareTable =
		{
			 255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,
			 255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,
			-255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255,
			-255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255
		};

		public static int[] ModRandomTable =
		{
			 196, -254,  -86,  176,  204,   82, -130, -188,  250,   40, -142, -172, -140,  -64,  -32, -192,
			  34,  144,  214,  -10,  232, -138, -124,  -80,   20, -122,  130,  218,  -36,  -76,  -26, -152,
			 -46,  176,   42, -188,   16,  212,   42, -224,   12,  218,   40, -176,  -60,   18, -254,  236,
			  84,  -68,  178,   -8, -102, -144,   42,  -58,  224,  246,  168, -202, -184,  196, -108, -190
		};
	
		public static int[] ft2VibratoTable =
		{
			  0,  -2,  -3,  -5,  -6,  -8,  -9, -11, -12, -14, -16, -17, -19, -20, -22, -23,
			-24, -26, -27, -29, -30, -32, -33, -34, -36, -37, -38, -39, -41, -42, -43, -44,
			-45, -46, -47, -48, -49, -50, -51, -52, -53, -54, -55, -56, -56, -57, -58, -59,
			-59, -60, -60, -61, -61, -62, -62, -62, -63, -63, -63, -64, -64, -64, -64, -64,
			-64, -64, -64, -64, -64, -64, -63, -63, -63, -62, -62, -62, -61, -61, -60, -60,
			-59, -59, -58, -57, -56, -56, -55, -54, -53, -52, -51, -50, -49, -48, -47, -46,
			-45, -44, -43, -42, -41, -39, -38, -37, -36, -34, -33, -32, -30, -29, -27, -26,
			-24, -23, -22, -20, -19, -17, -16, -14, -12, -11,  -9,  -8,  -6,  -5,  -3,  -2,
			  0,   2,   3,   5,   6,   8,   9,  11,  12,  14,  16,  17,  19,  20,  22,  23,
			 24,  26,  27,  29,  30,  32,  33,  34,  36,  37,  38,  39,  41,  42,  43,  44,
			 45,  46,  47,  48,  49,  50,  51,  52,  53,  54,  55,  56,  56,  57,  58,  59,
			 59,  60,  60,  61,  61,  62,  62,  62,  63,  63,  63,  64,  64,  64,  64,  64,
			 64,  64,  64,  64,  64,  64,  63,  63,  63,  62,  62,  62,  61,  61,  60,  60,
			 59,  59,  58,  57,  56,  56,  55,  54,  53,  52,  51,  50,  49,  48,  47,  46,
			 45,  44,  43,  42,  41,  39,  38,  37,  36,  34,  33,  32,  30,  29,  27,  26,
			 24,  23,  22,  20,  19,  17,  16,  14,  12,  11,   9,   8,   6,   5,   3,   2
		};

		public const int NORM_MAX_PERIOD		= 907;
		public const int NORM_MIN_PERIOD		= 108;
		public const int EXT_MAX_PERIOD			= 1814;
		public const int EXT_MIN_PERIOD			= 54;
		public const int MAX_PERIOD				= EXT_MAX_PERIOD;
		public const int MIN_PERIOD				= EXT_MIN_PERIOD;
		public const int BASEFREQUENCYt			= 8363;
		public const int BASEPERIOD				= 428;
		public const int MAXVOLUME				= 64;
		public const int SM_16BIT				= 0x04;	// 16 BIT
		public const int SM_STEREO				= 0x08;	// STEREO
		public const int LOOP_OFF				= 0x00;
		public const int LOOP_ON				= 0x01;

		public static string getNoteNameToIndex(int index)
		{
			if (index <  -1) return "---";
			if (index ==  0) return "...";  // No Note
			if (index == -1) return "***"; // Note cut value
			return (noteStrings[(index - 1) % 12] + (int) (((index - 1) / 12) + 2));
		}

		public static int getNoteIndexForPeriod(int period)
		{
			if (period ==  0) return 0;		// No Note
			if (period == -1) return -1;	// Note cut value
			int note = 0;
			for (int i = 1; i < 6; i++)
			{
				for (int j = 0; j < 12; j++)
				{
					int diff1 = getNotePeriod(note - 1, 0) - period;
					int diff2 = period - getNotePeriod(note, 0);
					if (diff1 < diff2) return note - 1;
					note++;
				}
			}
			return note;
		}

		public static int getNotePeriod(int note, int finetune)
		{
			int period = 7680 - (note + 24) * 64 - finetune * 8;
			int frequency = (int)(8363 * Math.Pow(2, ((4608 - period) * 0.001302083)));
			return (int)(3579545.25f / frequency);
		}

		public static int getNoteFreq(int note, int finetune)
		{
			int period = 7680 - (note + 24) * 64 - finetune * 8;
			int frequency = (int)(8363 * Math.Pow(2, ((4608 - period) * 0.001302083)));
			return frequency;
		}
		
	}

	public class MODInstrument
	{
		public string name 				= "";	// Name of the sample
		public int length				= 0;	// full length (already *2 --> Mod-Fomat)
		public int fineTune				= 0;	// Finetuning -8..+8
		public float volume				= 0;	// Basisvolume
		public int repeatStart			= 0;	// # of the loop start (already *2 --> Mod-Fomat)
		public int repeatStop			= 0;	// # of the loop end   (already *2 --> Mod-Fomat)
		public int repeatLength			= 0;	// length of the loop
		public int loopType				= 0;	// 0: no Looping, 1: normal, 2: pingpong, 3: backwards
		public int baseFrequency		= 0;	// BaseFrequency
	
		public List<float> sampleData   = new List<float>();	// The sampledata, already converted to 16 bit (always)
																// 8Bit: 0..127,128-255; 16Bit: -32768..0..+32767												
		//private var snd:Sound 		= new Sound();
		//private var channel:SoundChannel;
		private float playPos			= 0;
		private float playInc			= 1;
		private bool loopStart			= false;

		public MODInstrument()
		{
			//super();
			//snd.addEventListener(SampleDataEvent.SAMPLE_DATA, mixSound);
		}

		//public function playSound():void
		//{
		//	playPos = 0;
		//	playInc = Number(this.baseFrequency / 44100);
		//loopStart = false;
		//	channel = snd.play();
		//}

		//private void mixSound(event:SampleDataEvent)
		//{
		//	for (var i:int = 0; i< 4096; i++)
		//	{
		//		var n:Number = 0;
		//		if (int(playPos) <= this.length)
		//		{
		//			n = this.sampleData[int(playPos)];
		//			event.data.writeFloat(n);
		//			event.data.writeFloat(n);
		//		}
				
		//		if ((playPos >= length) && (!loopStart) && (loopType == MODConst.LOOP_ON))
		//			loopStart = true;
					
		//		if ((playPos >= repeatStop) && (loopStart))
		//			playPos = repeatStart;

		//		playPos += playInc;
		//	}
		//}
		public void fixSampleLoops()
		{
			if (sampleData == null || length == 0)
			{
				repeatStop = repeatStart = 0;
				repeatLength = repeatStop - repeatStart;
				loopType = 0;
				return;
			}
			if (repeatStop > length)
			{
				repeatStop = length;
				repeatLength = repeatStop - repeatStart;
			}
            if (repeatStart + 2 > repeatStop)
            {
                repeatStop = repeatStart = 0;
                repeatLength = repeatStop - repeatStart;
                loopType = 0;
            }
        }
		public override string ToString()
		{
			///*
			//if (length == 0) return this.name;
			string res = this.name;
			res +=	"(len:" + length + ","
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

			int fine = stream.ReadByte() & 0xF; // finetune Value>7 means negative 8..15= -8..-1
			fineTune = (fine > 7) ? fine - 16 : fine;

			baseFrequency = MODConst.getNoteFreq(24, fine);

			int vol = stream.ReadByte() & 0x7F; // volume 64 is maximum
			volume = (vol > 64) ? 1.0f : (float)(vol / 64);

			//// Repeat start and stop
			repeatStart = ModuleUtils.readWord(stream) * 2;
			repeatLength = ModuleUtils.readWord(stream) * 2;
			repeatStop = repeatStart + repeatLength;

			if (length < 4) length = 0;
            if (length > 0)
            {
                if (repeatStart > length) repeatStart = length;
                if (repeatStop > length) repeatStop = length;
                if (repeatStart >= repeatStop ||
                    repeatStop <= 8 ||
                    (repeatStop - repeatStart) <= 4)
                {
                    repeatStart = repeatStop = 0;
                    loopType = 0;
                }
                if (repeatStart < repeatStop) loopType = MODConst.LOOP_ON;
            }
            else loopType = 0;
            repeatLength = repeatStop - repeatStart;
        }

        public void readInstrumentData(Stream stream)
		{
			if (length > 0)
			{
				sampleData.Clear();
				for (int s = 0; s < length; s++)
					sampleData.Add((float)(ModuleUtils.readSignedByte(stream)) * 0.0078125f);  // 0.0078125 = 1/128
			}
			fixSampleLoops();
		}		
	}

	public class MODPatternChannel
	{
		public int period				= 0;
		public int noteIndex			= 0;
		public int instrument			= 0;
		public int effekt				= 0;
		public int effektOp				= 0;
		public int volumeEffekt			= 0;
		public int volumeEffektOp		= 0;

		public override string ToString()
		{
			string res = MODConst.getNoteNameToIndex(noteIndex);
			if ((period == 0 && noteIndex != 0) || (period != 0 && noteIndex == 0))
				res += "!";
			else
				res += " ";
			if (instrument != 0) res += ModuleUtils.getAsHex(instrument, 2); 
			else res += "..";
		
			res += " ";
			if ((effekt != 0) || (effektOp != 0))
			{
				res += ModuleUtils.getAsHex(effekt, 1);
				res += ModuleUtils.getAsHex(effektOp, 2);
			}
			else res += "...";
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
	}
	public class MODPattern
	{
		public List<MODPatternRow> patternRows = new List<MODPatternRow>();
		public override string ToString()
		{
			string res = "\n";
			if (patternRows[0] != null)
			{
				string ln = "====";
				for (int i = 0; i < patternRows[0].patternChannels.Count; i++) ln += "===========";

				res += ln + "\n";

				for (int i = 0; i < patternRows.Count; i++)
					res += ModuleUtils.getAsHex(i, 2) + ":|" + patternRows[i] + "\n";			

				res += ln + "\n";
			}
			else res += "empty pattern\n";
			return res;
		}

		private MODPatternChannel createNewPatternChannel(Stream stream, int nSamples)
		{
			MODPatternChannel channel = new MODPatternChannel();
			int b0 = stream.ReadByte();
			int b1 = stream.ReadByte();
			int b2 = stream.ReadByte();
			int b3 = stream.ReadByte();

			channel.instrument = ((b0 & 0xF0) | ((b2 & 0xF0) >> 4)) & nSamples;
			channel.period = ((b0 & 0x0F) << 8) | b1;

			if (channel.period > 0) channel.noteIndex = (MODConst.getNoteIndexForPeriod(channel.period) + 1);

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
	}
	public class MODMixerInfo
	{
		public bool played 					= false;
		public int counter 					= 0;
		public int speed 					= 6;
		public int patternDelay				= 0;
		public int BPM 						= 125;
		public int samplesPerTick 			= 0;
		public int mixerPosition 			= 0;
		public int mixerTime				= 0;
		public int currentRow 				= 0;
		public int track 					= 0;
		//public var pattern:cPattern				= null;
	}
	public class MODMixerChannel
	{
		public bool muted 				= false;
		
		public int effect 					= 0;
		public int effectArg 				= 0;
		public int effectArgX 				= 0;
		public int effectArgY 				= 0;
		
		public int lastEffect 				= 0;
		public int lastEffectArg 			= 0;
		public int lastEffectArgX 			= 0;
		public int lastEffectArgY 			= 0;

		public int portamentoStart			= 0;
		public int portamentoEnd			= 0;
		public int portamentoStep			= 0;

        public bool volumeSlideStart		= false;
		public float volumeSlideX			= 0;
		public float volumeSlideY			= 0;

        public int patternJumpCounter		= 0;
		//public var patternToJump:cPattern		= null;
		public int patternNumToJump			= 0;
		public int positionToJump			= 0;
				
		public int arpeggioPeriod 			= 0;		
		public int arpeggioCount			= 0;
		public int arpeggioIndex			= 0;
		public int arpeggioX 				= 0;
		public int arpeggioY				= 0;
		
		public int vibratoType				= 0;
		public int vibratoStart 			= 0;
		public float vibratoAdd				= 0;
		public int vibratoCount				= 0;
		public int vibratoAmp 				= 0;
		public int vibratoFreq				= 0;
		
		public int tremoloType				= 0;
		public int tremoloStart 			= 0;
		public int tremoloAdd	 			= 0;
		public int tremoloCount				= 0;
		public int tremoloAmp 				= 0;
		public int tremoloFreq				= 0;

        public int noteIndex 				= 0;
		public bool noNote 					= true;
		public int lastNoteIndex			= 0;

  //      public var instrument:cInstrument 		= null;
		//public var lastInstrument:cInstrument	= null;
		//public var sample:cSample				= null;
		public int currentFineTune			= 0;
		//public var lastSample:cSample			= null;
		public int lastFineTune				= 0;
		public int period 					= 0;
		public int lastPeriod				= 0;
		public int freq 					= 0;
		public int lastFreq					= 0;
		public int periodInc				= 0;
		public int samplePosition			= 0;
		public int samplePositionReal		= 0;
        public int loopType					= MODConst.LOOP_OFF;
        public bool sampleLoopStart			= false;
		public int sampleRepeatStart 		= 0;
		public int sampleRepeatStop 		= 0;
		public int sampleLength 			= 0;
		public float sampleVolume 			= 1.0f;
		public float channelVolume 			= 1.0f;		
	}

	public class MODSoundModule : SoundModule
    {
		//public var mixer:cMODMixer						= null;

		public string fileName 						= "";
		public long fileLength						= 0;
		public string trackerName					= "";
		public string modID							= "";
		public string songName						= "";
		public int nChannels						= 0;
		public int nInstruments						= 0;
		public int nSamples							= 0;
		public int nPatterns						= 0;
		public int BPMSpeed							= 0;
		public int tempo							= 0;
		private List<MODInstrument> instruments		= new List<MODInstrument>();
		private List<MODPattern> patterns			= new List<MODPattern>();
		public int songLength						= 0;
		public List<int> arrangement 				= new List<int>();
		public int baseVolume						= 0;
		public bool isAmigaLike						= true;
		
		//public var onLoaded:Function 					= doNothing;
		//public var onProgress:Function 				= doNothing;
		
		private int bytesLeft						= 0;

		public MODSoundModule():base("MOD format")
		{
			DebugMes("MOD Sound Module created");
        }

		private long getAllInstrumentsLength()
		{
			long allSamplesLength = 0;
			foreach (MODInstrument inst in instruments)
				allSamplesLength += inst.length;

			return allSamplesLength;
		}

		public string InstrumentsToString()
		{
			string res = "Samples info : \n";

			int i = 0;
			foreach (MODInstrument inst in instruments)
				res += ModuleUtils.getAsHex(i++, 2) + ':' + inst.ToString();

			//if (sample.length > 0)
			//{
			//	result += 'Samples info : \n';
			//	for (var i:int = 0; i < sample.length; i++)
			//			{
			//		result += (ModuleUtils.getAsHex(i, 2) + ':' + sample[i].toString());
			//		result += '\n';
			//	}
			//}

			return res;
		}


		private void readInstruments(Stream stream)
		{
			for (int i = 0; i < nSamples; i++)
			{
				MODInstrument inst = new MODInstrument();
				instruments.Add(inst);
				inst.readInstrumentHeader(stream);
				DebugMes(inst.ToString());
			}
		}

		private void readArrangement(Stream stream)
		{
			songLength = stream.ReadByte(); // count of pattern in arrangement
			stream.ReadByte();				// skip, ood old CIAA
		
			// always space for 128 pattern...
			for (int i = 0; i < 128; i++) arrangement.Add(stream.ReadByte());


			//string argmt = "Pattern order - ";
			//for (int i = 0; i < songLength; i++)
			//{
			//	argmt += "" + (i + 1) + " : " + arrangement[i];
			//	if (i < arrangement.Count - 1) argmt += " , ";
			//}
			//argmt += "\n";
			//DebugMes(argmt);
		}

		private int calcPattCount()
		{
			int headerLen = 150; // Name+SongLen+CIAA+SongArrangement
			if (nSamples > 15) headerLen += 4;  // Kennung
						
			int sampleLen = 0;
			foreach (MODInstrument inst in instruments) sampleLen += 30 + inst.length;
			
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
			if (nSamples > 15) stream.Seek(4, SeekOrigin.Current);	// skip ModID, if not NoiseTracker:
																	// Read the patterndata
																	// Get the amount of pattern and keep "bytesLeft" in mind!
			bytesLeft = calcPattCount();

            for (int i = 0; i < nPatterns; i++)
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

			checkFormat(stream);
			setModType();


			stream.Seek(0, SeekOrigin.Begin);

			songName = ModuleUtils.readString0(stream, 20);

			nInstruments = nSamples;

			readInstruments(stream); // read instruments		

			readArrangement(stream); // read pattern order	

			readPatterns(stream); // read patterns

			readInstrumentsData(stream); // read samples data

			//mixer = new cMODMixer(this);
			//mixer.mixInfo.BPM = BPMSpeed;
			//mixer.mixInfo.speed = tempo;

			return true;
		}

		private void setModType()
		{
			isAmigaLike = false;
			nSamples = 31;

			if (modID == "M.K." || modID == "M!K!" || modID == "M&K!" || modID == "N.T.")
			{
				isAmigaLike = true; 
				nChannels = 4;
				trackerName = "ProTracker";
			}
			
			if (modID.Substring(0, 3) == "FLT")
			{				
				nChannels = int.Parse(modID.Substring(3, 1));
				trackerName = "StarTrekker";
			}

			if (modID.Substring(0, 3) == "TDZ")
			{
				nChannels = int.Parse(modID.Substring(3, 1));
				trackerName = "StarTrekker";
			}

			if (modID.Substring(1, 3) == "CHN")
			{
				nChannels = int.Parse(modID.Substring(0, 1));
				trackerName = "StarTrekker";
			}

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

			for (int i = 0; i < songLength; i++)
			{
				modInfo += arrangement[i];
				if (i < songLength - 1) 
					modInfo += ",";
			}
			modInfo += "\n\n";


			//modInfo += instrumentsList.toString() + '\n';

			//for (i = 0; i < songLength; i++)
			//{
			//	var pNum:int = arrangement[i];
			//	modInfo += 'Pattern : ' + cModuleUtils.getAsHex(pNum, 2) + '\n';
			//	modInfo += patternsList.pattern[pNum].toString() + '\n';
			//}

			//modInfo += pLst.toString();
			/**/
			return modInfo;
		}

		public override void PlayInstrument(int num)
        {
			if (num < 0 || num >= nSamples) return;

			SoundPlayer player = new SoundPlayer();
			BinaryWriter buffer = new BinaryWriter(new MemoryStream());
			
			MODInstrument inst = instruments[num];
			
			uint samplesPerSecond = 44100;
			float frqMul = (float)(inst.baseFrequency) / (float)(samplesPerSecond);
			DebugMes("BaseFreq = " + inst.baseFrequency + " Freq mull = " + frqMul);
			uint soundBufferLen = (uint)(inst.length / frqMul); 
			DebugMes("SoundBufferLen = " + soundBufferLen + " Instrument len = " + inst.length);

			char[] chunkId = { 'R', 'I', 'F', 'F' };
			char[] format = { 'W', 'A', 'V', 'E' };
			char[] subchunk1Id = { 'f', 'm', 't', ' ' };
			char[] subchunk2Id = { 'd', 'a', 't', 'a' };
			uint subchunk1Size = 16;
			uint headerSize = 8;
			ushort audioFormat = 1;
			ushort numChannels = 1;  // Mono - 1, Stereo - 2
			ushort bitsPerSample = 16;
			ushort blockAlign = (ushort)(numChannels * ((bitsPerSample + 7) / 8));
			uint sampleRate = samplesPerSecond;
			uint byteRate = sampleRate * blockAlign;
			uint waveSize = 4;
			uint subchunk2Size = soundBufferLen * blockAlign;
			uint chunkSize = waveSize + headerSize + subchunk1Size + headerSize + subchunk2Size;

			buffer.Write(chunkId);
			buffer.Write(chunkSize);
			buffer.Write(format);
			buffer.Write(subchunk1Id);
			buffer.Write(subchunk1Size);
			buffer.Write(audioFormat);
			buffer.Write(numChannels);
			buffer.Write(sampleRate);
			buffer.Write(byteRate);
			buffer.Write(blockAlign);
			buffer.Write(bitsPerSample);
			buffer.Write(subchunk2Id);
			buffer.Write(subchunk2Size);

			float fpos = 0;
			for (int i = 0; i < soundBufferLen; i++)
			{
				short s = 0;
				if (fpos < inst.length)
					s = (short)(32767 * inst.sampleData[(int)(fpos)]);
				buffer.Write(s);
				fpos += frqMul;
			}

			buffer.BaseStream.Seek(0, SeekOrigin.Begin);

			player.Stream = buffer.BaseStream;
			player.PlaySync();

			buffer.Close();
			player.Dispose();
		}
	}
}

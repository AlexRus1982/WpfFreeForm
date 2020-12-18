using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
			return (noteStrings[(index - 1) % 12] + (int) (((index - 1) / 12) + 3));
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
	public static class MODUtils
	{
		public static string VERSION 				= "V1.0";
		public static string PROGRAM 				= "Sound engine";
		public static string FULLVERSION 			= PROGRAM + " " + VERSION;
		public static string COPYRIGHT 				= "Copyright by Alex 2006/07/08/09/10";
	
		public static string CODING_GUI 			= "cp850";
		public static string CODING_COMMANLINE 		= "cp1252";
		public static string currentCoding 			= CODING_GUI;

		public const int SOUNDFREQUENCY 			= 44100;
		public const int SOUNDBITS 					= 16;
		public const int LOOP_ON					= 0x01;
		public const int LOOP_SUSTAIN_ON			= 0x02;
		public const int LOOP_IS_PINGPONG			= 0x04;
		public const int LOOP_SUSTAIN_IS_PINGPONG	= 0x08;
		
		public static string getAsHex(int value, int digits)
		{
			string hex = value.ToString("X" + digits.ToString());
			return hex;
		}

		public static string getAsDec(int value, int digits)
		{
			string dec = value.ToString("D" + digits.ToString());
			return dec;
		}

	}
	public class MODSample
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
		//private var snd:Sound 			= new Sound();
		//private var channel:SoundChannel;
		//private var playPos:Number		= 0;
		//private var playInc:Number		= 1;
		//private var loopStart:Boolean   = false;

		public MODSample()
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
			res += ('(')
				   + "len:" + length + ","
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

		//public void readSampleHeader(fileData:ByteArray)
		//{
		//	name = fileData.readMultiByte(22, 'cp866'); // Samplename
		//	length = fileData.readUnsignedShort() << 1; // Length

		//	var fine:int = fileData.readUnsignedByte() & 0xF; // finetune Value>7 means negative 8..15= -8..-1
		//	fine = (fine > 7) ? fine - 16 : fine;

		//	fineTune = fine;
		//	baseFrequency = cMODConst.getNoteFreq(24, fine);

		//	var vol:int = fileData.readUnsignedByte() & 0x7F; // volume 64 is maximum
		//	volume = (vol > 64) ? 1.0 : Number(vol / 0x40);

		//	// Repeat start and stop
		//	repeatStart = fileData.readUnsignedShort() << 1;
		//	repeatLength = fileData.readUnsignedShort() << 1;
		//	repeatStop = repeatStart + repeatLength;

		//	if (length < 4) length = 0;
		//	if (length > 0)
		//	{
		//		if (repeatStart > length) repeatStart = length;
		//		if (repeatStop > length) repeatStop = length;
		//		if (repeatStart >= repeatStop ||
		//			repeatStop <= 8 ||
		//			(repeatStop - repeatStart) <= 4)
		//		{
		//			repeatStart = repeatStop = 0;
		//			loopType = 0;
		//		}
		//		if (repeatStart < repeatStop) loopType = cMODConst.LOOP_ON;
		//	}
		//	else loopType = 0;
		//	repeatLength = repeatStop - repeatStart;
		//}
		public void readSampleData(BinaryReader reader)
		{
			if (length > 0)
			{
				sampleData = new List<float>();
				for (int s = 0; s < length; s++)
				{
					int data = reader.ReadByte();
					sampleData.Add((float)((data - 127) * 0.0078125));  // 0.0078125 = 1/128
				}
			}
			fixSampleLoops();
		}		
	}
	public class MODInstrument
	{
		//public array sampleIndex			= [];
		//public var noteIndex:Array				= [];
		public int volumeFadeOut			= 0;
		public string name					= "Noname instrument";
	
		// Impulstracker Values:
		public string dosFileName			= "";
		public int dublicateNoteCheck		= 0;
		public int dublicateNoteAction		= 0;
		/**
	 	* NNA: New note action:
		*      0 = Note cut
		*      1 = Note continue
		*      2 = Note off
		*      3 = Note fade
		*/
		public int NNA						= 0;
		public int pitchPanSeparation		= 0;
		public int pitchPanCenter			= 0;
		public int globalVolume				= 0;
		public int defaultPan				= 0;
		public int randomVolumeVariation 	= 0;
		public int randomPanningVariation	= 0;
		public int initialFilterCutoff		= 0;
		public int initialFilterResonance	= 0;
		public override string ToString()
		{
			return name;
		}

		//public function readInstrument(fileData:ByteArray):void
		//{
		//}
	}
	public class MODInstrumentsList
	{
		//public var sample:Array = [];

		public int getAllSamplesLength()
		{
			int allSamplesLength = 0;
			//for (int i = 0; i < sample.length; i++)
			//	allSamplesLength += sample[i].length;
			return allSamplesLength;
		}

		public override string ToString()
		{
			string result = "";

			//if (sample.length > 0)
			//{
			//	result += 'Samples info : \n';
			//	for (var i:int = 0; i < sample.length; i++)
			//			{
			//		result += (MODUtils.getAsHex(i, 2) + ':' + sample[i].toString());
			//		result += '\n';
			//	}
			//}

			return result;
		}
	}
	public class MODPatternElement
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
			if (instrument != 0) res += MODUtils.getAsDec(instrument, 2); 
			else res += "..";
		
			res += " ";
			if ((effekt != 0) || (effektOp != 0))
			{
				res += MODUtils.getAsHex(effekt, 1);
				res += MODUtils.getAsHex(effektOp, 2);
			}
			else res += "...";
			return res;
		}
	}
	public class MODPatternRow
	{
		public List<MODPatternElement> patternElements = new List<MODPatternElement>();
		public override string ToString()
		{
			string res = "";
			for (int i = 0; i < patternElements.Count; i++)
				res += patternElements[i].ToString() + '|';
			return res;
		}
	}
	public class MODPattern
	{
		public List<MODPatternRow> patternRows = new List<MODPatternRow>();
		public override string ToString()
		{
			string res = "";
			string ln = "";
			if (patternRows[0] != null)
			{
				ln = "====";
				for (int i = 0; i < patternRows[0].patternElements.Count; i++) ln += "===========";

				res += ln + "\n";

				for (int i = 0; i < patternRows.Count; i++)
					res += MODUtils.getAsHex(i, 2) + ":|" + patternRows[i] + "\n";			

				res += ln + "\n";
			}
			else res += "empty pattern\n";
			return res;
		}

		//private function createNewPatternElement(note:uint, nSamples):cPatternElement
		//{
		//	var pe:cPatternElement = new cPatternElement();

		//	pe.instrument = ((((note & 0xF0000000) >>> 24) | ((note & 0xF000) >>> 12)) & nSamples);
		//	pe.period = ((note & 0x0FFF0000) >>> 16);

		//	if (pe.period > 0) pe.noteIndex = (cMODConst.getNoteIndexForPeriod(pe.period) + 1);

		//	pe.effekt = ((note & 0x0F00) >>> 8);
		//	pe.effektOp = (note & 0xFF);

		//	return pe;
		//}

		//public function readPatternData(fileData:ByteArray, modID: String, nChannels, nSamples: int):void
		//{
		//	var pRow:cPatternRow;
		//	if (modID == 'FLT8') // StarTrekker is slightly different
		//	{
		//		for (var row:int = 0; row < 64; row++)
		//		{
		//			pRow = new cPatternRow();
		//			for (var channel:int = 0; channel < 4; channel++)
		//			{
		//				pRow.patternElement.push(createNewPatternElement(fileData.readUnsignedInt(), nSamples));
		//			}
		//			for (channel = 4; channel < 8; channel++) pRow.patternElement.push(new cPatternElement());
		//			patternRow.push(pRow);
		//		}
		//		for (row = 0; row < 64; row++)
		//		{
		//			for (channel = 4; channel < 8; channel++)
		//			{
		//				patternRow[row].patternElement[channel] =
		//				createNewPatternElement(fileData.readUnsignedInt(), nSamples);
		//			}
		//		}
		//	}
		//	else
		//	{
		//		for (row = 0; row < 64; row++)
		//		{
		//			pRow = new cPatternRow();
		//			for (channel = 0; channel < nChannels; channel++)
		//			{
		//				pRow.patternElement.push(createNewPatternElement(fileData.readUnsignedInt(), nSamples));
		//			}
		//			patternRow.push(pRow);
		//		}
		//	}

		//}
	}
	public class MODPatternList
	{
		public List<MODPattern> patterns = new List<MODPattern>();
		public override string ToString()
		{
			string res = "";
			
			for (int i = 0; i < patterns.Count; i++)
				res += i + ". Pattern:\n" + patterns[i].ToString();
			
			return res;
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
		public MODSoundModule():base("MOD format")
		{
			DebugMes("MOD Sound Module created");
        }

		public override bool readFromStream(Stream stream)
		{
			return true;
		}

	}
}

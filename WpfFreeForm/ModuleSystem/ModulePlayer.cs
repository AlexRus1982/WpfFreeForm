using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModuleSystem
{
    public static class ModuleConst
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

        public const int NORM_MAX_PERIOD = 907;
        public const int NORM_MIN_PERIOD = 108;
        public const int EXT_MAX_PERIOD = 1814;
        public const int EXT_MIN_PERIOD = 54;
        public const int MAX_PERIOD = EXT_MAX_PERIOD;
        public const int MIN_PERIOD = EXT_MIN_PERIOD;
        public const int BASEFREQUENCY = 8363;
        public const int BASEPERIOD = 428;
        public const int MAXVOLUME = 64;
        public const int SM_16BIT = 0x04;   // 16 BIT
        public const int SM_STEREO = 0x08;  // STEREO
        public const int LOOP_OFF = 0x00;
        public const int LOOP_ON = 0x01;

        public static string getNoteNameToIndex(int index)
        {
            if (index < -1) return "---";
            if (index == 0) return "...";  // No Note
            if (index == -1) return "***"; // Note cut value
            return (noteStrings[(index - 1) % 12] + (int)(((index - 1) / 12) + 2));
        }

        public static int getNoteIndexForPeriod(int period)
        {
            if (period == 0) return 0;      // No Note
            if (period == -1) return -1;    // Note cut value
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

    public static class ModuleUtils
    {
        public static string VERSION = "V1.0";
        public static string PROGRAM = "Sound module engine";
        public static string COPYRIGHT = "Copyright by Alex 2020";
        public static string FULLVERSION = PROGRAM + " " + VERSION + " " + COPYRIGHT;

        public const int SOUNDFREQUENCY = 22050;
        public const int SOUNDBITS = 16;
        public const int MONO = 0x01;
        public const int STEREO = 0x02;
        public const int LOOP_ON = 0x01;
        public const int LOOP_SUSTAIN_ON = 0x02;
        public const int LOOP_IS_PINGPONG = 0x04;
        public const int LOOP_SUSTAIN_IS_PINGPONG = 0x08;

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

        public static string readString0(Stream stream, int len)
        {
            string res = "";
            bool stopString = false;
            for (int i = 0; i < len; i++)
            {
                int s = stream.ReadByte();
                if (s == 0) stopString = true;
                if (s != 0 && !stopString) res += (char)s;
            }
            return res;
        }

        public static int readWord(Stream stream)
        {
            byte[] data = new byte[2];
            stream.Read(data, 0, 2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public static int readSignedByte(Stream stream)
        {
            byte[] b = new byte[1];
            stream.Read(b);
            return ((b[0] & 0x80) == 0) ? b[0] : b[0] - 256;
        }
    }

    public class ModuleMixerChannel
    {
        public bool muted = false;

        public int effect = 0;
        public int effectArg = 0;
        public int effectArgX = 0;
        public int effectArgY = 0;

        public int lastEffect = 0;
        public int lastEffectArg = 0;
        public int lastEffectArgX = 0;
        public int lastEffectArgY = 0;

        public int portamentoStart = 0;
        public int portamentoEnd = 0;
        public int portamentoStep = 0;

        public bool volumeSlideStart = false;
        public float volumeSlideX = 0;
        public float volumeSlideY = 0;

        public int patternJumpCounter = 0;
        public int patternNumToJump = 0;
        public int positionToJump = 0;

        public int arpeggioPeriod0 = 0;
        public int arpeggioPeriod1 = 0;
        public int arpeggioPeriod2 = 0;
        public int arpeggioCount = 0;
        public int arpeggioIndex = 0;
        public int arpeggioX = 0;
        public int arpeggioY = 0;

        public int vibratoType = 0;
        public int vibratoStart = 0;
        public float vibratoAdd = 0;
        public int vibratoCount = 0;
        public int vibratoAmp = 0;
        public int vibratoFreq = 0;

        public int tremoloType = 0;
        public int tremoloStart = 0;
        public int tremoloAdd = 0;
        public int tremoloCount = 0;
        public int tremoloAmp = 0;
        public int tremoloFreq = 0;

        public int noteIndex = 0;
        public bool noNote = true;
        public int lastNoteIndex = 0;

        public ModuleInstrument instrument = null;
        public ModuleInstrument lastInstrument = null;
        public int currentFineTune = 0;
        public int lastFineTune = 0;
        public int period = 0;
        public int lastPeriod = 0;
        public int freq = 0;
        public int lastFreq = 0;
        public float periodInc = 0;
        public float instrumentPosition = 0;
        public int loopType = ModuleConst.LOOP_OFF;
        public bool instrumentLoopStart = false;
        public float instrumentRepeatStart = 0;
        public float instrumentRepeatStop = 0;
        public int instrumentLength = 0;
        public float instrumentVolume = 1.0f;
        public float channelVolume = 1.0f;
    }

    public class ModuleMixer
    {
        protected int mixFreq = 22050;
        protected int mixBufferLen = 8192;
        protected int mixBits = 16;
        protected int mixChnls = 2;

        protected SoundModule module = null;
        protected List<ModuleMixerChannel> mixChannels = new List<ModuleMixerChannel>();
        protected List<Func<ModuleMixerChannel, bool>> noteEffects = new List<Func<ModuleMixerChannel, bool>>();
        protected List<Func<ModuleMixerChannel, bool>> tickEffects = new List<Func<ModuleMixerChannel, bool>>();
        protected List<Func<ModuleMixerChannel, bool>> effectsE = new List<Func<ModuleMixerChannel, bool>>();

        protected bool played = false;
        protected bool moduleEnd = false;
        protected int counter = 0;
        protected int speed = 6;
        protected int patternDelay = 0;
        protected int BPM = 125;
        protected int maxPatternRows = 64;
        protected int samplesPerTick = 0;
        protected int mixerPosition = 0;
        protected int mixerTime = 0;
        protected ModulePattern pattern = null;
        protected int currentRow = 0;
        protected int track = 0;
        protected bool mixLoop = true;
        protected bool mixStart = false;
        protected bool soundSystemReady = false;
        protected bool mixingReady = false;
        protected ModuleSoundSystem soundSystem = new ModuleSoundSystem();
        protected BinaryWriter mixingBuffer = new BinaryWriter(new MemoryStream());

        public ModuleMixer(SoundModule module)
        {
            this.module = module;
        }

        public int calcSamplesPerTick(int currentBPM)
		{
			if (currentBPM <= 0) return 0;
			return (int)((mixFreq * 2.5) / currentBPM);
		}

		public float calcPeriodIncrement(int period)
		{			
			if (period != 0) return ((float)(3546895 / period) / (float)mixFreq);
			else return 1.0f;
		}

        public virtual void updateNote()
        {
            patternDelay = 0;
            for (int ch = 0; ch < module.nChannels; ch++)
			{
                ModuleMixerChannel mc = mixChannels[ch];
                ModulePatternChannel pe = pattern.patternRows[currentRow].patternChannels[ch];

                mc.lastEffect = mc.effect;
                mc.lastEffectArg = mc.effectArg;
                mc.lastEffectArgX = mc.effectArgX;
                mc.lastEffectArgY = mc.effectArgY;

                mc.effect = pe.effekt;
                mc.effectArg = pe.effektOp;
                mc.effectArgX = (mc.effectArg & 0xF0) >> 4;
                mc.effectArgY = (mc.effectArg & 0x0F);

                if ((pe.instrument > 0) && (pe.period == 0))
                {
                    mc.instrument = module.instruments[pe.instrument - 1];
                    if (mc.instrument != mc.lastInstrument)
                    {
                        mc.instrumentPosition = 0;
                        mc.instrumentLength = mc.instrument.length;
                        mc.loopType = mc.instrument.loopType;
                        mc.instrumentLoopStart = false;
                        mc.instrumentRepeatStart = mc.instrument.repeatStart;
                        mc.instrumentRepeatStop = mc.instrument.repeatStop;
                        mc.channelVolume = mc.instrument.volume;
                        mc.lastInstrument = mc.instrument;
                    }
                    else
                    {
                        mc.channelVolume = mc.instrument.volume;
                    }
                }

                if ((pe.instrument > 0) && (pe.period > 0))
                {
                    mc.lastInstrument = mc.instrument;
                    mc.lastFineTune = mc.currentFineTune;
                    mc.instrument = module.instruments[pe.instrument - 1];
                    mc.currentFineTune = mc.instrument.fineTune;
                    if (mc.instrument != null)
                    {
                        mc.instrumentPosition = 0;
                        mc.instrumentLength = mc.instrument.length;
                        mc.loopType = mc.instrument.loopType;
                        mc.instrumentLoopStart = false;
                        mc.instrumentRepeatStart = mc.instrument.repeatStart;
                        mc.instrumentRepeatStop = mc.instrument.repeatStop;
                        mc.channelVolume = mc.instrument.volume;
                        mc.lastInstrument = mc.instrument;
                    }
                }

                if (pe.period > 0)
                {
                    mc.noNote = false;
                    mc.lastNoteIndex = mc.noteIndex;
                    mc.noteIndex = pe.noteIndex;
                    mc.lastPeriod = mc.period;
                    if (mc.instrument != null) mc.period = ModuleConst.getNotePeriod(mc.noteIndex - 1, mc.currentFineTune);
                    else mc.period = 0;
                    mc.periodInc = calcPeriodIncrement(mc.period);
                    if (mc.instrument != null)
                    {
                        mc.instrumentPosition = 0;
                        mc.instrumentLength = mc.instrument.length;
                        mc.loopType = mc.instrument.loopType;
                        mc.instrumentLoopStart = false;
                        mc.instrumentRepeatStart = mc.instrument.repeatStart;
                        mc.instrumentRepeatStop = mc.instrument.repeatStop;
                    }
                }
                else mc.noNote = true;
            }

            currentRow++;
            if (currentRow >= maxPatternRows)
            {
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

        public virtual void updateNoteEffects()
        {
            for (int ch = 0; ch < module.nChannels; ch++)
                noteEffects[mixChannels[ch].effect](mixChannels[ch]);
        }

        public virtual void updateTickEffects()
        {
            for (int ch = 0; ch < module.nChannels; ch++)
                tickEffects[mixChannels[ch].effect](mixChannels[ch]);
        }

        public virtual void setBPM()
        {
            mixerPosition = 0;
            samplesPerTick = calcSamplesPerTick(BPM);
        }

        public virtual void updateBPM()
		{
			if (counter >= speed + patternDelay * speed)
			{
				counter = 0;
				updateNote();
				updateNoteEffects();
			}
			else updateTickEffects();
			counter++;
		}

		public virtual void playModule(int startPosition = 0)
		{
			track = startPosition;

			pattern = module.patterns[module.arrangement[track]];
            //module.BPMSpeed = 229;
            //module.tempo = 5;
            System.Diagnostics.Debug.WriteLine("Mixer -> " + module.BPMSpeed);

            mixChannels.Clear();
            for (int ch = 0; ch < module.nChannels; ch++)
			{
                ModuleMixerChannel mc = new ModuleMixerChannel();
				mc.instrument = module.instruments[0];
				mc.lastInstrument = module.instruments[0];
                mixChannels.Add(mc);
            }

            var startTime = System.Diagnostics.Stopwatch.StartNew();
            System.Diagnostics.Debug.WriteLine("Start mixing -> ...");

            mixingBuffer.BaseStream.SetLength(0);

            moduleEnd = false;
            while (!moduleEnd)
            {
                mixData();
            }

            startTime.Stop();
            var resultTime = startTime.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                                                resultTime.Hours,
                                                resultTime.Minutes,
                                                resultTime.Seconds,
                                                resultTime.Milliseconds);
            System.Diagnostics.Debug.WriteLine("End mixing, time to mix = " + elapsedTime);
            System.Diagnostics.Debug.WriteLine("Mixing data length = " + mixingBuffer.BaseStream.Length);

            int secondsAll = (int)mixingBuffer.BaseStream.Length / (2 * mixFreq);
            TimeSpan t = TimeSpan.FromSeconds(secondsAll);
            System.Diagnostics.Debug.WriteLine("Mixing data music length = " + t.ToString());

            BinaryWriter soundBuffer = soundSystem.getBuffer;
            soundSystem.SetBufferLen((uint)mixingBuffer.BaseStream.Length / 2);
            mixingBuffer.BaseStream.Seek(0, SeekOrigin.Begin);
            mixingBuffer.BaseStream.CopyTo(soundBuffer.BaseStream);

            soundSystem.Play();

        }

        public virtual void Stop()
        {
            soundSystem.Stop();
        }

		private void mixData()
		{
            string ms = " channels " + module.nChannels + " ";
            for (int pos = 0; pos < mixBufferLen; pos++)
			{
				float mixValueR = 0;
				float mixValueL = 0;
				float mixValue = 0;
				for (int ch = 0; ch < module.nChannels; ch++)
				{
                    ModuleMixerChannel mc = mixChannels[ch];
                    //if (ch != 0) mc.muted = true;
					if (!mc.muted)
					{
						if ((mc.instrumentPosition >= mc.instrumentLength) && (!mc.instrumentLoopStart) && (mc.loopType == ModuleConst.LOOP_ON))
							mc.instrumentLoopStart = true;

						if ((mc.instrumentPosition >= mc.instrumentRepeatStop) && (mc.instrumentLoopStart))
							mc.instrumentPosition = mc.instrumentRepeatStart;

						if (mc.instrumentPosition < mc.instrumentLength)
						{
							//mixValue = int(getSampleValueSimple(mc.samplePosition, mc.samplePositionReal, mc.sample.sampleData) * mc.sampleVolume);
							mixValue += mc.instrument.instrumentData[(int)mc.instrumentPosition] * mc.channelVolume;
							//mixValueL += (((ch & 0x03) == 0) || ((ch & 0x03) == 3)) ? mixValue : 0;
							//mixValueR += (((ch & 0x03) == 1) || ((ch & 0x03) == 2)) ? mixValue : 0;
						}
					}
					mc.instrumentPosition += mc.periodInc;
				}

                //if (module.nChannels > 0)
                //{
                //	event.data.writeFloat(Number((mixValueL * 0.75 + mixValueR * 0.25) / (module.nChannels * 16384)));
                //	event.data.writeFloat(Number((mixValueR * 0.75 + mixValueL * 0.25) / (module.nChannels * 16384)));
                //}
                //else
                //{
                //	event.data.writeFloat(0.0);
                //	event.data.writeFloat(0.0);					
                //}

                mixingBuffer.Write((short)(32767 * mixValue / module.nChannels));

                mixerPosition++;
				if (mixerPosition >= samplesPerTick)
				{
					setBPM();
					updateBPM();
				}                
			}
        }
    }

    //	public class ModuleMixer
    //	{
    //		const MIX_FREQ:int 					= 44100;
    //		const MIX_BUFFER_LEN:int			= 8192;
    //		const MIX_BITS:int 					= 16;
    //		const MIX_CHANNELS:int 				= 2;
    //		const MIX_SIGNED:Boolean 			= true;
    //		const REAL_PART_DIVIDER:int			= 256;
    //		const REAL_PART_SHIFT:int			= 8;

    //		private function getSampleValueSimple(pos:int, rpos:int, sampleData:Array):int
    //		{
    //			return sampleData[pos];
    //		}

    //		private function getSampleValueLinear(pos:int, rpos:int, sampleData:Array):int
    //		{
    //			var posInt:int = pos;
    //			if ((posInt + 1) > sampleData.length) return sampleData[pos];
    //			var posReal:Number = (rpos - (pos << REAL_PART_SHIFT)) / REAL_PART_DIVIDER;
    //			var a1:int = sampleData[posInt    ];
    //			var a2:int = sampleData[posInt + 1];
    //			return int(a1 + (a2 - a1) * posReal);
    //		}

    //		private function getSampleValueSpline(pos : int, rpos:int, sampleData:Array):int
    //		{
    //			var posInt:int = pos;
    //			if (((posInt - 1) < 0) || ((posInt + 2) > sampleData.length)) return sampleData[posInt];
    //			var posReal:Number = (rpos - (pos << REAL_PART_SHIFT)) / REAL_PART_DIVIDER;
    //			var a1:int = sampleData[posInt - 1];
    //			var a2:int = sampleData[posInt    ];
    //			var a3:int = sampleData[posInt + 1];
    //			var a4:int = sampleData[posInt + 2];			

    //			var b0:int = ( a1 + a2 + a2 + a2 + a2 + a3);
    //  			var b1:int = (-a1 + a3);
    //  			var b2:int = ( a1 - a2 - a2 + a3);
    //  			var b3:int = (-a1 + a2 + a2 + a2 - a3 - a3 - a3 + a4);
    //  			return int(((b3 * posReal * 0.1666666 + b2 * 0.5) * posReal + b1 * 0.5) * posReal + b0 * 0.1666666);
    //		}

    //		//note update
    //		function nEffect_09(mc:cMixerChannel):void
    //		{
    //			mc.samplePosition = mc.effectArg << 8;
    //			mc.samplePositionReal = mc.samplePosition << REAL_PART_SHIFT;

    //			if ((mc.samplePosition >= mc.sampleLength) && (!mc.sampleLoopStart) && (mc.loopType == cMODConst.LOOP_ON))
    //					mc.sampleLoopStart = true;

    //			if ((mc.samplePosition >= mc.sampleRepeatStop) && (mc.sampleLoopStart))
    //			{
    //				mc.samplePosition = mc.sampleRepeatStart;
    //				mc.samplePositionReal = mc.samplePosition << REAL_PART_SHIFT;
    //			}					
    //		}
    //		function nEffect_0B(mc:cMixerChannel):void
    //		{
    //			mc.patternJumpCounter = 0;
    //			mc.patternNumToJump = mc.effectArgX * 16 + mc.effectArgY;
    //			if (mc.patternNumToJump > 0x7F) mc.patternNumToJump = 0;
    //			mc.patternToJump = module.patternsList.pattern[module.arrangement[mc.patternNumToJump]];

    //			mixInfo.currentRow = 0;
    //			mixInfo.track = mc.patternNumToJump;
    //			mixInfo.pattern = mc.patternToJump;
    //		}
    //		function nEffect_0D(mc:cMixerChannel):void
    //		{
    //			mc.patternJumpCounter = 0;
    //			mc.patternToJump = module.patternsList.pattern[module.arrangement[mixInfo.track + 1]];
    //			mc.positionToJump = mc.effectArgX * 10 + mc.effectArgY;
    //			if (mc.positionToJump > 0x3F) mc.positionToJump = 0;

    //			mixInfo.currentRow = mc.positionToJump;
    //			mixInfo.track++;
    //			mixInfo.pattern = mc.patternToJump;	
    //		}

    //		//tick update
    //		function tEffect_05(mc:cMixerChannel):void
    //		{
    //			tEffect_0A(mc);
    //			tEffect_03(mc);
    //		}
    //		function tEffect_06(mc:cMixerChannel):void
    //		{
    //			tEffect_0A(mc);
    //			tEffect_04(mc);
    //		}
    //		function tEffect_07(mc:cMixerChannel):void
    //		{
    //			if (mc.tremoloType % 4 == 0) mc.tremoloAdd = cMODConst.ModSinusTable	[mc.tremoloCount];
    //			if (mc.tremoloType % 4 == 1) mc.tremoloAdd = cMODConst.ModRampDownTable	[mc.tremoloCount];
    //			if (mc.tremoloType % 4 == 2) mc.tremoloAdd = cMODConst.ModSquareTable	[mc.tremoloCount];
    //			if (mc.tremoloType % 4 == 3) mc.tremoloAdd = cMODConst.ModRandomTable	[mc.tremoloCount];
    //			mc.channelVolume = mc.tremoloStart + int((mc.tremoloAdd / 128) * (mc.tremoloAmp / 0x40));
    //			mc.channelVolume = (mc.channelVolume < 0.0) ? 0.0 : mc.channelVolume;
    //			mc.channelVolume = (mc.channelVolume > 1.0) ? 1.0 : mc.channelVolume;
    //			mc.tremoloCount = (mc.tremoloCount + mc.tremoloFreq) & 0x3F;

    //		}
    //		function tEffect_08(mc:cMixerChannel):void{}
    //		function tEffect_09(mc:cMixerChannel):void{}
    //		function tEffect_0A(mc:cMixerChannel):void
    //		{
    //			if (mc.volumeSlideStart)
    //			{
    //				mc.channelVolume += mc.volumeSlideX;
    //				mc.channelVolume -= mc.volumeSlideY;
    //				mc.channelVolume = (mc.channelVolume < 0.0) ? 0.0 : mc.channelVolume;
    //				mc.channelVolume = (mc.channelVolume > 1.0) ? 1.0 : mc.channelVolume;
    //			}
    //			else mc.volumeSlideStart = true;
    //		}
    //		function tEffect_0B(mc:cMixerChannel):void{}
    //		function tEffect_0C(mc:cMixerChannel):void{}
    //		function tEffect_0D(mc:cMixerChannel):void{}
    //		function tEffect_0E(mc:cMixerChannel):void{}
    //		function tEffect_0F(mc:cMixerChannel):void{}

    //		//E effects
    //		function effect_E0(mc:cMixerChannel):void{}		
    //		function effect_E1(mc:cMixerChannel):void
    //		{
    //			mc.period -= mc.effectArgY;
    //			mc.period = (mc.period < 113) ? 113 : mc.period;
    //			mc.periodInc = calcPeriodIncrement(mc.period, MIX_FREQ);
    //		}
    //		function effect_E2(mc:cMixerChannel):void
    //		{
    //			mc.period += mc.effectArgY;
    //			mc.period = (mc.period > 856) ? 856 : mc.period;			
    //			mc.periodInc = calcPeriodIncrement(mc.period, MIX_FREQ);
    //		}		
    //		function effect_E3(mc:cMixerChannel):void{}
    //		function effect_E4(mc:cMixerChannel):void
    //		{
    //			mc.vibratoType = mc.effectArgY & 0x7;
    //		}
    //		function effect_E5(mc:cMixerChannel):void
    //		{
    //			mc.currentFineTune = mc.effectArgY;
    //		}
    //		function effect_E6(mc:cMixerChannel):void{}
    //		function effect_E7(mc:cMixerChannel):void
    //		{
    //			mc.tremoloType = mc.effectArgY & 0x7;
    //		}
    //		function effect_E8(mc:cMixerChannel):void{}
    //		function effect_E9(mc:cMixerChannel):void{}
    //		function effect_EA(mc:cMixerChannel):void
    //		{
    //			if (mc.effectArgX != 0)
    //			{
    //				mc.volumeSlideX = Number(mc.effectArgX / 0x40);
    //				mc.volumeSlideY = 0;
    //			}
    //			mc.channelVolume += mc.volumeSlideX;
    //			mc.channelVolume = (mc.channelVolume > 1.0) ? 1.0 : mc.channelVolume;
    //			mc.effect = 0x0A;
    //		}
    //		function effect_EB(mc:cMixerChannel):void
    //		{
    //			if (mc.effectArgY != 0)
    //			{
    //				mc.volumeSlideX = 0;
    //				mc.volumeSlideY = Number(mc.effectArgY / 0x40);
    //			}
    //			mc.channelVolume -= mc.volumeSlideY;
    //			mc.channelVolume = (mc.channelVolume < 0.0) ? 0.0 : mc.channelVolume;
    //			mc.effect = 0x0A;
    //		}
    //		function effect_EC(mc:cMixerChannel):void{}
    //		function effect_ED(mc:cMixerChannel):void{}
    //		function effect_EF(mc:cMixerChannel):void{}

    //	}
    //}

    public class ModuleInstrument
    {
        public string name = "";    // Name of the sample
        public int length = 0;  // full length (already *2 --> Mod-Fomat)
        public int fineTune = 0;    // Finetuning -8..+8
        public float volume = 0;    // Basisvolume
        public float repeatStart = 0; // # of the loop start (already *2 --> Mod-Fomat)
        public float repeatStop = 0;  // # of the loop end   (already *2 --> Mod-Fomat)
        public int repeatLength = 0;    // length of the loop
        public int loopType = 0;    // 0: no Looping, 1: normal, 2: pingpong, 3: backwards
        public int baseFrequency = 0;   // BaseFrequency

        public List<float> instrumentData = new List<float>();    // The sampledata, already converted to 16 bit (always)
                                                                  // 8Bit: -128 to 127; 16Bit: -32768..0..+32767

        public ModuleInstrument()
        {
        }

        public void fixSampleLoops()
        {
            if (instrumentData == null || length == 0)
            {
                repeatStop = repeatStart = 0;
                repeatLength = (int)(repeatStop - repeatStart);
                loopType = 0;
                return;
            }
            if (repeatStop > length)
            {
                repeatStop = length;
                repeatLength = (int)(repeatStop - repeatStart);
            }
            if (repeatStart + 2 > repeatStop)
            {
                repeatStop = repeatStart = 0;
                repeatLength = (int)(repeatStop - repeatStart);
                loopType = 0;
            }
        }
        public override string ToString()
        {
            ///*
            //if (length == 0) return this.name;
            string res = this.name;
            res += "(len:" + length + ","
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

            baseFrequency = ModuleConst.getNoteFreq(24, fine);

            int vol = stream.ReadByte(); // volume 64 is maximum
            volume = (vol > 64) ? 1.0f : (float)vol / 64.0f;

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
                if (repeatStart < repeatStop) loopType = ModuleConst.LOOP_ON;
            }
            else loopType = 0;
            repeatLength = (int)(repeatStop - repeatStart);
        }

        public void readInstrumentData(Stream stream)
        {
            instrumentData.Clear();
            if (length > 0)
            {
                for (int s = 0; s < length; s++)
                    instrumentData.Add((float)(ModuleUtils.readSignedByte(stream)) * 0.0078125f);  // 0.0078125 = 1/128
            }
            fixSampleLoops();
        }

        public void Clear()
        {
            instrumentData.Clear();
        }
    }

    public class ModulePatternChannel
    {
        public int period = 0;
        public int noteIndex = 0;
        public int instrument = 0;
        public int effekt = 0;
        public int effektOp = 0;
        public int volumeEffekt = 0;
        public int volumeEffektOp = 0;

        public override string ToString()
        {
            string res = ModuleConst.getNoteNameToIndex(noteIndex);
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

    public class ModulePatternRow
    {
        public List<ModulePatternChannel> patternChannels = new List<ModulePatternChannel>();
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

    public class ModulePattern
    {
        public List<ModulePatternRow> patternRows = new List<ModulePatternRow>();
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

        public void Clear()
        {
            patternRows.Clear();
        }
    }

    public class SoundModule : IDisposable
    {
        protected readonly string SoundModuleName = "Base module";
        protected float position = 0;
        protected ModuleSoundSystem soundSystem = new ModuleSoundSystem();

        public string fileName = "";
        public long fileLength = 0;
        public string trackerName = "";
        public string modID = "";
        public string songName = "";
        public int nChannels = 0;
        public int nInstruments = 0;
        public int nSamples = 0;
        public int nPatterns = 0;
        public int BPMSpeed = 0;
        public int tempo = 0;
        public int songLength = 0;
        public int baseVolume = 0;
        public bool isAmigaLike = true;
        public List<ModuleInstrument> instruments = new List<ModuleInstrument>();
        public List<ModulePattern> patterns = new List<ModulePattern>();
        public List<int> arrangement = new List<int>();

        protected int bytesLeft = 0;


        public float Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                rewindToPosition();
            }
        }

        public SoundModule(string soundModuleName)
        {
            SoundModuleName = soundModuleName;
        }

        public virtual bool checkFormat(Stream stream)
        {
            return false;
        }

        public virtual bool readFromStream(Stream stream)
        {
            return true;
        }

        public virtual void rewindToPosition(){}

        public virtual void Play(){}

        public virtual void PlayInstrument(int num){}

        public virtual void Stop(){}

        public virtual void Pause(){}

        public virtual void Dispose(){}

        protected void DebugMes(string mes)
        {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine(SoundModuleName + " -> " + mes);
            #endif
        }
    }

    public class ModulePlayer
    {
        private List<SoundModule> libModules = new List<SoundModule>();
        private SoundModule sModule = null;

        public float Position
        {
            get
            {
                if (sModule != null) return sModule.Position;
                else return 0;
            }
            set
            {
                if (sModule != null) sModule.Position = value;
            }
        }

        public ModulePlayer()
        {
            libModules.Add(new MODSoundModule());
        }

        public bool OpenFromStream(Stream stream)
        {
            if (sModule != null) sModule.Dispose();
            for (int i = 0; i < libModules.Count; i++)
                if (libModules[i].checkFormat(stream)) sModule = libModules[i];

            if (sModule != null) sModule.readFromStream(stream);
            DebugMes(sModule.ToString());
            return true;
        }

        public bool OpenFromFile(string fileName)
        {
            if (sModule != null) sModule.Dispose();

            bool res = false;

            using (FileStream fstream = File.OpenRead(fileName))
            {
                DebugMes("Read from file : " + fileName);
                res = OpenFromStream(fstream);
            }

            return res;
        }

        public void Play()
        {
            if (sModule != null) sModule.Play();
        }

        public void PlayInstrument(int num)
        {
            if (sModule != null) sModule.PlayInstrument(num);
        }

        public void Stop()
        {
            if (sModule != null) sModule.Stop();
        }

        public void Pause()
        {
            if (sModule != null) sModule.Pause();
        }

        private void DebugMes(string mes)
        {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("ModulePlayer -> " + mes);
            #endif
        }
    }
}

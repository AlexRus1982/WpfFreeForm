﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModuleSystem {

    /// <summary>
    /// ModuleConst describe useful constant values
    /// </summary>
    public static class ModuleConst {

        public const string VERSION                 = "V1.0";
        public const string PROGRAM                 = "Sound module player";
        public const string COPYRIGHT               = "Copyright by Alex 2020";
        public const string FULLVERSION             = PROGRAM + " " + VERSION + " " + COPYRIGHT;

        public const int SOUNDBITS                  = 16;
        public const int MONO                       = 0x01;
        public const int STEREO                     = 0x02;
        public const int LOOP_OFF                   = 0x00;
        public const int LOOP_ON                    = 0x01;
        public const int LOOP_SUSTAIN_ON            = 0x02;
        public const int LOOP_IS_PINGPONG           = 0x04;
        public const int LOOP_SUSTAIN_IS_PINGPONG   = 0x08;

        public const int SOUNDFREQUENCY             = 22050; //44100
        public const int SOUNDBUFFERSECONDS         = 1;
        public const int MIX_LEN                    = SOUNDFREQUENCY * SOUNDBUFFERSECONDS;
        public const int MIX_WAIT                   = MIX_LEN * 2;
        public const int MIX_CHANNELS               = MONO;

        public const float AMIGA_FREQUENCY          = 7093789.2f;                   //7093789.2f 7159090.5f
        public const float HALF_AMIGA_FREQUENCY     = AMIGA_FREQUENCY * 0.5f;       //7093789.2f 7159090.5f
        public const int NORM_MAX_PERIOD            = 856;
        public const int NORM_MIN_PERIOD            = 113;
        public const int EXT_MAX_PERIOD             = 1712;
        public const int EXT_MIN_PERIOD             = 57;
        public const int MAX_PERIOD                 = NORM_MAX_PERIOD;
        public const int MIN_PERIOD                 = NORM_MIN_PERIOD;
        public const int BASEFREQUENCY              = 8363;
        public const int BASEPERIOD                 = 428;
        public const int MAXVOLUME                  = 64;
        public const int SM_16BIT                   = 0x04;     // 16 BIT
        public const int SM_STEREO                  = 0x08;     // STEREO
        public const int SOUND_AMP                  = 32750;    // MAX SOUND AMP -32768..32767

        public static string[] noteStrings = {
             "C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-"
        };

        public static int[] ModSinusTable = {
               0,   24,   49,   74,   97,  120,  141,  161,  180,  197,  212,  224,  235,  244,  250,  253,
             255,  253,  250,  244,  235,  224,  212,  197,  180,  161,  141,  120,   97,   74,   49,   24,
               0,  -24,  -49,  -74,  -97, -120, -141, -161, -180, -197, -212, -224, -235, -244, -250, -253,
            -255, -253, -250, -244, -235, -224, -212, -197, -180, -161, -141, -120,  -97,  -74,  -49,  -24
        };

        public static int[] ModRampDownTable = {
               0,   -8,  -16,  -24,  -32,  -40,  -48,  -56,  -64,  -72,  -80,  -88,  -96, -104, -112, -120,
            -128, -136, -144, -152, -160, -168, -176, -184, -192, -200, -208, -216, -224, -232, -240, -248,
             255,  247,  239,  231,  223,  215,  207,  199,  191,  183,  175,  167,  159,  151,  143,  135,
             127,  119,  113,  103,   95,   87,   79,   71,   63,   55,   47,   39,   31,   23,   15,    7
        };

        public static int[] ModSquareTable = {
             255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,
             255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,  255,
            -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255,
            -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255, -255
        };

        public static int[] ModRandomTable = {
             196, -254,  -86,  176,  204,   82, -130, -188,  250,   40, -142, -172, -140,  -64,  -32, -192,
              34,  144,  214,  -10,  232, -138, -124,  -80,   20, -122,  130,  218,  -36,  -76,  -26, -152,
             -46,  176,   42, -188,   16,  212,   42, -224,   12,  218,   40, -176,  -60,   18, -254,  236,
              84,  -68,  178,   -8, -102, -144,   42,  -58,  224,  246,  168, -202, -184,  196, -108, -190
        };

        public static int[] ft2VibratoTable = {
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

        public static uint[,] PERIOD_TABLE = {
           {1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016, 960 , 906,  // Finetune +0
            856 , 808 , 762 , 720 , 678 , 640 , 604 , 570 , 538 , 508 , 480 , 453,
            428 , 404 , 381 , 360 , 339 , 320 , 302 , 285 , 269 , 254 , 240 , 226,
            214 , 202 , 190 , 180 , 170 , 160 , 151 , 143 , 135 , 127 , 120 , 113,
            107 , 101 , 95  , 90  , 85  , 80  , 75  , 71  , 67  , 63  , 60  , 56 },
           {1700, 1604, 1514, 1430, 1348, 1274, 1202, 1134, 1070, 1010, 954 , 900,  // Finetune +1
            850 , 802 , 757 , 715 , 674 , 637 , 601 , 567 , 535 , 505 , 477 , 450,
            425 , 401 , 379 , 357 , 337 , 318 , 300 , 284 , 268 , 253 , 239 , 225,
            213 , 201 , 189 , 179 , 169 , 159 , 150 , 142 , 134 , 126 , 119 , 113,
            106 , 100 , 94  , 89  , 84  , 79  , 75  , 71  , 67  , 63  , 59  , 56 },
           {1688, 1592, 1504, 1418, 1340, 1264, 1194, 1126, 1064, 1004, 948 , 894,  // Finetune +2
            844 , 796 , 752 , 709 , 670 , 632 , 597 , 563 , 532 , 502 , 474 , 447,
            422 , 398 , 376 , 355 , 335 , 316 , 298 , 282 , 266 , 251 , 237 , 224,
            211 , 199 , 188 , 177 , 167 , 158 , 149 , 141 , 133 , 125 , 118 , 112,
            105 , 99  , 94  , 88  , 83  , 79  , 74  , 70  , 66  , 62  , 59  , 56 },
           {1676, 1582, 1492, 1408, 1330, 1256, 1184, 1118, 1056, 996 , 940 , 888,  // Finetune +3
            838 , 791 , 746 , 704 , 665 , 628 , 592 , 559 , 528 , 498 , 470 , 444,
            419 , 395 , 373 , 352 , 332 , 314 , 296 , 280 , 264 , 249 , 235 , 222,
            209 , 198 , 187 , 176 , 166 , 157 , 148 , 140 , 132 , 125 , 118 , 111,
            104 , 99  , 93  , 88  , 83  , 78  , 74  , 70  , 66  , 62  , 59  , 55 },
           {1664, 1570, 1482, 1398, 1320, 1246, 1176, 1110, 1048, 990 , 934 , 882,  // Finetune +4
            832 , 785 , 741 , 699 , 660 , 623 , 588 , 555 , 524 , 495 , 467 , 441,
            416 , 392 , 370 , 350 , 330 , 312 , 294 , 278 , 262 , 247 , 233 , 220,
            208 , 196 , 185 , 175 , 165 , 156 , 147 , 139 , 131 , 124 , 117 , 110,
            104 , 98  , 92  , 87  , 82  , 78  , 73  , 69  , 65  , 62  , 58  , 55 },
           {1652, 1558, 1472, 1388, 1310, 1238, 1168, 1102, 1040, 982 , 926 , 874,  // Finetune +5
            826 , 779 , 736 , 694 , 655 , 619 , 584 , 551 , 520 , 491 , 463 , 437,
            413 , 390 , 368 , 347 , 328 , 309 , 292 , 276 , 260 , 245 , 232 , 219,
            206 , 195 , 184 , 174 , 164 , 155 , 146 , 138 , 130 , 123 , 116 , 109,
            103 , 97  , 92  , 87  , 82  , 77  , 73  , 69  , 65  , 61  , 58  , 54 },
           {1640, 1548, 1460, 1378, 1302, 1228, 1160, 1094, 1032, 974 , 920 , 868,  // Finetune +6
            820 , 774 , 730 , 689 , 651 , 614 , 580 , 547 , 516 , 487 , 460 , 434,
            410 , 387 , 365 , 345 , 325 , 307 , 290 , 274 , 258 , 244 , 230 , 217,
            205 , 193 , 183 , 172 , 163 , 154 , 145 , 137 , 129 , 122 , 115 , 109,
            102 , 96  , 91  , 86  , 81  , 77  , 72  , 68  , 64  , 61  , 57  , 54 },
           {1628, 1536, 1450, 1368, 1292, 1220, 1150, 1086, 1026, 968 , 914 , 862,  // Finetune +7
            814 , 768 , 725 , 684 , 646 , 610 , 575 , 543 , 513 , 484 , 457 , 431,
            407 , 384 , 363 , 342 , 323 , 305 , 288 , 272 , 256 , 242 , 228 , 216,
            204 , 192 , 181 , 171 , 161 , 152 , 144 , 136 , 128 , 121 , 114 , 108,
            102 , 96  , 90  , 85  , 80  , 76  , 72  , 68  , 64  , 60  , 57  , 54 },
           {1814, 1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016, 960,  // Finetune -8
            907 , 856 , 808 , 762 , 720 , 678 , 640 , 604 , 570 , 538 , 508 , 480,
            453 , 428 , 404 , 381 , 360 , 339 , 320 , 302 , 285 , 269 , 254 , 240,
            226 , 214 , 202 , 190 , 180 , 170 , 160 , 151 , 143 , 135 , 127 , 120,
            113 , 107 , 101 , 95  , 90  , 85  , 80  , 75  , 71  , 67  , 63  , 60 },
           {1800, 1700, 1604, 1514, 1430, 1350, 1272, 1202, 1134, 1070, 1010, 954,  // Finetune -7
            900 , 850 , 802 , 757 , 715 , 675 , 636 , 601 , 567 , 535 , 505 , 477,
            450 , 425 , 401 , 379 , 357 , 337 , 318 , 300 , 284 , 268 , 253 , 238,
            225 , 212 , 200 , 189 , 179 , 169 , 159 , 150 , 142 , 134 , 126 , 119,
            112 , 106 , 100 , 94  , 89  , 84  , 79  , 75  , 71  , 67  , 63  , 59 },
           {1788, 1688, 1592, 1504, 1418, 1340, 1264, 1194, 1126, 1064, 1004, 948,  // Finetune -6
            894 , 844 , 796 , 752 , 709 , 670 , 632 , 597 , 563 , 532 , 502 , 474,
            447 , 422 , 398 , 376 , 355 , 335 , 316 , 298 , 282 , 266 , 251 , 237,
            223 , 211 , 199 , 188 , 177 , 167 , 158 , 149 , 141 , 133 , 125 , 118,
            111 , 105 , 99  , 94  , 88  , 83  , 79  , 74  , 70  , 66  , 62  , 59 },
           {1774, 1676, 1582, 1492, 1408, 1330, 1256, 1184, 1118, 1056, 996 , 940,  // Finetune -5
            887 , 838 , 791 , 746 , 704 , 665 , 628 , 592 , 559 , 528 , 498 , 470,
            444 , 419 , 395 , 373 , 352 , 332 , 314 , 296 , 280 , 264 , 249 , 235,
            222 , 209 , 198 , 187 , 176 , 166 , 157 , 148 , 140 , 132 , 125 , 118,
            111 , 104 , 99  , 93  , 88  , 83  , 78  , 74  , 70  , 66  , 62  , 59 },
           {1762, 1664, 1570, 1482, 1398, 1320, 1246, 1176, 1110, 1048, 988 , 934,  // Finetune -4
            881 , 832 , 785 , 741 , 699 , 660 , 623 , 588 , 555 , 524 , 494 , 467,
            441 , 416 , 392 , 370 , 350 , 330 , 312 , 294 , 278 , 262 , 247 , 233,
            220 , 208 , 196 , 185 , 175 , 165 , 156 , 147 , 139 , 131 , 123 , 117,
            110 , 104 , 98  , 92  , 87  , 82  , 78  , 73  , 69  , 65  , 61  , 58 },
           {1750, 1652, 1558, 1472, 1388, 1310, 1238, 1168, 1102, 1040, 982 , 926,  // Finetune -3
            875 , 826 , 779 , 736 , 694 , 655 , 619 , 584 , 551 , 520 , 491 , 463,
            437 , 413 , 390 , 368 , 347 , 328 , 309 , 292 , 276 , 260 , 245 , 232,
            219 , 206 , 195 , 184 , 174 , 164 , 155 , 146 , 138 , 130 , 123 , 116,
            109 , 103 , 97  , 92  , 87  , 82  , 77  , 73  , 69  , 65  , 61  , 58 },
           {1736, 1640, 1548, 1460, 1378, 1302, 1228, 1160, 1094, 1032, 974 , 920,  // Finetune -2
            868 , 820 , 774 , 730 , 689 , 651 , 614 , 580 , 547 , 516 , 487 , 460,
            434 , 410 , 387 , 365 , 345 , 325 , 307 , 290 , 274 , 258 , 244 , 230,
            217 , 205 , 193 , 183 , 172 , 163 , 154 , 145 , 137 , 129 , 122 , 115,
            108 , 102 , 96  , 91  , 86  , 81  , 77  , 72  , 68  , 64  , 61  , 57 },
           {1724, 1628, 1536, 1450, 1368, 1292, 1220, 1150, 1086, 1026, 968 , 914,  // Finetune -1
            862 , 814 , 768 , 725 , 684 , 646 , 610 , 575 , 543 , 513 , 484 , 457,
            431 , 407 , 384 , 363 , 342 , 323 , 305 , 288 , 272 , 256 , 242 , 228,
            216 , 203 , 192 , 181 , 171 , 161 , 152 , 144 , 136 , 128 , 121 , 114,
            108 , 101 , 96  , 90  , 85  , 80  , 76  , 72  , 68  , 64  , 60  , 57 }
        };

        public static string GetNoteNameToIndex(int index) {
            if (index == 0) {
                return "...";  // No Note
            }
            if (index == 97) {
                return "==="; // Note cut value
            }
            return (noteStrings[index % 12] + ((int)(index / 12) + 3));
        }

        public static byte GetNoteIndexForPeriod(int period) {
            if (period == 0) {
                return 0;      // No Note
            }

            byte note = 0;            
            for (uint j = 0; j < 59; j++) {
                uint period1 = GetNotePeriod(j, 0);
                uint period2 = GetNotePeriod(j + 1, 0);
                int diff1 = (int)(period1 - period);
                int diff2 = (int)(period - period2);
                if (period1 >= period && period >= period2) {
                    if (diff1 < diff2) {
                        return note;
                    }
                    else {
                        return (byte)(note + 1);
                    }
                }
                note++;
            }
            return note;
        }

        public static uint GetNoteFreq(int note, int finetune) {
            //float period = 1920 - (note + 14) * 16 - finetune / 8;
            //int frequency = (int)(8363 * Math.Pow(2, (1152 - period) / 192));
            uint frequency = ModuleConst.PERIOD_TABLE[finetune, note];
            return frequency;
        }

        public static uint GetNotePeriod(uint note, int finetune) {
            //int frequency = GetNoteFreq(note, finetune);
            //return (int)(ModuleConst.AMIGA_FREQUENCY / frequency);
            return ModuleConst.PERIOD_TABLE[finetune, note];
        }

    }

    /// <summary>
    /// ModuleUtils some useful functions
    /// </summary>
    public static class ModuleUtils {

        public static string GetAsHex(uint value, uint digits) {
            return value.ToString("X" + digits.ToString());
        }

        public static string GetAsDec(int value, int digits) {
            return value.ToString("D" + digits.ToString());
        }

        public static string ReadString0(Stream stream, int len) {
            string res = "";
            bool stopString = false;
            for (int i = 0; i < len; i++) {
                int s = stream.ReadByte();
                if (s == 0) {
                    stopString = true;
                }
                if (s != 0 && !stopString) {
                    res += (char)s;
                }
            }
            return res;
        }

        public static ushort ReadWord(Stream stream) {
            byte[] data = new byte[2];
            stream.Read(data, 0, 2);
            return BitConverter.ToUInt16(data, 0);
        }

        public static ushort ReadWordSwap(Stream stream) {
            byte[] data = new byte[2];
            stream.Read(data, 0, 2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public static uint ReadDWord(Stream stream) {
            uint b1 = ReadWord(stream);
            uint b2 = ReadWord(stream);
            return b1 + b2 * 65536;
        }

        public static byte ReadByte(Stream stream) {
            byte[] b = new byte[1];
            stream.Read(b);
            return b[0];
        }

        public static int ReadSignedByte(Stream stream) {
            byte[] b = new byte[1];
            stream.Read(b);
            return ((b[0] & 0x80) == 0) ? b[0] : b[0] - 256;
        }

        public static int ReadSignedWordSwap(Stream stream) {
            byte[] data = new byte[2];
            stream.Read(data, 0, 2);
            //Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }


    }

    public class ModulePatternChannel {
    }

    public class ModulePatternRow {

        public List<ModulePatternChannel> patternChannels = new List<ModulePatternChannel>();

        public override string ToString() {
            string res = "";
            for (int i = 0; i < patternChannels.Count; i++)
                res += patternChannels[i].ToString() + '|';
            return res;
        }

        public void Clear() {
            patternChannels.Clear();
        }

    }

    public class ModulePattern {

        public List<ModulePatternRow> patternRows = new List<ModulePatternRow>();

        public void Clear() {
            patternRows.Clear();
        }

    }

    public class ModuleInstrument {
        public uint orderNumber = 0;                        //
        public uint sizeInBytes = 0;                  //
        public uint length = 0;                       //
        public float volume = 0;                    // Basisvolume
        public int fineTune = 0;                    // Finetuning -8..+8

        public string name = "";
        public uint samplesNumber = 0;

        public uint headerSize = 0;

        public ModuleInstrument() {
        }

        public virtual float this[float index] {
            get {
                return 0.0f;
            }
            set {
            }
        }

        public virtual float CalculateInstrumentPosition(float pos) {
            return 0.0f;
        }

        public override string ToString() {
            return ToShortString();
        }

        public virtual string ToShortString() {
            return name;
        }

        public virtual void ReadHeaderFromStream(Stream stream, ref uint orderNumber) {
        }

        public virtual void ReadSampleDataFromStream(Stream stream) {
        }

        public virtual void Clear() {
        }

    }

    public class ModuleInstrumentsList {
        protected List<ModuleInstrument> instruments = new List<ModuleInstrument>();

        public ModuleInstrument this[int index] {
            get {
                return instruments[index];
            }
            set {
            }
        }

        public void Add(ModuleInstrument instrument) {
            instruments.Add(instrument);
        }

        public int Count {
            get {
                return instruments.Count;
            }
        }

        public List<ModuleInstrument> List {
            get {
                return instruments;
            }
        }

        public void Clear() {
            for (int i = 0; i < instruments.Count; i++) {
                instruments[i].Clear();
            }
            instruments.Clear();

        }

    }

    public class Module : IDisposable {

        protected readonly string moduleName        = "Base module";
        protected float position                    = 0;

        public string fileName                      = "";
        public long fileLength                      = 0;
        public string trackerName                   = "";
        public uint version                         = 0;
        public uint headerSize                      = 0;
        public uint songLength                      = 0;
        public uint songRepeatPosition              = 0;

        public string moduleID                      = "";
        public string songName                      = "";
        public uint numberOfChannels                = 0;
        public uint numberOfInstruments             = 0;
        public uint numberOfSamples                 = 0;
        public uint numberOfPatterns                = 0;
        public uint BPM                             = 125;
        public uint tempo                           = 6;

        public float baseVolume                     = 0.0f;
        public bool isAmigaLike                     = true;

        public List<uint> arrangement               = new List<uint>();
        public List<ModulePattern> patterns         = new List<ModulePattern>();

        protected ModuleInstrumentsList instruments = new ModuleInstrumentsList();
        protected ModuleMixer mixer                 = null;

        public float Position {
            get {
                return position;
            }
            set {
                position = value;
                RewindToPosition();
            }
        }

        public ModuleInstrumentsList Instruments {
            get {
                return instruments;
            }

            set {
            }
        }

        public ModulePattern this[int index] {
            get {
                int patNum = (int)arrangement[index];
                return patterns[patNum];
            }
            set {
            }
        }

        public virtual float InstrumentVolume(byte instrumentNumber) {
            return 0;
        }

        public Module(string Name) {
            moduleName = Name;
        }

        public virtual bool CheckFormat(Stream stream) {
            return false;
        }

        public virtual bool ReadFromStream(Stream stream) {
            return true;
        }

        public virtual void RewindToPosition() {
        }

        public virtual void Play() {
        }

        public virtual void PlayInstrument(uint instrumentNumber = 0) {
        }

        public virtual void Stop() {
        }

        public virtual void Pause() {
        }

        public virtual void Dispose() {
        }

        protected void DebugMes(string mes) {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("Module " + moduleName + " -> " + mes);
            #endif
        }

    }
 
    /// <summary>
    /// MixerChannel data description used in sound mix process
    /// </summary>
    public class ModuleMixerChannel {

        public bool muted                   = false;

        public uint effect                  = 0;
        public uint effectArg               = 0;
        public uint effectArgX              = 0;
        public uint effectArgY              = 0;

        public int portamentoStart          = 0;
        public int portamentoEnd            = 0;
        public int portamentoStep           = 0;
        public int lastPortamentoStep       = 0;

        public bool volumeSlideStart        = false;
        public float volumeSlideX           = 0;
        public float volumeSlideY           = 0;
        public float volumeSlideStep        = 0;

        public uint patternJumpCounter      = 0;
        public uint patternNumToJump        = 0;
        public uint positionToJump          = 0;

        public uint arpeggioPeriod0         = 0;
        public uint arpeggioPeriod1         = 0;
        public uint arpeggioPeriod2         = 0;
        public uint arpeggioCount           = 0;
        public uint arpeggioIndex           = 0;
        public uint arpeggioX               = 0;
        public uint arpeggioY               = 0;

        public uint vibratoType             = 0;
        public int vibratoStart             = 0;
        public int vibratoPeriod            = 0;
        public float vibratoAdd             = 0;
        public int vibratoCount             = 0;
        public int vibratoAmp               = 0;
        public int vibratoFreq              = 0;
        public int lastVibratoAmp           = 0;
        public int lastVibratoFreq          = 0;

        public uint tremoloType             = 0;
        public float tremoloStart           = 0;
        public int tremoloAdd               = 0;
        public int tremoloCount             = 0;
        public int tremoloAmp               = 0;
        public int tremoloFreq              = 0;

        public uint noteIndex               = 0;
        public bool isNote                  = false;

        public int currentFineTune          = 0;
        public int lastFineTune             = 0;
        public uint period                  = 0;
        public float periodInc              = 0;
        public float instrumentPosition     = 2;
        public int instrumentDirection      = 1;
        public uint loopType                = ModuleConst.LOOP_ON;
        public ushort instrumentIndex       = 0;
        public ushort instrumentLastIndex   = 0;
        public float instrumentLoopStart    = 0;
        public float instrumentLoopEnd      = 0;
        public uint instrumentLength         = 0;
        public float instrumentVolume       = 1.0f;
        public float channelVolume          = 1.0f;

        public virtual void CalculateInstrumentPosition() {
        }

    }
   
    /// <summary>
    /// ModuleMixer the main class where sound created
    /// </summary>
    public class ModuleMixer {

        protected bool playing          = false;
        protected bool moduleEnd        = false;
        protected bool pattEnd          = false;
        protected uint counter          = 0;
        protected uint speed            = 6;
        protected uint patternDelay     = 0;
        protected uint BPM              = 125;
        protected uint maxPatternRows   = 64;
        protected uint samplesPerTick   = 0;
        protected uint mixerPosition    = 0;
        protected uint currentRow       = 0;
        protected uint track            = 0;
        protected bool mixLoop          = true;

        protected Module module         = null;
        protected ModulePattern pattern = null;

        protected List<ModuleMixerChannel> mixChannels = new List<ModuleMixerChannel>();

        protected List<Func<ModuleMixerChannel, bool>> noteEffects    = new List<Func<ModuleMixerChannel, bool>>();
        protected List<Func<ModuleMixerChannel, bool>> tickEffects    = new List<Func<ModuleMixerChannel, bool>>();
        protected List<Func<ModuleMixerChannel, bool>> effectsE       = new List<Func<ModuleMixerChannel, bool>>();

        protected List<bool> noteEffectsUsed  = new List<bool>();
        protected List<bool> tickEffectsUsed  = new List<bool>();
        protected List<bool> effectsEUsed     = new List<bool>();

        protected PlayBackWaveProvider waveStream = null;
        protected WaveOutEvent waveOut            = null;        
        protected Task MixingTask                 = null;

        protected bool NoEffect(ModuleMixerChannel mc)               // no effect
        {
            return false;
        }

        public ModuleMixer(Module module) {
            this.module = module;
            for (int i = 0; i < 34; i++) {
                noteEffectsUsed.Add(false);
                tickEffectsUsed.Add(false);
                effectsEUsed.Add(false);
            }

            /*
			for (int i = 0; i < 34; i++)
            {
				NoteEffects.Add(NoEffect);
				TickEffects.Add(NoEffect);
				effectsE.Add(NoEffect);
			}
			return; // without effects;
			*/

        }

        protected void SetBPM() {
            mixerPosition = 0;
            samplesPerTick = CalcSamplesPerTick(BPM);
        }

        protected void NextRow() {
            currentRow++;
            if (currentRow >= pattern.patternRows.Count) {
                pattEnd = true;
                currentRow = 0;
                track++;
                if (track >= module.songLength) {
                    moduleEnd = true;
                    //if (!mixLoop) played = true;
                    //else
                    //{
                    //    track = 0;
                    //    pattern = module.patterns[module.arrangement[track]];
                    //}
                }
                else {
                    pattern = module[(int)track];
                }
            }
        }

        public virtual uint GetNotePeriod(uint note, int fineTune) {
            uint period = (uint)(7680 - (note + 24) * 64 - fineTune * 8);
            return period;
        }

        public uint GetNoteFreq(uint note, int fineTune) {
            float period = GetNotePeriod(note, fineTune);
            float frequency = (float)(8363 * Math.Pow(2, (4608.0f - period) * 0.001302083f));
            return (uint)(ModuleConst.HALF_AMIGA_FREQUENCY / frequency);
        }

        public float CalcPeriodIncrement(uint period) {
            return (period != 0) ? (ModuleConst.HALF_AMIGA_FREQUENCY / period) / ModuleConst.SOUNDFREQUENCY : 1.0f;
        }

        public float CalcClampPeriodIncrement(ModuleMixerChannel mc) {
            mc.period = (mc.period < ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.period;
            mc.period = (mc.period > ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.period;
            return (mc.period != 0) ? (ModuleConst.HALF_AMIGA_FREQUENCY / mc.period) / ModuleConst.SOUNDFREQUENCY : 1.0f;
        }


        public uint CalcSamplesPerTick(uint currentBPM) {
            return (currentBPM <= 0) ? 0 : (uint)(ModuleConst.SOUNDFREQUENCY * 2.5f / currentBPM);
        }

        /*
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            startTime.Stop();
            var resultTime = startTime.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                                                resultTime.Hours,
                                                resultTime.Minutes,
                                                resultTime.Seconds,
                                                resultTime.Milliseconds);
        */

        public float GetSampleValue(ModuleMixerChannel mc) {
            if (mc.instrumentIndex <= 0) {
                return 0.0f;
            }
            ModuleInstrument sampleData = module.Instruments[(int)mc.instrumentIndex - 1];
            mc.instrumentPosition = sampleData.CalculateInstrumentPosition(mc.instrumentPosition);
            return GetSampleValueSimple(mc);
        }

        protected virtual float GetSampleValueSimple(ModuleMixerChannel mc) {
            ModuleInstrument sampleData = module.Instruments[(int)mc.instrumentIndex - 1];
            return sampleData[(int)mc.instrumentPosition] * mc.channelVolume;
        }

        protected virtual float GetSampleValueLinear(ModuleMixerChannel mc) {
            ModuleInstrument sampleData = module.Instruments[(int)mc.instrumentIndex - 1];
            int posInt = (int)mc.instrumentPosition;
            float posReal = mc.instrumentPosition - posInt;
            float a1 = sampleData[posInt];
            if ((posInt + 1) >= mc.instrumentLength)
                return a1 * mc.channelVolume;
            float a2 = sampleData[posInt + 1];
            return (a1 + (a2 - a1) * posReal) * mc.channelVolume;
        }

        protected virtual float GetSampleValueSpline(ModuleMixerChannel mc) {
            ModuleInstrument sampleData = module.Instruments[(int)mc.instrumentIndex - 1];
            int posInt = (int)mc.instrumentPosition;
            float posReal = mc.instrumentPosition - posInt;
            float a2 = sampleData[posInt];
            if (((posInt - 1) < 0) || ((posInt + 2) >= mc.instrumentLength))
                return a2 * mc.channelVolume;
            float a1 = sampleData[posInt - 1];
            float a3 = sampleData[posInt + 1];
            float a4 = sampleData[posInt + 2];

            float b0 = a1 + a2 + a2 + a2 + a2 + a3;
            float b1 = -a1 + a3;
            float b2 = a1 - a2 - a2 + a3;
            float b3 = -a1 + a2 + a2 + a2 - a3 - a3 - a3 + a4;
            return (((b3 * posReal * 0.1666666f + b2 * 0.5f) * posReal + b1 * 0.5f) * posReal + b0 * 0.1666666f) * mc.channelVolume;
        }

        protected void UpdateNoteEffects() {
            for (int ch = 0; ch < module.numberOfChannels; ch++) {
                noteEffects[(int)mixChannels[ch].effect](mixChannels[ch]);
                if (mixChannels[ch].effect == 0 && mixChannels[ch].effectArg != 0) {
                    noteEffectsUsed[0] = true;
                }
                if (mixChannels[ch].effect != 0) {
                    noteEffectsUsed[(int)mixChannels[ch].effect] = true;
                }
            }
        }

        protected void UpdateTickEffects() {
            for (int ch = 0; ch < module.numberOfChannels; ch++) {
                tickEffects[(int)mixChannels[ch].effect](mixChannels[ch]);
                if (mixChannels[ch].effect == 0 && mixChannels[ch].effectArg != 0) {
                    tickEffectsUsed[0] = true;
                }
                if (mixChannels[ch].effect != 0) {
                    tickEffectsUsed[(int)mixChannels[ch].effect] = true;
                }
            }
        }

        protected virtual void UpdateNote() {
        }

        protected virtual void UpdateBPM() {
        }

        protected virtual void ResetChannelInstrument(ModuleMixerChannel mc) {
        }

        protected virtual ModuleMixerChannel CreateMixerChannel() {
            return new ModuleMixerChannel();
        }

        protected virtual void InitModule(uint startPosition = 0) {
            track = startPosition;
            counter = 0;
            patternDelay = 0;
            samplesPerTick = 0;
            mixerPosition = 0;
            currentRow = 0;
            pattern = module[(int)track];
            BPM = module.BPM;
            speed = module.tempo;
            moduleEnd = false;

            mixChannels.Clear();
            for (int ch = 0; ch < module.numberOfChannels; ch++) {
                ModuleMixerChannel mc = CreateMixerChannel();
                mc.instrumentIndex = 0;
                mc.instrumentLastIndex = 0;
                mixChannels.Add(mc);
            }

            SetBPM();
            UpdateBPM();
        }

        protected virtual void MixData() {
            string ms = " channels " + module.numberOfChannels + " ";
            for (int pos = 0; pos < ModuleConst.MIX_LEN; pos++) {
                float mixValueR = 0;
                float mixValueL = 0;
                float mixValue = 0;
                for (int ch = 0; ch < module.numberOfChannels; ch++) {
                    ModuleMixerChannel mc = mixChannels[ch];
                    //if (ch != 1) mc.muted = true;
                    if (!mc.muted) {
                        mixValue += GetSampleValue(mc);

                        //if (ModuleConst.MIX_CHANNELS == ModuleConst.STEREO) {
                        //    mixValueL += (((ch & 0x03) == 0) || ((ch & 0x03) == 3)) ? mixValue * 0.75f : mixValue * 0.25f;
                        //    mixValueR += (((ch & 0x03) == 1) || ((ch & 0x03) == 2)) ? mixValue * 0.75f : mixValue * 0.25f;
                        //}

                    }
                    mc.instrumentPosition += mc.periodInc;
                }
                if (ModuleConst.MIX_CHANNELS != ModuleConst.STEREO) {
                    mixValue /= module.numberOfChannels;
                }
                //else {
                //    mixValueL /= (module.numberOfChannels << 1);
                //    mixValueR /= (module.numberOfChannels << 1);
                //}

                if (ModuleConst.MIX_CHANNELS != ModuleConst.STEREO) {
                    waveStream.Write((short)mixValue);
                }
                //else {
                //    waveStream.Write((short)mixValueL);
                //    waveStream.Write((short)mixValueR);
                //}

                mixerPosition++;
                if (mixerPosition >= samplesPerTick) {
                    SetBPM();
                    UpdateBPM();
                }

            }

        }

        public virtual void PlayModule(uint startPosition = 0) {
            playing = false;
            InitModule(startPosition);

            waveOut?.Stop();
            MixingTask?.Wait();

            waveStream?.Dispose();
            waveOut?.Dispose();
            MixingTask?.Dispose();

            waveStream = new PlayBackWaveProvider(ModuleConst.SOUNDFREQUENCY /* module.numberOfChannels*/, ModuleConst.MIX_CHANNELS == ModuleConst.STEREO);
            playing = true;

            MixingTask = Task.Factory.StartNew(() => {
                while ((!moduleEnd) && playing) {
                    MixData();
                    while (waveStream.QueueLength > ModuleConst.MIX_WAIT && playing) {
                        Thread.Sleep(100);
                    }
                    //DebugMes("Play position - " + waveOut.GetPosition() + " queue length - " + waveStream.QueueLength);
                }
                while (waveStream.QueueLength > 0 && playing) {
                    Thread.Sleep(100);
                    //DebugMes("Play position - " + waveOut.GetPosition() + " queue length - " + waveStream.QueueLength);
                }
            });

            waveOut = new WaveOutEvent();
            waveOut.Init(waveStream);
            waveOut.Play();
        }

        public void Stop() {
            playing = false;
            waveOut.Stop();
        }

        public void DebugMes(string mes) {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine(mes);
            #endif
        }

    }

}

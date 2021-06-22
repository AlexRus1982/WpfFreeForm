using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModuleSystem {
    
    public class XM_Sample {
        public uint orderNumber = 0;                        //
        public uint sizeInBytes = 0;                        //
        public uint length = 0;                             //
        public uint loopStart = 0;                          //
        public uint loopLength = 0;                         //
        public uint loopEnd = 0;                            //
        public float volume = 0;                            //
        public sbyte finetune = 0;                          // -128..+127;
        public byte type = 0;                               // Bit 0-1: 0 = No loop
                                                            //		    1 = Forward loop
                                                            //		    2 = Bidirectional loop(aka ping-pong)
                                                            // Bit 4  : 0 = 8-bit samples
                                                            //		    1 = 16-bit samples
        public byte loopType = 0;
        public bool is16Bits = false;
        public byte panning = 0;                            // 0..255
        public sbyte relativeNoteNumber = 0;                // (signed byte)
        public byte packedType = 0;                         // 00 = regular delta packed sample data
                                                            // AD = 4-bit ADPCM-compressed sample data
        public string name = "";                            // 22 - chars
        public List<float> sampleData = new List<float>();  // 8- or 16-bit packed values

        public XM_Sample() {
        }

        public float this[float index] {
            get {
                if (index < 0) {
                    return 0.0f;
                }

                if ((index >= length) && (loopType == 0x00)) {
                    return 0.0f;
                }

                if ((index >= loopEnd) && (loopType == 0x01)) {
                    int pos = (int)(index - loopEnd) % (int)loopLength + (int)loopStart;
                    return sampleData[pos];
                }

                if ((index >= loopStart) && (loopType == 0x02)) {
                    int iter = (int)(index - loopStart) / (int)loopLength;
                    if (iter % 2 == 0) {
                        int pos = (int)loopStart + (int)(index - loopStart) % (int)loopLength;
                        return sampleData[pos];
                    }
                    if (iter % 2 == 1) {
                        int pos = (int)loopEnd - (int)(index - loopStart) % (int)loopLength;
                        return sampleData[pos - 1];
                    }
                    return 0.0f;
                }

                return sampleData[(int)index];
            }
        }

        public float CalculateInstrumentPosition(float pos) {
            if (pos < 0) {
                return 0.0f;
            }

            if ((pos >= length) && (loopType == 0x00)) {
                return length;
            }

            if ((pos >= loopEnd) && (loopType == 0x01)) {
                return (int)loopStart + (int)(pos - loopEnd) % (int)loopLength;
            }

            if ((pos >= loopStart) && (loopType == 0x02)) {
                int iter = (int)(pos - loopStart) / (int)loopLength;
                if (iter % 2 == 0) {
                    return pos;
                }
                if (iter % 2 == 1) {
                    return (int)loopEnd - (int)(pos - loopStart) % (int)loopLength - 1;
                }
                return pos;
            }
            return pos;
        }

        public override string ToString() {
            string loopType = "Off";
            loopType        = ((type & 0x01) != 0) ? "Forward" : loopType;
            loopType        = ((type & 0x02) != 0) ? "Bidirectional" : loopType;
            string res      = name.Trim().PadRight(22, '.');

            if (sizeInBytes > 0) {
                res +=  " ( size : " + length + " byte(s), "
                        + (is16Bits ? "16-bit" : "8-bit") + ", "
                        + "volume : " + volume + ", "
                        + "loop : " + type + " = " + loopType + ", ";
                
                if ((type & 0x03) != 0) {
                    res += "loopStart : " + loopStart + ", " + "loopEnd : " + loopEnd + ", " + "loopLength : " + loopLength + ", ";
                }
                
                res += "packed - " + packedType + ((packedType != 0xAD) ? " - regular delta" : " - ADPCM-compressed") + ")";
            }

            return res;

        }

        public string ToShortString() {
            return name;
        }

        public void readHeaderFromStream(Stream stream) {
            long startSamplePosition = stream.Position;

            sizeInBytes         = ModuleUtils.ReadDWord(stream);
            loopStart           = ModuleUtils.ReadDWord(stream);
            loopLength          = ModuleUtils.ReadDWord(stream);
            byte vol            = ModuleUtils.ReadByte(stream);
            volume              = (vol >= 64) ? 1.0f : (float)vol / 64.0f;
            finetune            = (sbyte)ModuleUtils.ReadSignedByte(stream);
            type                = ModuleUtils.ReadByte(stream);
            panning             = ModuleUtils.ReadByte(stream);
            relativeNoteNumber  = (sbyte)ModuleUtils.ReadSignedByte(stream);
            packedType          = ModuleUtils.ReadByte(stream);
            name                = ModuleUtils.ReadString0(stream, 22);

            length              = ((type & 0x10) != 0) ? sizeInBytes >> 1 : sizeInBytes;
            loopType            = (byte)(type & 0x03);
            is16Bits            = (type & 0x10) != 0;
            

            loopStart           = is16Bits ? loopStart >> 1  : loopStart;
            loopLength          = is16Bits ? loopLength >> 1 : loopLength;
            loopEnd             = loopStart + loopLength;
            loopEnd             = (loopEnd > length) ? length : loopEnd;
            loopStart           = (loopStart > length) ? length : loopStart;

            relativeNoteNumber = 0;
            //stream.Seek(startSamplePosition + 40, SeekOrigin.Begin);
        }

        // NOTE : this function load regular delta packed sample data
        // TODO : need to do loading of AD = 4-bit ADPCM-compressed sample data
        public void readSampleDataFromStream(Stream stream) {
            sampleData.Clear();
            float ampDivider = ((type & 0x10) != 0) ? 0.000030517578125f /* 1/32768 */ : 0.0078125f /* 1/128 */;
            if (length > 0) {
                float oldValue = 0;
                for (uint i = 0; i < length; i++) {
                    float sampleValue;
                    sampleValue = ((type & 0x10) != 0) ? ModuleUtils.ReadSignedWordSwap(stream) : ModuleUtils.ReadSignedByte(stream);
                    oldValue += sampleValue * ampDivider;
                    if (oldValue < -1) {
                        oldValue += 2;
                    }
                    else if (oldValue > 1) {
                        oldValue -= 2;
                    }
                    sampleData.Add(oldValue * ModuleConst.SOUND_AMP);
                }
            }
        }

        public void Clear() {
            sampleData.Clear();
        }

    }

    public class XM_Instrument : ModuleInstrument {
        public uint instrumentSize = 0;
        public byte instrumentType = 0;

        public List<byte> keymapAssignements = new List<byte>();
        public List<ushort> pointsForVolumeEnvelope = new List<ushort>();
        public List<ushort> pointsForPanningEnvelope = new List<ushort>();
        public byte numberOfVolumePoints = 0;
        public byte numberOfPanningPoints = 0;
        public byte volumeSustainPoint = 0;
        public byte volumeLoopStartPoint = 0;
        public byte volumeLoopEndPoint = 0;
        public byte panningSustainPoint = 0;
        public byte panningLoopStartPoint = 0;
        public byte panningLoopEndPoint = 0;
        public byte volumeType = 0; // bit 0: On; 1: Sustain; 2: Loop;
        public byte panningType = 0; // bit 0: On; 1: Sustain; 2: Loop;
        public byte vibratoType = 0;
        public byte vibratoSweep = 0;
        public byte vibratoDepth = 0;
        public byte vibratoRate = 0;
        public ushort volumeFadeout = 0;
        public List<byte> reserved = new List<byte>(22); // reserved 22 bytes
        public List<XM_Sample> samples = new List<XM_Sample>();
        public XM_Sample sample = null;

        public XM_Instrument() {
        }

        public override float this[float index] {
            get {
                return sample[index];
            }
        }

        public override float CalculateInstrumentPosition(float pos) {
            return sample.CalculateInstrumentPosition(pos);
        }

        public override string ToString() {
            string res = name.Trim().PadRight(22, '.');
            if (samples.Count > 0) {
                res += " ( sample(s) : ";
                foreach (XM_Sample sample in samples) {
                    res += sample.orderNumber + " ";
                }
                res += ")";
            }
            return res;
        }

        public void ReadFromStream(Stream stream, ref uint sampleOrder) {
            long startInstrumentPosition = stream.Position;
            instrumentSize = ModuleUtils.ReadDWord(stream);
            name = ModuleUtils.ReadString0(stream, 22);
            instrumentType = ModuleUtils.ReadByte(stream); // Length
            samplesNumber = ModuleUtils.ReadWord(stream);
            samples.Clear();

            if (samplesNumber > 0) {
                headerSize = ModuleUtils.ReadDWord(stream);
                for (uint i = 0; i < 96; i++) {
                    keymapAssignements.Add(ModuleUtils.ReadByte(stream));
                }
                for (uint i = 0; i < 24; i++) {
                    pointsForVolumeEnvelope.Add(ModuleUtils.ReadWord(stream));
                }
                for (uint i = 0; i < 24; i++) {
                    pointsForPanningEnvelope.Add(ModuleUtils.ReadWord(stream));
                }
                numberOfVolumePoints = ModuleUtils.ReadByte(stream);
                numberOfPanningPoints = ModuleUtils.ReadByte(stream);
                volumeSustainPoint = ModuleUtils.ReadByte(stream);
                volumeLoopStartPoint = ModuleUtils.ReadByte(stream);
                volumeLoopEndPoint = ModuleUtils.ReadByte(stream);
                panningSustainPoint = ModuleUtils.ReadByte(stream);
                panningLoopStartPoint = ModuleUtils.ReadByte(stream);
                panningLoopEndPoint = ModuleUtils.ReadByte(stream);
                volumeType = ModuleUtils.ReadByte(stream); // bit 0: On; 1: Sustain; 2: Loop;
                panningType = ModuleUtils.ReadByte(stream); // bit 0: On; 1: Sustain; 2: Loop;
                vibratoType = ModuleUtils.ReadByte(stream);
                vibratoSweep = ModuleUtils.ReadByte(stream);
                vibratoDepth = ModuleUtils.ReadByte(stream);
                vibratoRate = ModuleUtils.ReadByte(stream);
                volumeFadeout = ModuleUtils.ReadWord(stream);
                //for (uint i = 0; i < 22; i++)	reserved.Add(ModuleUtils.ReadByte(stream));

                //System.Diagnostics.Debug.WriteLine("Seek : " + stream.Position + " " + (instrumentSize - 243) + "\n");
                stream.Seek(instrumentSize - 241, SeekOrigin.Current);

                long sampleDataLength = 0;
                for (uint i = 0; i < samplesNumber; i++) {
                    var sample = new XM_Sample();
                    sample.readHeaderFromStream(stream);
                    sample.orderNumber = sampleOrder++;
                    samples.Add(sample);
                    sampleDataLength += sample.sizeInBytes;
                }

                long startSampleDataPosition = stream.Position;
                foreach (XM_Sample sample in samples) {
                    sample.readSampleDataFromStream(stream);
                }
                stream.Seek(startSampleDataPosition + sampleDataLength, SeekOrigin.Begin);
            }
            else {
                stream.Seek(startInstrumentPosition + instrumentSize, SeekOrigin.Begin);
            }

            sample = (samples.Count > 0) ? samples[0] : null;
            if (sample != null) {
                volume = sample.volume;
            }
        }

        public override void Clear() {
            foreach (XM_Sample sample in samples) {
                sample.Clear();
            }
            samples.Clear();
        }

    }

    public class XM_PatternChannel : MOD_PatternChannel {

        public byte volumeEffect = 0;
        public bool cutNote = false;

        public static string[] effectStrings =
        {
             "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
             "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
             "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
             "U", "V", "W", "X"
        };

        private static string GetNoteNameToIndex(byte index) {
            if (index == 0) {
                return "...";  // No Note
            }
            if (index == 97) {
                return "==="; // Note cut value
            }
            return (ModuleConst.noteStrings[(index - 1) % 12] + ((index - 1) / 12 + 1));
        }

        private static string getVolumeEffectString(byte volumeEffect) {
            if (volumeEffect >= 0x00 && volumeEffect <= 0x0F) {
                return "...";  // do nothing
            }
            if (volumeEffect >= 0x10 && volumeEffect <= 0x4F) {
                return "V" + ModuleUtils.GetAsDec(volumeEffect - 0x10, 2);  // v 0..63
            }
            if (volumeEffect == 0x50) {
                return "V64";  // v 64
            }
            if (volumeEffect >= 0x51 && volumeEffect <= 0x5F) {
                return "...";  // undefined
            }
            if (volumeEffect >= 0x60 && volumeEffect <= 0x6F) {
                return "D" + ModuleUtils.GetAsDec(volumeEffect - 0x60, 2);  // volumeEffect slide down
            }
            if (volumeEffect >= 0x70 && volumeEffect <= 0x7F) {
                return "C" + ModuleUtils.GetAsDec(volumeEffect - 0x70, 2);  // volumeEffect slide up
            }
            if (volumeEffect >= 0x80 && volumeEffect <= 0x8F) {
                return "B" + ModuleUtils.GetAsDec(volumeEffect - 0x80, 2);  // Fine volumeEffect down
            }
            if (volumeEffect >= 0x90 && volumeEffect <= 0x9F) {
                return "A" + ModuleUtils.GetAsDec(volumeEffect - 0x90, 2);  // Fine volumeEffect up
            }
            if (volumeEffect >= 0xA0 && volumeEffect <= 0xAF) {
                return "U" + ModuleUtils.GetAsDec(volumeEffect - 0xA0, 2);  // Vibrato speed
            }
            if (volumeEffect >= 0xB0 && volumeEffect <= 0xBF) {
                return "H" + ModuleUtils.GetAsDec(volumeEffect - 0xB0, 2);  // Vibrato deph
            }
            if (volumeEffect >= 0xC0 && volumeEffect <= 0xCF) {
                return "P" + ModuleUtils.GetAsDec((volumeEffect - 0xC0) * 4 + 2, 2);  // Set panning (2,6,10,14..62)
            }
            if (volumeEffect >= 0xD0 && volumeEffect <= 0xDF) {
                return "L" + ModuleUtils.GetAsDec(volumeEffect - 0xD0, 2);  // Pan slide left
            }
            if (volumeEffect >= 0xE0 && volumeEffect <= 0xEF) {
                return "R" + ModuleUtils.GetAsDec(volumeEffect - 0xE0, 2);  // Pan slide right
            }
            if (volumeEffect >= 0xF0 && volumeEffect <= 0xFF) {
                return "G" + ModuleUtils.GetAsDec(volumeEffect - 0xF0, 2);  // Tone portamento
            }
            return "...";
        }

        public override string ToString() {
            string res = GetNoteNameToIndex(noteIndex);
            //res += ((period == 0 && noteIndex != 0) || (period != 0 && noteIndex == 0)) ? "!" : " ";
            res += " " + ((instrumentIndex != 0) ? ModuleUtils.GetAsDec(instrumentIndex, 2) : "..");
            res += " " + getVolumeEffectString(volumeEffect) + " ";
            res += ((effekt != 0) && (effekt < 34)) ? effectStrings[effekt] + ModuleUtils.GetAsHex(effektOp, 2) : "...";
            return res;
        }

    }

    public class XM_Pattern : ModulePattern{

        private uint headerLength = 0;
        private byte packingType = 0;
        private uint numberOfRows = 0;
        private uint packedSize = 0;

        private XM_PatternChannel CreateNewPatternChannel(Stream stream, uint numberOfSamples) {
            XM_PatternChannel channel = new XM_PatternChannel();
            byte b0 = ModuleUtils.ReadByte(stream);
            if ((b0 & 0x80) != 0) {
                if ((b0 & 0x01) != 0) {
                    channel.noteIndex = ModuleUtils.ReadByte(stream);
                }
                if ((b0 & 0x02) != 0) {
                    channel.instrumentIndex = ModuleUtils.ReadByte(stream);
                }
                if ((b0 & 0x04) != 0) {
                    channel.volumeEffect = ModuleUtils.ReadByte(stream);
                }
                if ((b0 & 0x08) != 0) {
                    channel.effekt = ModuleUtils.ReadByte(stream);
                }
                if ((b0 & 0x10) != 0) {
                    channel.effektOp = ModuleUtils.ReadByte(stream);
                }
            }
            else {
                channel.noteIndex = (byte)(b0 & 0x7F);
                channel.instrumentIndex = ModuleUtils.ReadByte(stream);
                channel.volumeEffect = ModuleUtils.ReadByte(stream);
                channel.effekt = ModuleUtils.ReadByte(stream);
                channel.effektOp = ModuleUtils.ReadByte(stream);
            }
            if (channel.noteIndex > 0 && channel.noteIndex < 97 && channel.volumeEffect == 0) {
                channel.volumeEffect = 0x50;
            }
            return channel;
        }

        public void ReadPatternData(Stream stream, string moduleID, uint numberOfChannels, uint numberOfSamples) {
            headerLength = ModuleUtils.ReadDWord(stream);
            packingType = ModuleUtils.ReadByte(stream);
            numberOfRows = ModuleUtils.ReadWord(stream);
            packedSize = ModuleUtils.ReadWord(stream);

            long patternEndPosition = stream.Position + packedSize;

            patternRows.Clear();
            for (int row = 0; row < numberOfRows; row++) {
                ModulePatternRow pRow = new ModulePatternRow();
                for (int channel = 0; channel < numberOfChannels; channel++) {
                    pRow.patternChannels.Add(CreateNewPatternChannel(stream, numberOfSamples));
                }

                patternRows.Add(pRow);
            }

            stream.Seek(patternEndPosition, SeekOrigin.Begin);
        }

        public override string ToString() {
            string res = "\n";
            res += "\n" + "headerLength : " + headerLength;
            res += "\n" + "packingType : " + packingType;
            res += "\n" + "numberOfRows : " + numberOfRows;
            res += "\n" + "packedSize : " + packedSize;
            res += "\n";

            if (patternRows[0] != null) {
                string ln = "====";
                for (int i = 0; i < patternRows[0].patternChannels.Count; i++) {
                    ln += "===============";
                }

                res += ln + "\n";

                for (int i = 0; i < patternRows.Count; i++) {
                    res += ModuleUtils.GetAsHex((uint)i, 2) + ":|" + patternRows[i] + "\n";
                }

                res += ln + "\n";
            }
            else {
                res += "empty pattern\n";
            }
            return res;
        }

    }

    public class XM_Module : Module {

        private byte XM_1A_Byte = 0;
        private uint XM_Flags = 0;

        public XM_Module() : base("XM format") {
            DebugMes("XM Sound Module created");
        }

        public string InstrumentsToString() {
            string res = "Instruments info : \n";
            uint i = 1;
            foreach (XM_Instrument inst in Instruments.List) {
                res += ModuleUtils.GetAsDec((int)i++, 3) + " : " + inst.ToString().Trim() + "\n";
            }
            return res;
        }

        public string SamplesToString() {
            string res = "Samples info : \n";
            uint i = 1;
            foreach (XM_Instrument inst in Instruments.List) {
                foreach (XM_Sample sample in inst.samples) {
                    res += ModuleUtils.GetAsDec((int)i++, 3) + " : " + sample.ToString().Trim() + "\n";
                }
            }
            return res;
        }

        public override bool CheckFormat(Stream stream) {
            byte[] bytesID = new byte[17];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytesID);
            stream.Seek(37, SeekOrigin.Begin);
            XM_1A_Byte = (byte)stream.ReadByte();
            moduleID = Encoding.ASCII.GetString(bytesID);
            return moduleID == "Extended Module: " && XM_1A_Byte == 0x1A;
        }

        private void ReadArrangement(Stream stream) {
            arrangement.Clear();
            for (int i = 0; i < 256; i++) {
                byte patNum = ModuleUtils.ReadByte(stream);
                if (i < songLength) {
                    arrangement.Add(patNum);
                }
            }
        }

        private void ReadInstruments(Stream stream) {
            Instruments.Clear();
            for (uint i = 0; i < numberOfInstruments; i++) {
                XM_Instrument inst = new XM_Instrument();
                Instruments.Add(inst);
                inst.ReadFromStream(stream, ref i);
            }
        }

        private void ReadPatterns(Stream stream) {
            foreach (XM_Pattern pat in patterns) {
                pat.Clear();
            }

            patterns.Clear();
            for (int i = 0; i < numberOfPatterns; i++)                      // Read the patterndata
            {
                XM_Pattern pattern = new XM_Pattern();
                pattern.ReadPatternData(stream, moduleID, numberOfChannels, numberOfSamples);
                patterns.Add(pattern);
                //DebugMes("Patern : " + i + pattern.ToString());
            }
        }

        public override bool ReadFromStream(Stream stream) {
            if (!CheckFormat(stream)) {
                return false;
            }

            fileLength = stream.Length;
            baseVolume = 1.0f;
            BPM = 125;
            tempo = 6;

            stream.Seek(0, SeekOrigin.Begin);
            moduleID = ModuleUtils.ReadString0(stream, 17);
            songName = ModuleUtils.ReadString0(stream, 20);
            XM_1A_Byte = (byte)stream.ReadByte();
            trackerName = ModuleUtils.ReadString0(stream, 20);
            version = ModuleUtils.ReadWord(stream);
            headerSize = (uint)ModuleUtils.ReadDWord(stream);
            songLength = ModuleUtils.ReadWord(stream);
            songRepeatPosition = ModuleUtils.ReadWord(stream);
            numberOfChannels = ModuleUtils.ReadWord(stream);
            numberOfPatterns = ModuleUtils.ReadWord(stream);
            numberOfInstruments = ModuleUtils.ReadWord(stream);
            XM_Flags = (uint)ModuleUtils.ReadWord(stream);
            tempo = ModuleUtils.ReadWord(stream);
            BPM = ModuleUtils.ReadWord(stream);

            ReadArrangement(stream);        // read pattern order
            ReadPatterns(stream);           // read patterns
            ReadInstruments(stream);        // read instruments

            DebugMes(InstrumentsToString());
            DebugMes(SamplesToString());

            mixer = new XM_Mixer(this);
            return true;
        }

        public override string ToString() {
            string modInfo = "";
            modInfo += "\n" + "moduleID : " + moduleID;
            modInfo += "\n" + "songName : " + songName;
            modInfo += "\n" + "XM_1A_Byte : " + XM_1A_Byte;
            modInfo += "\n" + "trackerName : " + trackerName;
            modInfo += "\n" + "version : " + version;
            modInfo += "\n" + "headerSize : " + headerSize;
            modInfo += "\n" + "songLength : " + songLength;
            modInfo += "\n" + "songRepeatPosition : " + songRepeatPosition;
            modInfo += "\n" + "numberOfChannels : " + numberOfChannels;
            modInfo += "\n" + "numberOfPatterns : " + numberOfPatterns;
            modInfo += "\n" + "numberOfInstruments : " + numberOfInstruments;
            modInfo += "\n" + "XM_Flags : " + XM_Flags;
            modInfo += "\n" + "tempo : " + tempo;
            modInfo += "\n" + "BPM : " + BPM + "\n";
            for (int i = 0; i < songLength; i++) {
                modInfo += arrangement[i] + ((i < songLength - 1) ? "," : "");
            }
            return modInfo + "\n\n";
        }

        public override void Play() {
            mixer.PlayModule(0);
        }

        public override void PlayInstrument(uint instrumentNumber = 0) {
            PlayBackWaveProvider waveStream = new PlayBackWaveProvider(ModuleConst.SOUNDFREQUENCY, ModuleConst.MIX_CHANNELS == ModuleConst.STEREO);

            XM_Sample sample = ((XM_Instrument)Instruments[(int)instrumentNumber - 1]).sample;

            string mes = "\n\n\nSample number : " + sample.orderNumber;

            float period = 7680.0f - (48 + sample.relativeNoteNumber) * 64.0f;
            float frequency = (float)(8363 * Math.Pow(2, (4608.0f - period) * 0.001302083f));
            float periodInc = (period != 0) ? frequency * 2 / ModuleConst.SOUNDFREQUENCY : 1.0f;
            float pos = 0;

            mes += " freq : " + period;
            DebugMes(mes);

            while (pos < sample.length * 4 - 1) {
                waveStream.Write((short)sample[(int)pos]);
                pos += periodInc;
            }

            WaveOutEvent waveOut = new WaveOutEvent();
            waveOut.Init(waveStream);
            waveOut.Play();
        }


        public override void Stop() {
            mixer.Stop();
        }

        public override void Dispose() {
            mixer.Stop();
        }

    }
 
    public class XM_MixerChannel : ModuleMixerChannel {
        public byte volumeEffect                = 0;
    }

    public class XM_Mixer : MOD_Mixer {

        public XM_Mixer(Module module) : base(module) {
            noteEffects.Add(NoEffect); //G(010h) Set global volume
            noteEffects.Add(NoEffect); //H(011h) Global volume slide
            noteEffects.Add(NoEffect); //I(012h) Unused
            noteEffects.Add(NoEffect); //J(013h) Unused
            noteEffects.Add(NoEffect); //K(014h) Unused
            noteEffects.Add(NoEffect); //L(015h) Set envelope position
            noteEffects.Add(NoEffect); //M(016h) Unused
            noteEffects.Add(NoEffect); //N(017h) Unused
            noteEffects.Add(NoEffect); //O(018h) Unused
            noteEffects.Add(NoEffect); //P(019h) Panning slide
            noteEffects.Add(NoEffect); //Q(01ah) Unused
            noteEffects.Add(NoEffect); //R(01bh) Multi retrig note
            noteEffects.Add(NoEffect); //S(01ch) Unused
            noteEffects.Add(NoEffect); //T(01dh) Tremor
            noteEffects.Add(NoEffect); //U(01eh) Unused
            noteEffects.Add(NoEffect); //V(01fh) Unused
            noteEffects.Add(NoEffect); //W(020h) Unused
            noteEffects.Add(NoEffect); //X1(021h) Extra fine porta up
            noteEffects.Add(NoEffect); //X2(021h) Extra fine porta down

            tickEffects.Add(NoEffect);  //G(010h) Set global volume
            tickEffects.Add(NoEffect);  //H(011h) Global volume slide
            tickEffects.Add(NoEffect);  //I(012h) Unused
            tickEffects.Add(NoEffect);  //J(013h) Unused
            tickEffects.Add(NoEffect);  //K(014h) Unused
            tickEffects.Add(NoEffect);  //L(015h) Set envelope position
            tickEffects.Add(NoEffect);  //M(016h) Unused
            tickEffects.Add(NoEffect);  //N(017h) Unused
            tickEffects.Add(NoEffect);  //O(018h) Unused
            tickEffects.Add(NoEffect);  //P(019h) Panning slide
            tickEffects.Add(NoEffect);  //Q(01ah) Unused
            tickEffects.Add(NoEffect);  //R(01bh) Multi retrig note
            tickEffects.Add(NoEffect);  //S(01ch) Unused
            tickEffects.Add(NoEffect);  //T(01dh) Tremor
            tickEffects.Add(NoEffect);  //U(01eh) Unused
            tickEffects.Add(NoEffect);  //V(01fh) Unused
            tickEffects.Add(NoEffect);  //W(020h) Unused
            tickEffects.Add(NoEffect);  //X1(021h) Extra fine porta up
            tickEffects.Add(NoEffect);  //X2(021h) Extra fine porta down

        }

        public override uint GetNotePeriod(uint note, int fineTune) {
            float period = 7680.0f - note * 64.0f - fineTune * 0.5f;
            return (uint)period;
        }

        protected override ModuleMixerChannel CreateMixerChannel() {
            return new XM_MixerChannel();
        }

        protected override void ResetChannelInstrument(ModuleMixerChannel mc) {
            XM_Instrument instrument = (XM_Instrument)module.Instruments[(int)mc.instrumentIndex - 1];
            mc.instrumentPosition = 2;
            mc.instrumentLength = instrument.sample.length;
            mc.loopType = instrument.sample.loopType;
            mc.instrumentLoopStart = instrument.sample.loopStart;
            mc.instrumentLoopEnd = instrument.sample.loopEnd;
            mc.currentFineTune = instrument.sample.finetune;

            mc.vibratoCount = 0;
            mc.tremoloCount = 0;
            mc.arpeggioCount = 0;
        }

        protected override void UpdateNote() {
            patternDelay = 0;
            for (int ch = 0; ch < module.numberOfChannels; ch++) {
                XM_MixerChannel mc = (XM_MixerChannel)mixChannels[ch];
                XM_PatternChannel pe = (XM_PatternChannel)pattern.patternRows[(int)currentRow].patternChannels[ch];

                mc.effect = pe.effekt;
                mc.effectArg = pe.effektOp;
                mc.effectArgX = (mc.effectArg & 0xF0) >> 4;
                mc.effectArgY = (mc.effectArg & 0x0F);

                if (pe.noteIndex == 97) {
                    mc.channelVolume = 0.0f;
                }

                if (pe.instrumentIndex > 0 && pe.noteIndex > 0 && pe.noteIndex < 97) {
                    mc.instrumentLastIndex = mc.instrumentIndex;
                    mc.instrumentIndex = pe.instrumentIndex;
                    ResetChannelInstrument(mc);
                    mc.channelVolume = module.Instruments[(int)mc.instrumentIndex - 1].volume;
                    mc.portamentoStart = (int)mc.period;
                    mc.noteIndex = (uint)(pe.noteIndex + ((XM_Instrument)module.Instruments[(int)mc.instrumentIndex - 1]).sample.relativeNoteNumber);
                    mc.period = GetNoteFreq(mc.noteIndex, mc.currentFineTune);
                    mc.portamentoEnd = (int)mc.period;
                    mc.periodInc = CalcPeriodIncrement(mc.period);
                }

                if (pe.instrumentIndex > 0 && pe.noteIndex == 0 && pe.instrumentIndex != mc.instrumentLastIndex) {
                    mc.instrumentLastIndex = pe.instrumentIndex;
                    mc.instrumentIndex = pe.instrumentIndex;
                    ResetChannelInstrument(mc);
                    mc.channelVolume = module.Instruments[(int)mc.instrumentIndex - 1].volume;
                }

                if (pe.instrumentIndex > 0 && pe.noteIndex == 0 && pe.instrumentIndex == mc.instrumentLastIndex) {
                    mc.channelVolume = module.Instruments[(int)mc.instrumentIndex - 1].volume;
                }

                if (pe.instrumentIndex == 0 && pe.noteIndex > 0 && pe.noteIndex < 97) {
                    mc.portamentoStart = (int)mc.period;
                    mc.noteIndex = (uint)(pe.noteIndex + ((XM_Instrument)module.Instruments[(int)mc.instrumentIndex - 1]).sample.relativeNoteNumber);
                    mc.period = GetNoteFreq(mc.noteIndex, mc.currentFineTune);
                    mc.portamentoEnd = (int)mc.period;
                    mc.periodInc = CalcPeriodIncrement(mc.period);
                    ResetChannelInstrument(mc);
                }
            }

            NextRow();

        }

        protected void UpdateVolumeEffects() {
            for (int ch = 0; ch < module.numberOfChannels; ch++) {
                byte volumeEffect = ((XM_MixerChannel)mixChannels[ch]).volumeEffect;
                //if (volumeEffect >= 0x00 && volumeEffect <= 0x0F);  // do nothing
                if (volumeEffect >= 0x10 && volumeEffect <= 0x4F) {
                    mixChannels[ch].channelVolume = (float)(volumeEffect - 0x10) / 64.0f;  // v 0..63
                }
                if (volumeEffect == 0x50) {
                    mixChannels[ch].channelVolume = 1.0f;  // v 64
                }
                //if (volumeEffect >= 0x51 && volumeEffect <= 0x5F) return "...";  // undefined
                //if (volumeEffect >= 0x60 && volumeEffect <= 0x6F) return "D" + ModuleUtils.GetAsDec(volumeEffect - 0x60, 2);  // volumeEffect slide down
                //if (volumeEffect >= 0x70 && volumeEffect <= 0x7F) return "C" + ModuleUtils.GetAsDec(volumeEffect - 0x70, 2);  // volumeEffect slide up
                //if (volumeEffect >= 0x80 && volumeEffect <= 0x8F) return "B" + ModuleUtils.GetAsDec(volumeEffect - 0x80, 2);  // Fine volumeEffect down
                //if (volumeEffect >= 0x90 && volumeEffect <= 0x9F) return "A" + ModuleUtils.GetAsDec(volumeEffect - 0x90, 2);  // Fine volumeEffect up
                //if (volumeEffect >= 0xA0 && volumeEffect <= 0xAF) return "U" + ModuleUtils.GetAsDec(volumeEffect - 0xA0, 2);  // Vibrato speed
                //if (volumeEffect >= 0xB0 && volumeEffect <= 0xBF) return "H" + ModuleUtils.GetAsDec(volumeEffect - 0xB0, 2);  // Vibrato deph
                //if (volumeEffect >= 0xC0 && volumeEffect <= 0xCF) return "P" + ModuleUtils.GetAsDec((volumeEffect - 0xC0) * 4 + 2, 2);  // Set panning (2,6,10,14..62)
                //if (volumeEffect >= 0xD0 && volumeEffect <= 0xDF) return "L" + ModuleUtils.GetAsDec(volumeEffect - 0xD0, 2);  // Pan slide left
                //if (volumeEffect >= 0xE0 && volumeEffect <= 0xEF) return "R" + ModuleUtils.GetAsDec(volumeEffect - 0xE0, 2);  // Pan slide right
                //if (volumeEffect >= 0xF0 && volumeEffect <= 0xFF) return "G" + ModuleUtils.GetAsDec(volumeEffect - 0xF0, 2);  // Tone portamento
            }
        }

        protected override void UpdateBPM() {
            if (counter >= ((1 + patternDelay) * speed)) {
                counter = 0;
                UpdateNote();
                UpdateNoteEffects();
                UpdateVolumeEffects();
                UpdateTickEffects();
            }
            else {
                UpdateTickEffects();
            }
            counter++;
        }

    }

}

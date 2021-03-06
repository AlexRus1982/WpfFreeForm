﻿using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModuleSystem {
   
    public class MOD_Instrument : ModuleInstrument {

        public uint repeatStart             = 0;                   // # of the loop start (already *2 --> Mod-Fomat)
        public uint repeatStop              = 0;                    // # of the loop end   (already *2 --> Mod-Fomat)
        public uint repeatLength            = 0;                    // length of the loop
        public uint loopType                = 0;                    // 0: no Looping, 1: normal, 2: pingpong, 3: backwards
        public List<float> sampleData       = new List<float>();  // The sampledata, already converted to 16 bit (always)
                                                                // 8Bit: -128 to 127; 16Bit: -32768..0..+32767
        public MOD_Instrument() {
        }

        public override float this[float index] {
            get {
                if (index < 0) {
                    return 0.0f;
                }

                if ((index >= length) && (loopType == 0x00)) {
                    return 0.0f;
                }

                if ((index >= repeatStop) && (loopType == 0x01)) {
                    int newPos = (int)(index - repeatStop) % (int)repeatLength + (int)repeatStart;
                    return sampleData[newPos];
                }

                return sampleData[(int)index];
            }
        }

        public override float CalculateInstrumentPosition(float pos) {
            if (pos < 0) {
                return 0.0f;
            }

            if ((pos >= length) && (loopType == 0x00)) {
                return length;
            }

            if ((pos >= repeatStop) && (loopType == 0x01)) {
                return (int)(pos - repeatStop) % (int)repeatLength + (int)repeatStart;
            }

            return pos;
        }


        public override string ToString() {
            string res = name;
            res += "(sizeInBytes:" + sizeInBytes + ","
                + "orderNumber:" + orderNumber + ","
                + "fTune:" + fineTune + ","
                + "vol:" + volume + ","
                + "repStart:" + repeatStart + ","
                + "repLen:" + repeatLength + ","
                + "repStop:" + repeatStop + ")";
            return res;
        }

        public override void ReadHeaderFromStream(Stream stream, ref uint orderNumber) {
            this.orderNumber = orderNumber;
            name = ModuleUtils.ReadString0(stream, 22);
            length = (uint)ModuleUtils.ReadWordSwap(stream) * 2; // Length
            sizeInBytes = length;

            fineTune = stream.ReadByte() & 0xF; // finetune Value>7 means negative 8..15= -8..-1
            fineTune = (fineTune > 7) ? fineTune - 16 : fineTune;

            int vol = stream.ReadByte(); // volume 64 is maximum
            volume = (vol > 64) ? 1.0f : (float)vol / 64.0f;

            //// Repeat start and stop
            repeatStart  = (uint)ModuleUtils.ReadWordSwap(stream) * 2;
            repeatLength = (uint)ModuleUtils.ReadWordSwap(stream) * 2;
            repeatStop = repeatStart + repeatLength;

            if (length < 4) {
                length = 0;
            }

            if (length > 0) {
                if (repeatStart > length) {
                    repeatStart = length;
                }

                if (repeatStop > length) {
                    repeatStop = length;
                }

                if (repeatStart >= repeatStop || repeatStop <= 8 || (repeatStop - repeatStart) <= 4) {
                    repeatStart = repeatStop = 0;
                    loopType = 0;
                }

                if (repeatStart < repeatStop) {
                    loopType = ModuleConst.LOOP_ON;
                }
            }
            else {
                loopType = 0;
            }

            repeatLength = (uint)(repeatStop - repeatStart);
        }

        public override void ReadSampleDataFromStream(Stream stream) {
            sampleData.Clear();
            if (length > 0) {
                for (int s = 0; s < length; s++) {
                    float sampleValue = ModuleUtils.ReadSignedByte(stream) * 0.0078125f; // 0.0078125f = 1/128
                    sampleData.Add(sampleValue * ModuleConst.SOUND_AMP);
                }
            }
        }

        public override void Clear() {
            sampleData.Clear();
        }

    }

    public class MOD_PatternChannel : ModulePatternChannel {
        public uint period = 0;
        public byte noteIndex = 0;
        public ushort instrumentIndex = 0;
        public uint effekt = 0;
        public uint effektOp = 0;
        public uint volumeEffekt = 0;
        public uint volumeEffektOp = 0;

        public override string ToString() {
            string res = ModuleConst.GetNoteNameToIndex((int)noteIndex);
            res += ((period == 0 && noteIndex != 0) || (period != 0 && noteIndex == 0)) ? "!" : " ";
            res += (instrumentIndex != 0) ? ModuleUtils.GetAsHex(instrumentIndex, 2) : "..";
            res += " ";
            res += ((effekt != 0) || (effektOp != 0)) ? ModuleUtils.GetAsHex(effekt, 1) + ModuleUtils.GetAsHex(effektOp, 2) : "...";
            return res;
        }

    }

    public class MOD_Pattern : ModulePattern {

        private MOD_PatternChannel CreateNewPatternChannel(Stream stream, uint numberOfSamples) {
            MOD_PatternChannel channel = new MOD_PatternChannel();
            uint b0 = (uint)stream.ReadByte();
            uint b1 = (uint)stream.ReadByte();
            uint b2 = (uint)stream.ReadByte();
            uint b3 = (uint)stream.ReadByte();

            channel.instrumentIndex = (byte)(((b0 & 0xF0) | ((b2 & 0xF0) >> 4)) & numberOfSamples);
            channel.period = (uint)(((b0 & 0x0F) << 8) | b1);

            if (channel.period > 0) {
                channel.noteIndex = ModuleConst.GetNoteIndexForPeriod((int)channel.period);
            }

            channel.effekt = b2 & 0x0F;
            channel.effektOp = b3;

            return channel;
        }

        public void ReadPatternData(Stream stream, string moduleID, uint numberOfChannels, uint numberOfSamples) {
            if (moduleID == "FLT8") // StarTrekker is slightly different
            {
                for (int row = 0; row < 64; row++) {
                    ModulePatternRow pRow = new ModulePatternRow();
                    for (int channel = 0; channel < 4; channel++) {
                        pRow.patternChannels.Add(CreateNewPatternChannel(stream, numberOfSamples));
                    }
                    for (int channel = 4; channel < 8; channel++)
                        pRow.patternChannels.Add(new MOD_PatternChannel());

                    patternRows.Add(pRow);
                }
                for (int row = 0; row < 64; row++)
                    for (int channel = 4; channel < 8; channel++)
                        patternRows[row].patternChannels[channel] = CreateNewPatternChannel(stream, numberOfSamples);
            }
            else {
                for (int row = 0; row < 64; row++) {
                    ModulePatternRow pRow = new ModulePatternRow();
                    for (int channel = 0; channel < numberOfChannels; channel++)
                        pRow.patternChannels.Add(CreateNewPatternChannel(stream, numberOfSamples));

                    patternRows.Add(pRow);
                }
            }
        }

        public override string ToString() {
            string res = "\n";
            if (patternRows[0] != null) {
                string ln = "====";
                for (int i = 0; i < patternRows[0].patternChannels.Count; i++)
                    ln += "===========";

                res += ln + "\n";

                for (int i = 0; i < patternRows.Count; i++)
                    res += ModuleUtils.GetAsHex((uint)i, 2) + ":|" + patternRows[i] + "\n";

                res += ln + "\n";
            }
            else
                res += "empty pattern\n";
            return res;
        }

    }

    public class MOD_Module : Module {

        private uint bytesLeft = 0;

        public MOD_Module() : base("MOD format") {
            DebugMes("MOD Sound Module created");
        }

        private long GetAllInstrumentsLength() {
            long allSamplesLength = 0;
            foreach (ModuleInstrument inst in Instruments.List) {
                allSamplesLength += inst.sizeInBytes;
            }
            return allSamplesLength;
        }

        public string InstrumentsToString() {
            string res = "Samples info : \n";
            int i = 0;
            foreach (MOD_Instrument inst in Instruments.List) {
                res += ModuleUtils.GetAsHex((uint)i++, 2) + ':' + inst.ToString() + "\n";
            }
            return res;
        }

        private void ReadInstruments(Stream stream) {
            Instruments.Clear();
            for (uint i = 0; i < numberOfSamples; i++) {
                MOD_Instrument inst = new MOD_Instrument();
                Instruments.Add(inst);
                inst.ReadHeaderFromStream(stream, ref i);
            }
            DebugMes(InstrumentsToString());
        }

        private void ReadArrangement(Stream stream) {
            songLength = (uint)stream.ReadByte(); // count of pattern in arrangement
            stream.ReadByte();              // skip, ood old CIAA

            arrangement.Clear();
            // always space for 128 pattern...
            for (int i = 0; i < 128; i++) {
                arrangement.Add((uint)stream.ReadByte());
            }

        }

        private uint CalcPattCount() {
            int headerLen = 150; // Name+SongLen+CIAA+SongArrangement
            if (numberOfSamples > 15) {
                headerLen += 4;  // Kennung
            }

            uint sampleLen = 0;
            foreach (MOD_Instrument inst in Instruments.List) {
                sampleLen += 30 + inst.sizeInBytes;
            }

            int spaceForPattern = (int)(fileLength - headerLen - sampleLen);

            // Lets find out about the highest Patternnumber used
            // in the song arrangement
            uint maxPatternNumber = 0;
            for (int i = 0; i < songLength; i++) {
                uint patternNumber = arrangement[i];
                if (patternNumber > maxPatternNumber && patternNumber < 0x80) {
                    maxPatternNumber = (uint)arrangement[i];
                }
            }
            maxPatternNumber++; // Highest number becomes highest count 

            // It could be the WOW-Format:
            if (moduleID == "M.K.") {
                // so check for 8 channels:
                uint totalPatternBytes = maxPatternNumber * 2048; //64*4*8
                                                                  // This mod has 8 channels! --> WOW
                if (totalPatternBytes == spaceForPattern) {
                    isAmigaLike = true;
                    numberOfChannels = 8;
                    trackerName = "Grave Composer";
                }
            }

            uint bytesPerPattern = (256 * numberOfChannels); //64*4*numberOfChannels
            numberOfPatterns = (uint)(spaceForPattern / bytesPerPattern);
            uint bytesLeft = (uint)(spaceForPattern % bytesPerPattern);
            if (bytesLeft > 0) // It does not fit!
            {
                if (maxPatternNumber > numberOfPatterns) {
                    // The modfile is too short. The highest pattern is reaching into
                    // the sampledata, but it has to be read!
                    bytesLeft -= bytesPerPattern;
                    numberOfPatterns = (uint)(maxPatternNumber + 1);
                }
                else {
                    // The modfile is too long. Sometimes this happens if composer
                    // add additional data to the modfile.
                    bytesLeft += (uint)((numberOfPatterns - maxPatternNumber) * bytesPerPattern);
                    numberOfPatterns = maxPatternNumber;
                }
                return bytesLeft;
            }
            return 0;
        }

        private void ReadPatterns(Stream stream) {
            foreach (ModulePattern pat in patterns) {
                pat.Clear();
            }

            patterns.Clear();
            if (numberOfSamples > 15) {
                stream.Seek(4, SeekOrigin.Current); // skip moduleID, if not NoiseTracker:
            }

            bytesLeft = CalcPattCount();
            for (int i = 0; i < numberOfPatterns; i++)                      // Read the patterndata
            {
                MOD_Pattern pattern = new MOD_Pattern();
                pattern.ReadPatternData(stream, moduleID, numberOfChannels, numberOfSamples);
                patterns.Add(pattern);
                //DebugMes("Patern : " + i + pattern.ToString());
            }
        }

        private void ReadInstrumentsData(Stream stream) {
            // Sampledata: If the modfile was too short, we need to recalculate:
            if (bytesLeft < 0) {
                long calcSamplePos = GetAllInstrumentsLength();
                calcSamplePos = fileLength - calcSamplePos;
                if (calcSamplePos < stream.Position) {
                    stream.Seek(calcSamplePos, SeekOrigin.Begin); // do this only, if needed!
                }
            }
            for (int i = 0; i < numberOfSamples; i++) {
                Instruments[i].ReadSampleDataFromStream(stream);
            }
        }

        public override bool ReadFromStream(Stream stream) {
            fileLength = stream.Length;
            baseVolume = 1.0f;
            BPM = 125;
            tempo = 6;

            if (!CheckFormat(stream)) {
                return false;
            }

            SetModuleType();

            stream.Seek(0, SeekOrigin.Begin);
            songName = ModuleUtils.ReadString0(stream, 20);
            numberOfInstruments = numberOfSamples;
            ReadInstruments(stream);
            ReadArrangement(stream);
            ReadPatterns(stream);
            ReadInstrumentsData(stream);

            mixer = new MOD_Mixer(this);
            return true;
        }

        private void SetModuleType() {
            isAmigaLike = false;
            numberOfSamples = 31;
            trackerName = "StarTrekker";

            if (moduleID == "M.K." || moduleID == "M!K!" || moduleID == "M&K!" || moduleID == "N.T.") {
                isAmigaLike = true;
                numberOfChannels = 4;
                trackerName = "ProTracker";
            }

            if (moduleID.Substring(0, 3) == "FLT" || moduleID.Substring(0, 3) == "TDZ") {
                numberOfChannels = uint.Parse(moduleID.Substring(3, 1));
            }

            if (moduleID.Substring(1, 3) == "CHN") {
                numberOfChannels = uint.Parse(moduleID.Substring(0, 1));
            }

            if (moduleID == "CD81" || moduleID == "OKTA") {
                numberOfChannels = 8;
                trackerName = "Atari Oktalyzer";
            }

            if (moduleID.Substring(2, 2) == "CH" || moduleID.Substring(2, 2) == "CN") {
                numberOfChannels = uint.Parse(moduleID.Substring(0, 2));
                trackerName = "TakeTracker";
            }
        }

        public override bool CheckFormat(Stream stream) {
            byte[] bytesID = new byte[4];
            stream.Seek(1080, SeekOrigin.Begin);
            stream.Read(bytesID);
            moduleID = Encoding.ASCII.GetString(bytesID);
            return moduleID                 == "M.K." ||
                   moduleID                 == "M!K!" ||
                   moduleID                 == "M&K!" ||
                   moduleID                 == "N.T." ||
                   moduleID                 == "CD81" ||
                   moduleID                 == "OKTA" ||
                   moduleID.Substring(0, 3) == "FLT"  ||
                   moduleID.Substring(0, 3) == "TDZ"  ||
                   moduleID.Substring(1, 3) == "CHN"  ||
                   moduleID.Substring(2, 2) == "CH"   ||
                   moduleID.Substring(2, 2) == "CN";
        }

        public override string ToString() {
            string info = "";
            info += "Mod Length " + this.fileLength + "\n";
            info += "Mod with " + numberOfSamples + " samples and " + numberOfChannels + " channels using\n";
            info += "Tracker : " + trackerName + " moduleID : " + moduleID + "\n";
            info += (isAmigaLike) ? "Protracker" : "Fast Tracker log";
            info += " frequency table\n";
            info += "Song named : " + songName + "\n";
            info += "SongLength : " + songLength + "\n";
            info += "Pattern channels : " + numberOfChannels + "\n";

            for (int i = 0; i < songLength; i++) {
                info += arrangement[i] + ((i < songLength - 1) ? "," : "");
            }

            info += "\n\n";
            return info;
        }

        public override void PlayInstrument(uint instrumentNumber = 0) {
            if (instrumentNumber < 0 || instrumentNumber >= numberOfSamples) {
                return;
            }

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

        public override void Play() {
            mixer.PlayModule(0);
        }

        public override void Stop() {
            mixer.Stop();
        }

        public override void Dispose() {
            mixer.Stop();
        }

    }

    public class MOD_MixerChannel : ModuleMixerChannel {

        public MOD_PatternChannel patternChannel    = null;

    }
 
    public class MOD_Mixer : ModuleMixer {

        #region Effects

        private bool NoteEffect0(ModuleMixerChannel mc)                // Arpeggio
        {
            if (mc.effectArg == 0)
                return false;
            mc.arpeggioCount = 0;
            mc.arpeggioIndex = mc.noteIndex;
            mc.arpeggioX = mc.arpeggioIndex + mc.effectArgX;
            mc.arpeggioY = mc.arpeggioIndex + mc.effectArgY;
            mc.arpeggioPeriod0 = GetNoteFreq(mc.arpeggioIndex, /*mc.currentFineTune*/ 0);
            mc.period = mc.arpeggioPeriod0;
            mc.arpeggioPeriod1 = (mc.arpeggioX < 60) ? GetNoteFreq(mc.arpeggioX, /*mc.currentFineTune*/ 0) : mc.arpeggioPeriod0;
            mc.arpeggioPeriod2 = (mc.arpeggioY < 60) ? GetNoteFreq(mc.arpeggioY, /*mc.currentFineTune*/ 0) : mc.arpeggioPeriod0;
            return true;
        }

        private bool NoteEffect1(ModuleMixerChannel mc)                // Slide up (Portamento Up)
        {
            mc.portamentoStart = (int)mc.period;
            mc.portamentoStep = -(int)mc.effectArg;
            return true;
        }

        private bool NoteEffect2(ModuleMixerChannel mc)             // Slide down (Portamento Down)
        {
            mc.portamentoStart = (int)mc.period;
            mc.portamentoStep = (int)mc.effectArg;
            return true;
        }

        private bool NoteEffect3(ModuleMixerChannel mc)             // Slide to note
        {
            if (mc.effectArg != 0) {
                mc.lastPortamentoStep = mc.portamentoStep;
                mc.portamentoStep = (mc.portamentoStart <= mc.portamentoEnd) ? (int)mc.effectArg : -(int)mc.effectArg;
            }
            else
                mc.portamentoStep = mc.lastPortamentoStep;

            return true;
        }

        private bool NoteEffect4(ModuleMixerChannel mc)             // Vibrato
        {
            mc.vibratoStart = (int)mc.period;
            mc.vibratoPeriod = mc.vibratoStart;
            if (mc.effectArgX != 0 && mc.effectArgY != 0) {
                mc.lastVibratoFreq = (int)mc.effectArgX;
                mc.lastVibratoAmp = (int)mc.effectArgY;
                mc.vibratoFreq = (int)mc.effectArgX;
                mc.vibratoAmp = (int)mc.effectArgY;
                mc.vibratoCount = 0;
            }
            else {
                mc.vibratoFreq = mc.lastVibratoFreq;
                mc.vibratoAmp = mc.lastVibratoAmp;
                mc.vibratoCount = 0;
            }
            return true;
        }

        private bool NoteEffect5(ModuleMixerChannel mc)             // Continue Slide to note + Volume slide
        {
            NoteEffectA(mc);
            return true;
        }

        private bool NoteEffect6(ModuleMixerChannel mc)             // Continue Vibrato + Volume Slide
        {
            NoteEffectA(mc);
            mc.effectArgX = 0;
            mc.effectArgY = 0;
            NoteEffect4(mc);
            return true;
        }

        private bool NoteEffect7(ModuleMixerChannel mc)             // Tremolo
        {
            mc.tremoloCount = (mc.tremoloType <= 0x03) ? 0 : mc.tremoloCount;
            mc.tremoloStart = mc.channelVolume;
            mc.tremoloFreq = (mc.effectArgX != 0) ? (int)mc.effectArgX : 0;
            mc.tremoloAmp = (mc.effectArgY != 0) ? (int)mc.effectArgY : 0;
            return true;
        }

        private bool NoteEffect8(ModuleMixerChannel mc)             // Not Used
        {
            return true;
        }

        private bool NoteEffect9(ModuleMixerChannel mc)             // Set Sample Offset
        {
            mc.instrumentPosition = mc.effectArg << 8;
            return true;
        }

        private bool NoteEffectA(ModuleMixerChannel mc)             // Volume Slide
        {
            mc.volumeSlideStep = 0;
            if (mc.effectArgX != 0)
                mc.volumeSlideStep = (float)mc.effectArgX / 0x40;   // Volume Slide up
            else if (mc.effectArgY != 0)
                mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;  // Volume Slide down
            mc.channelVolume -= mc.volumeSlideStep;
            return true;
        }

        private bool NoteEffectB(ModuleMixerChannel mc)             // Position Jump
        {
            mc.patternNumToJump = mc.effectArgX * 16 + mc.effectArgY;
            if (mc.patternNumToJump > 0x7F)
                mc.patternNumToJump = 0;

            currentRow = 0;
            track = module.arrangement[(int)mc.patternNumToJump];
            pattern = module.patterns[(int)module.arrangement[(int)track]];
            return true;
        }

        private bool NoteEffectC(ModuleMixerChannel mc)             // Set Volume
        {
            mc.channelVolume = (float)mc.effectArg / 0x40;
            mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
            return true;
        }

        private bool NoteEffectD(ModuleMixerChannel mc)             // Pattern Break
        {
            mc.positionToJump = mc.effectArgX * 10 + mc.effectArgY;
            if (mc.positionToJump > 0x3F)
                mc.positionToJump = 0;

            track++;
            pattern = module.patterns[(int)module.arrangement[(int)track]];
            currentRow = mc.positionToJump;

            return true;
        }

        private bool NoteEffectE(ModuleMixerChannel mc)             // Extended Effects
        {
            effectsE[(int)mc.effectArgX](mc);
            effectsEUsed[(int)mc.effectArgX] = true;
            return true;
        }

        private bool NoteEffectF(ModuleMixerChannel mc)             // SetSpeed
        {
            if ((mc.effectArg >= 0x20) && (mc.effectArg <= 0xFF)) {
                BPM = mc.effectArg;
                SetBPM();
            }
            if ((mc.effectArg > 0) && (mc.effectArg <= 0x1F))
                speed = mc.effectArg;

            //System.Diagnostics.Debug.WriteLine("Set BPM/speed -> " + BPM + " : " + speed);

            return true;
        }
        //----------------------------------------------------------------------------------------------

        private bool TickEffect0(ModuleMixerChannel mc) {
            if (mc.effectArg == 0)
                return false;
            uint arpeggioPeriod = mc.arpeggioPeriod0;
            arpeggioPeriod = (mc.arpeggioCount == 1) ? mc.arpeggioPeriod1 : arpeggioPeriod;
            arpeggioPeriod = (mc.arpeggioCount == 2) ? mc.arpeggioPeriod2 : arpeggioPeriod;
            //mc.period = arpeggioPeriod;
            mc.periodInc = CalcPeriodIncrement(arpeggioPeriod);
            mc.arpeggioCount = (mc.arpeggioCount + 1) % 4;
            return true;
        }

        private bool TickEffect1(ModuleMixerChannel mc) {
            mc.period = (uint)mc.portamentoStart;
            mc.periodInc = CalcClampPeriodIncrement(mc);
            mc.portamentoStart += mc.portamentoStep;
            return true;
        }

        private bool TickEffect2(ModuleMixerChannel mc) {
            mc.period = (uint)mc.portamentoStart;
            mc.periodInc = CalcClampPeriodIncrement(mc);
            mc.portamentoStart += mc.portamentoStep;
            return true;
        }

        private bool TickEffect3(ModuleMixerChannel mc) {
            mc.period = (uint)mc.portamentoStart;
            mc.periodInc = CalcClampPeriodIncrement(mc);

            mc.portamentoStart += mc.portamentoStep;
            if (mc.portamentoStep < 0)
                mc.portamentoStart = (mc.portamentoStart > mc.portamentoEnd) ? mc.portamentoStart : mc.portamentoEnd;
            else
                mc.portamentoStart = (mc.portamentoStart < mc.portamentoEnd) ? mc.portamentoStart : mc.portamentoEnd;
            return true;
        }

        private bool TickEffect4(ModuleMixerChannel mc) {
            mc.vibratoAdd = (mc.vibratoType % 4 == 0) ? ModuleConst.ModSinusTable[mc.vibratoCount] : mc.vibratoAdd;
            mc.vibratoAdd = (mc.vibratoType % 4 == 1) ? ModuleConst.ModRampDownTable[mc.vibratoCount] : mc.vibratoAdd;
            mc.vibratoAdd = (mc.vibratoType % 4 == 2) ? ModuleConst.ModSquareTable[mc.vibratoCount] : mc.vibratoAdd;
            mc.vibratoAdd = (mc.vibratoType % 4 == 3) ? ModuleConst.ModRandomTable[mc.vibratoCount] : mc.vibratoAdd;

            mc.periodInc = CalcPeriodIncrement((uint)mc.vibratoPeriod);
            mc.vibratoPeriod = mc.vibratoStart + (int)(mc.vibratoAdd * ((float)mc.vibratoAmp / 128.0f));
            mc.vibratoCount = (mc.vibratoCount + mc.vibratoFreq) & 0x3F;
            return true;
        }

        private bool TickEffect5(ModuleMixerChannel mc) {
            TickEffectA(mc);
            TickEffect3(mc);
            return true;
        }

        private bool TickEffect6(ModuleMixerChannel mc) {
            TickEffectA(mc);
            TickEffect4(mc);
            return true;
        }

        private bool TickEffect7(ModuleMixerChannel mc) {
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

        private bool TickEffect8(ModuleMixerChannel mc) {
            return true;
        }

        private bool TickEffect9(ModuleMixerChannel mc) {
            return true;
        }

        private bool TickEffectA(ModuleMixerChannel mc)             // Volume Slide tick
        {
            mc.channelVolume += mc.volumeSlideStep;
            mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
            mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
            return true;
        }

        private bool TickEffectB(ModuleMixerChannel mc) {
            return true;
        }

        private bool TickEffectC(ModuleMixerChannel mc) {
            return true;
        }

        private bool TickEffectD(ModuleMixerChannel mc) {
            return true;
        }

        private bool TickEffectE(ModuleMixerChannel mc) {
            return true;
        }

        private bool TickEffectF(ModuleMixerChannel mc) {
            return true;
        }
        //----------------------------------------------------------------------------------------------

        private bool EffectE0(ModuleMixerChannel mc) {
            return true;
        }

        private bool EffectE1(ModuleMixerChannel mc) {
            mc.period -= mc.effectArgY;
            mc.periodInc = CalcClampPeriodIncrement(mc);
            return true;
        }

        private bool EffectE2(ModuleMixerChannel mc) {
            mc.period += mc.effectArgY;
            mc.periodInc = CalcClampPeriodIncrement(mc);
            return true;
        }

        private bool EffectE3(ModuleMixerChannel mc) {
            return true;
        }

        private bool EffectE4(ModuleMixerChannel mc) {
            mc.vibratoType = mc.effectArgY & 0x7;
            return true;
        }

        private bool EffectE5(ModuleMixerChannel mc) {
            mc.currentFineTune = (int)mc.effectArgY;
            return true;
        }

        private bool EffectE6(ModuleMixerChannel mc) {
            return true;
        }

        private bool EffectE7(ModuleMixerChannel mc) {
            mc.tremoloType = mc.effectArgY & 0x7;
            return true;
        }

        private bool EffectE8(ModuleMixerChannel mc) {
            return true;
        }

        private bool EffectE9(ModuleMixerChannel mc)                // Retrigger Sample
        {
            return true;
        }

        private bool EffectEA(ModuleMixerChannel mc)                // Fine Volume Slide Up
        {
            mc.effect = 0x0A;
            mc.volumeSlideStep = (float)mc.effectArgY / 0x40;
            return true;
        }

        private bool EffectEB(ModuleMixerChannel mc)                // Fine Volume Slide Down
        {
            mc.effect = 0x0A;
            mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;
            return true;
        }

        private bool EffectEC(ModuleMixerChannel mc)                // Cut Sample
        {
            return true;
        }

        private bool EffectED(ModuleMixerChannel mc) {
            return true;
        }

        private bool EffectEE(ModuleMixerChannel mc)                // Delay
        {
            patternDelay = mc.effectArgY;
            return true;
        }

        private bool EffectEF(ModuleMixerChannel mc) {
            return true;
        }
        //----------------------------------------------------------------------------------------------
        #endregion

        public MOD_Mixer(Module module) : base (module) {
            noteEffects.Add(NoteEffect0);               // 0. ARPEGGIO
            noteEffects.Add(NoteEffect1);               // 1. PORTAMENTO UP 
            noteEffects.Add(NoteEffect2);               // 2. PORTAMENTO DOWN
            noteEffects.Add(NoteEffect3);               // 3. TONE PORTAMENTO
            noteEffects.Add(NoteEffect4);               // 4. VIBRATO
            noteEffects.Add(NoteEffect5);               // 5. TONE PORTAMENTO + VOLUME SLIDE
            noteEffects.Add(NoteEffect6);               // 6. VIBRATO + VOLUME SLIDE
            noteEffects.Add(NoteEffect7);               // 7. TREMOLO
            noteEffects.Add(/*NoteEffect8*/NoEffect);   // 8. PAN
            noteEffects.Add(/*NoteEffect9*/NoEffect);   // 9. SAMPLE OFFSET
            noteEffects.Add(NoteEffectA);               // A. VOLUME SLIDE
            noteEffects.Add(/*NoteEffectB*/NoEffect);   // B. POSITION JUMP
            noteEffects.Add(NoteEffectC);               // C. SET VOLUME
            noteEffects.Add(NoteEffectD);               // D. PATTERN BREAK
            noteEffects.Add(NoteEffectE);               // E. EXTEND EFFECTS
            noteEffects.Add(NoteEffectF);               // F. SET SPEED

            tickEffects.Add(TickEffect0);               // tick effect 0. ARPEGGIO
            tickEffects.Add(TickEffect1);               // tick effect 1. PORTAMENTO UP 
            tickEffects.Add(TickEffect2);               // tick effect 2. PORTAMENTO DOWN
            tickEffects.Add(TickEffect3);               // tick effect 3. TONE PORTAMENTO
            tickEffects.Add(TickEffect4);               // tick effect 4. VIBRATO
            tickEffects.Add(TickEffect5);               // tick effect 5. TONE PORTAMENTO + VOLUME SLIDE
            tickEffects.Add(TickEffect6);               // tick effect 6. VIBRATO + VOLUME SLIDE
            tickEffects.Add(TickEffect7);               // tick effect 7. TREMOLO
            tickEffects.Add(/*TickEffect8*/NoEffect);   // tick effect 8. PAN
            tickEffects.Add(/*TickEffect9*/NoEffect);   // tick effect 9. SAMPLE OFFSET
            tickEffects.Add(TickEffectA);               // tick effect A. VOLUME SLIDE
            tickEffects.Add(/*TickEffectB*/NoEffect);   // tick effect B. POSITION JUMP
            tickEffects.Add(/*TickEffectC*/NoEffect);   // tick effect C. SET VOLUME
            tickEffects.Add(/*TickEffectD*/NoEffect);   // tick effect D. PATTERN BREAK
            tickEffects.Add(/*TickEffectE*/NoEffect);   // tick effect E. EXTEND EFFECTS
            tickEffects.Add(/*TickEffectF*/NoEffect);   // tick effect F. SET SPEED

            effectsE.Add(/*EffectE0*/NoEffect);         // E0. SET FILTER
            effectsE.Add(/*EffectE1*/NoEffect);         // E1. FINE PORTAMENTO UP
            effectsE.Add(/*EffectE2*/NoEffect);         // E2. FINE PORTAMENTO DOWN
            effectsE.Add(/*EffectE3*/NoEffect);         // E3. GLISSANDO CONTROL
            effectsE.Add(/*EffectE4*/NoEffect);         // E4. VIBRATO WAVEFORM 
            effectsE.Add(/*EffectE5*/NoEffect);         // E5. SET FINETUNE
            effectsE.Add(/*EffectE6*/NoEffect);         // E6. PATTERN LOOP
            effectsE.Add(/*EffectE7*/NoEffect);         // E7. TREMOLO WAVEFORM
            effectsE.Add(/*EffectE8*/NoEffect);         // E8. 16 POSITION PAN
            effectsE.Add(/*EffectE9*/NoEffect);         // E9. RETRIG NOTE
            effectsE.Add(EffectEA);                     // EA. FINE VOLUME SLIDE UP
            effectsE.Add(EffectEB);                     // EB. FINE VOLUME SLIDE DOWN
            effectsE.Add(/*EffectEC*/NoEffect);         // EC. CUT NOTE
            effectsE.Add(/*EffectED*/NoEffect);         // ED. NOTE DELAY
            effectsE.Add(/*EffectEE*/NoEffect);         // EE. PATTERN DELAY
            effectsE.Add(/*EffectEF*/NoEffect);         // EF. INVERT LOOP
        }

        protected override ModuleMixerChannel CreateMixerChannel() {
            return new MOD_MixerChannel();
        }

        protected override void ResetChannelInstrument(ModuleMixerChannel mc) {
            MOD_Instrument instrument = (MOD_Instrument)module.Instruments[(int)mc.instrumentIndex - 1];
            mc.instrumentPosition = 2;
            mc.instrumentLength = instrument.length;
            mc.loopType = instrument.loopType;
            mc.instrumentLoopStart = instrument.repeatStart;
            mc.instrumentLoopEnd = instrument.repeatStop;
            mc.currentFineTune = instrument.fineTune;

            mc.vibratoCount = 0;
            mc.tremoloCount = 0;
            mc.arpeggioCount = 0;
        }

        protected override void UpdateNote() {
            patternDelay = 0;
            for (int ch = 0; ch < module.numberOfChannels; ch++) {
                MOD_MixerChannel mc = (MOD_MixerChannel)mixChannels[ch];
                MOD_PatternChannel pe = (MOD_PatternChannel)pattern.patternRows[(int)currentRow].patternChannels[ch];

                mc.patternChannel = pe;
                mc.effect = pe.effekt;
                mc.effectArg = pe.effektOp;
                mc.effectArgX = (mc.effectArg & 0xF0) >> 4;
                mc.effectArgY = mc.effectArg & 0x0F;

                if (pe.instrumentIndex > 0 && pe.period > 0) {
                    mc.instrumentLastIndex = mc.instrumentIndex;
                    mc.instrumentIndex = pe.instrumentIndex;
                    ResetChannelInstrument(mc);
                    mc.channelVolume = module.Instruments[(int)mc.instrumentIndex - 1].volume;
                    mc.portamentoStart = (int)mc.period;
                    mc.noteIndex = ModuleConst.GetNoteIndexForPeriod((int)pe.period);
                    mc.period = GetNoteFreq(mc.noteIndex, mc.currentFineTune);
                    mc.portamentoEnd = (int)mc.period;
                    mc.periodInc = CalcClampPeriodIncrement(mc);
                }

                if (pe.instrumentIndex > 0 && pe.period == 0 && pe.instrumentIndex != mc.instrumentLastIndex) {
                    mc.instrumentLastIndex = pe.instrumentIndex;
                    mc.instrumentIndex = pe.instrumentIndex;
                    ResetChannelInstrument(mc);
                    mc.channelVolume = module.Instruments[(int)mc.instrumentIndex - 1].volume;
                }

                if (pe.instrumentIndex > 0 && pe.period == 0 && pe.instrumentIndex == mc.instrumentLastIndex) {
                    mc.channelVolume = module.Instruments[(int)mc.instrumentIndex - 1].volume;
                }

                if (pe.instrumentIndex == 0 && pe.period > 0) {
                    mc.portamentoStart = (int)mc.period;
                    mc.noteIndex = ModuleConst.GetNoteIndexForPeriod((int)pe.period);
                    mc.period = GetNoteFreq(mc.noteIndex, mc.currentFineTune);
                    mc.portamentoEnd = (int)mc.period;
                    mc.periodInc = CalcClampPeriodIncrement(mc);
                    ResetChannelInstrument(mc);
                }
            }

            NextRow();

        }

        protected override void UpdateBPM() {
            if (counter >= ((1 + patternDelay) * speed)) {
                counter = 0;
                UpdateNote();
                UpdateNoteEffects();
                UpdateTickEffects();
            }
            else {
                UpdateTickEffects();
            }
            counter++;
        }

    }

}

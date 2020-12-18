using System;
using System.Collections.Generic;
using System.Text;

using System.Media;
using System.IO;

namespace WpfFreeFormModulePlayer.ModulePlayer
{
    public partial class ModuleSoundSystem
    {
        public delegate void SoundSystemHandler();
        public event SoundSystemHandler onPlayed;

        private uint musicBufferLen_ms = 500;
        private uint samplesPerSecond = 44100;
        private uint musicBufferLen;

        private SoundPlayer player = new SoundPlayer();
        private BinaryWriter writer0 = new BinaryWriter(new MemoryStream());
        private BinaryWriter writer1 = new BinaryWriter(new MemoryStream());
        private uint musicBuffer = 0;
        public ModuleSoundSystem()
        {
            calcBufferLen();
            WriteWavHeader(writer0); GenerateWaveform(writer0, 0, true);
            WriteWavHeader(writer1); GenerateWaveform(writer1, 0, true);
            onPlayed += Player_onPlayed;
        }
        public void setSampleRate(uint samplesPerSecond)
        {
            this.samplesPerSecond = samplesPerSecond;
            calcBufferLen();
        }
        public void setBufferLenMs(uint ms)
        {
            this.musicBufferLen_ms = ms;
            calcBufferLen();
        }
        private void calcBufferLen()
        {
            musicBufferLen = (uint)((decimal)samplesPerSecond * musicBufferLen_ms / 1000);
        }
        private void DebugMes(string mes)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(mes);
#endif
        }
        private void WriteWavHeader(BinaryWriter writer)
        {
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
            uint subchunk2Size = musicBufferLen * blockAlign;
            uint chunkSize = waveSize + headerSize + subchunk1Size + headerSize + subchunk2Size;

            writer.Write(chunkId);
            writer.Write(chunkSize);
            writer.Write(format);
            writer.Write(subchunk1Id);
            writer.Write(subchunk1Size);
            writer.Write(audioFormat);
            writer.Write(numChannels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            writer.Write(subchunk2Id);
            writer.Write(subchunk2Size);
        }
        private void GenerateWaveform(BinaryWriter writer, uint frequency, bool zero = false)
        {
            const double TAU = 2 * Math.PI;
            double theta = frequency * TAU / (double)samplesPerSecond;
            // 'volume' is UInt16 with range 0 thru Uint16.MaxValue ( = 65 535)
            // we need 'amp' to have the range of 0 thru Int16.MaxValue ( = 32 767)
            uint volume = 16383;
            double amp = volume >> 2; // so we simply set amp = volume / 2

            writer.BaseStream.Seek(44, SeekOrigin.Begin);

            for (int step = 0; step < musicBufferLen; step++)
            {
                short s = (short)(volume * Math.Sin(theta * (double)step));
                writer.Write((zero) ? 0 : s);
            }

            writer.BaseStream.Seek(0, SeekOrigin.Begin);
        }
        public void Play(uint frequency)
        {
            BinaryWriter writer = (musicBuffer == 0) ? writer0 : writer1;

            player.Stream = writer.BaseStream;

            //DebugMes("Current buffer - " + musicBuffer);
            //DebugMes("Playing start ...");

            player.PlaySync();

            GenerateWaveform(writer, frequency);

            //DebugMes("Playing stop.");
            onPlayed?.Invoke();

            musicBuffer = musicBuffer ^ 1; //SwapBuffers
        }
        private void Player_onPlayed()
        {
            DebugMes("Stop play event!"); ;
        }

    }
}

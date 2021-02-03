using System.IO;
using System.Media;

namespace ModuleSystem
{
    //-------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------
    public partial class ModuleSoundSystem
    {
        private uint samplesPerSecond = 44100;
        private uint musicBufferLen = 0;

        private SoundPlayer player = new SoundPlayer();
        private BinaryWriter writer = new BinaryWriter(new MemoryStream());
        public ModuleSoundSystem()
        {
        }
        public BinaryWriter getBuffer
        {
            get
            {
                writer.BaseStream.SetLength(0);
                WriteWavHeader(writer);
                writer.BaseStream.Seek(44, SeekOrigin.Begin);
                return writer;
            }
            set
            {
            }
        }
        public void SetSampleRate(uint samplesPerSecond)
        {
            this.samplesPerSecond = samplesPerSecond;
        }
        public void SetBufferLen(uint len)
        {
            musicBufferLen = len;
            writer.BaseStream.SetLength(0);
            WriteWavHeader(writer);
            writer.BaseStream.Seek(44, SeekOrigin.Begin);
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
            ushort blockAlign = (ushort)(numChannels * (bitsPerSample / 8));
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
        public void Play()
        {
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            player.Stream = writer.BaseStream;
            player.Play();
        }
        public void Stop()
        {
            player.Stop();
        }
        private void DebugMes(string mes)
        {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine(mes);
            #endif
        }
    }
}

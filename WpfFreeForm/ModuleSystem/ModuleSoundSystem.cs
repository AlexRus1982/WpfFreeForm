using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace ModuleSystem
{
    //-------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------
    class ModuleSoundStream : Stream
    {
        private long position;
        private ConcurrentQueue<byte> sampleQueue;
        private AutoResetEvent dataAvailableSignaler = new AutoResetEvent(false);
        private int preloadSize = 2000;

        public ModuleSoundStream(int sampleRate)
        {
            position = 0;
            sampleQueue = new ConcurrentQueue<byte>();
            WriteWavHeader(sampleRate);
        }

        /// <summary>
        /// Write audio short sample -32768..32767 into stream
        /// </summary>
        public void Write(short sample)
        {
            sampleQueue.Enqueue((byte)(sample & 0xFF));
            sampleQueue.Enqueue((byte)(sample >> 8));

            //send signal to Read method
            if (sampleQueue.Count >= preloadSize)
                dataAvailableSignaler.Set();
        }

        /// <summary>
        /// Count of unread bytes in buffer
        /// </summary>
        public int Buffered
        {
            get { return sampleQueue.Count; }
        }

        /// <summary>
        /// Read
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (position >= Length)
                return 0;

            //wait while data will be available
            if (sampleQueue.Count < preloadSize)
                dataAvailableSignaler.WaitOne();

            var res = offset;
            while (count-- > 0 && sampleQueue.Count > 0) //copy data from incoming queue to output buffer
            {
                byte b;
                if (!sampleQueue.TryDequeue(out b)) return 0;
                buffer[res++] = b;
            }
            position += (res - offset);

            return res;
        }

        #region WAV header
        /// <summary>
        /// Prepair wave header with sampleRate frequency
        /// </summary>
        private void WriteWavHeader(int sampleRate)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

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
            //uint sampleRate = (uint)ModuleConst.SOUNDFREQUENCY;
            uint byteRate = (uint)sampleRate * blockAlign;
            uint waveSize = 4;
            uint subchunk2Size = (uint)(int.MaxValue - 44); //)(int.MaxValue * blockAlign);
            uint chunkSize = waveSize + headerSize + subchunk1Size + headerSize + subchunk2Size;

            writer.Write(chunkId);
            writer.Write(chunkSize);
            writer.Write(format);
            writer.Write(subchunk1Id);
            writer.Write(subchunk1Size);
            writer.Write(audioFormat);
            writer.Write(numChannels);
            writer.Write((uint)sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            writer.Write(subchunk2Id);
            writer.Write(subchunk2Size);

            byte[] headerBytes = stream.ToArray();
            for (int i = 0; i < headerBytes.Length; i++)
                sampleQueue.Enqueue(headerBytes[i]);

            writer.Dispose();
            stream.Dispose();
        }
        #endregion

        #region Stream impl
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return (uint)(int.MaxValue - 44); }
        }

        public override long Position
        {
            get { return position; }
            set { position = value; }
        }

        public long QueueLength
        {
            get { return sampleQueue.Count; }
            set { ; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

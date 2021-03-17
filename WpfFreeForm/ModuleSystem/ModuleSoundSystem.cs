﻿using System;
using System.IO;
using System.Media;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using NAudio.Wave;

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

    class SoundStream : Stream
    {
        private long length;
        private long position;
        private ConcurrentQueue<byte> sampleQueue;
        private AutoResetEvent dataAvailableSignaler = new AutoResetEvent(false);
        private int preloadSize = 2000;

        public SoundStream(int sampleRate = 8000)
        {
            length = int.MaxValue / 2 - 36;
            position = 0;

            //add wav header with too big length (near 70 hours for sample rate 8000)
            sampleQueue = new ConcurrentQueue<byte>(BuildWavHeader((int)length, sampleRate));
        }

        /// <summary>
        /// Write audio samples into stream
        /// </summary>
        public void Write(short sample)
        {
            byte[] bytes = BitConverter.GetBytes(sample);

            //write samples to sample queue
            sampleQueue.Enqueue(bytes[0]);
            sampleQueue.Enqueue(bytes[1]);

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
            if (position >= length)
                return 0;

            //wait while data will be available
            if (sampleQueue.Count < preloadSize)
                dataAvailableSignaler.WaitOne();

            var res = 0;

            //copy data from incoming queue to output buffer
            while (count > 0 && sampleQueue.Count > 0)
            {
                byte b;
                if (!sampleQueue.TryDequeue(out b)) return 0;
                buffer[offset + res] = b;
                count--;
                res++;
                position++;
            }

            return res;
        }

        #region WAV header

        public static byte[] BuildWavHeader(int samplesCount, int sampleRate = 8000)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                short frameSize = (short)(16 / 8);
                writer.Write(0x46464952);
                writer.Write(36 + samplesCount * frameSize);
                writer.Write(0x45564157);
                writer.Write(0x20746D66);
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * frameSize);
                writer.Write(frameSize);
                writer.Write((short)16);
                writer.Write(0x61746164);
                writer.Write(samplesCount * frameSize);
                return stream.ToArray();
            }
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
            get { return length; }
        }

        public override long Position
        {
            get { return position; }
            set {; }
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

    public class StreamPlayer : IDisposable
    {
        private SoundStream stream;
        private WaveOutEvent waveOut;
        private WaveFileReader reader;

        public void StreamPlayerTest()
        {
            Random rnd = new Random();

            this.PlayAsync();

            //бесконечно пишем звук
            while (true)
            {
                //случайная частота
                var freq = 0.01 * rnd.Next(10);
                //пишем синусоидальный звук в плеер
                for (int i = 0; i < 8000; i++)
                {
                    var v = (short)(Math.Sin(freq * i * Math.PI * 2) * short.MaxValue);
                    this.Write(v);
                }
                Console.WriteLine("freq {0:0.00}", freq);
                //отдыхаем секунду
                Thread.Sleep(990);
            };
        }

        public StreamPlayer(int sampleRate = 8000)
        {
            stream = new SoundStream(sampleRate);
            waveOut = new WaveOutEvent();
        }

        /// <summary>
        /// Write audio samples into stream
        /// </summary>
        public void Write(short sample)
        {
            stream.Write(sample);
        }

        /// <summary>
        /// Plays sound
        /// </summary>
        public void PlayAsync()
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                reader = new WaveFileReader(stream);
                waveOut.Init(reader);
                waveOut.Play();
            });
        }

        /// <summary>
        /// Stop playing
        /// </summary>
        public void Stop()
        {
            waveOut.Stop();
        }

        /// <summary>
        /// Volume
        /// </summary>
        public float Volume
        {
            get { return waveOut.Volume; }
            set { waveOut.Volume = value; }
        }

        /// <summary>
        /// Count of unread bytes in buffer
        /// </summary>
        public int Buffered
        {
            get { return stream.Buffered; }
        }

        public void Dispose()
        {
            waveOut.Dispose();
            reader.Dispose();
            stream.Dispose();
        }
    }
}

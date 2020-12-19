using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ModuleSystem
{
    public static class ModuleUtils
    {
        public static string VERSION = "V1.0";
        public static string PROGRAM = "Sound engine";
        public static string FULLVERSION = PROGRAM + " " + VERSION;
        public static string COPYRIGHT = "Copyright by Alex 2020";

        public static string CODING_GUI = "cp850";
        public static string CODING_COMMANLINE = "cp1252";
        public static string currentCoding = CODING_GUI;

        public const int SOUNDFREQUENCY = 44100;
        public const int SOUNDBITS = 16;
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
            int data;
            if ((b[0] & 0x80) == 0) data = b[0];
            else data = b[0] - 256;

            return data;
        }

    }

    public class SoundModule : IDisposable
    {
        protected readonly string SoundModuleName = "Base module";
        protected float position = 0;

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

        public virtual void rewindToPosition()
        {
        }

        public virtual void Play()
        {
        }
        public virtual void PlayInstrument(int num)
        {
        }

        public virtual void Stop()
        {
        }

        public virtual void Pause()
        {
        }

        public virtual void Dispose()
        {
        }

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

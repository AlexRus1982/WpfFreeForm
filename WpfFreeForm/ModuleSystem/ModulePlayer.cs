using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ModuleSystem
{
    public class SoundModule : IDisposable
    {
        private readonly string SoundModuleName = "Base module";
        private float position = 0;

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

        public SoundModule()
        {
        }

        public bool checkFormat()
        {
            return false;
        }

        public bool readFromStream(Stream stream)
        {
            return true;
        }

        private void rewindToPosition()
        {
        }

        public void Play()
        {
        }

        public void Stop()
        {
        }

        public void Pause()
        {
        }

        public void Dispose()
        {
        }

        private void DebugMes(string mes)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(SoundModuleName + " -> " + mes);
#endif
        }

    }

    public class ModulePlayer
    {
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

        }

        public bool OpenFromStream(Stream stream)
        {
            if (sModule != null) sModule.Dispose();
            DebugMes("Read from stream");
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

        public void PlayInstrument(int instrNum)
        {
        }

        public void Play()
        {
            if (sModule != null) sModule.Play();
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

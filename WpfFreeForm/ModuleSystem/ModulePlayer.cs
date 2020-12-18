using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ModuleSystem
{
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

using System.Collections.Generic;
using System.IO;
using System;

namespace ModuleSystem {
    
    public class ModulePlayer {

        private Module module = null;

        public float Position {
            get {
                return (module != null) ? module.Position : 0;
            }
            set {
                if (module != null) {
                    module.Position = value;
                }
            }
        }

        public ModulePlayer() {
        }

        private void TryModuleLoader(Module checkingModule, Stream stream) {
            if (checkingModule.CheckFormat(stream)) {
                module = checkingModule;
            }
        }

        public bool OpenFromStream(Stream stream) {
            module?.Dispose();
            TryModuleLoader(new MOD_Module(), stream);
            TryModuleLoader(new XM_Module(), stream);
            module?.ReadFromStream(stream);
            DebugMes(module?.ToString());
            return module != null;
        }

        public bool OpenFromFile(string fileName) {
            bool res = false;
            using (FileStream fstream = File.OpenRead(fileName)) {
                DebugMes("Read from file : " + fileName);
                res = OpenFromStream(fstream);
            }
            return res;
        }

        public void Play() {
            module?.Play();
        }

        public void PlayInstrument(uint num) {
            module?.PlayInstrument(num);
        }

        public void Stop() {
            module?.Stop();
        }

        public void Pause() {
            module?.Pause();
        }

        private void DebugMes(string mes) {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("ModulePlayer -> " + mes);
            #endif
        }

    }

}

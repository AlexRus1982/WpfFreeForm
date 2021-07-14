using System.Collections.Generic;
using System.IO;
using System;

namespace ModuleSystem {
    public class ModulePlayer {

        private List<Module> libModules = new List<Module>();
        private Module module           = null;

        public float Position {
            get {
                if (module != null) {
                    return module.Position;
                }
                else {
                    return 0;
                }
            }
            set {
                if (module != null) {
                    module.Position = value;
                }
            }
        }

        public ModulePlayer() {
            libModules.Add(new MOD_Module());
            libModules.Add(new XM_Module());
        }

        public bool OpenFromStream(Stream stream) {
            module?.Dispose();
            for (int i = 0; i < libModules.Count; i++) {
                if (libModules[i].CheckFormat(stream)) {
                    module = libModules[i];
                }
            }
            module?.ReadFromStream(stream);
            DebugMes(module?.ToString());
            return true;
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

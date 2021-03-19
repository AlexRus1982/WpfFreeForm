using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Media.Effects;

using ModuleSystem;

namespace WpfFreeFormModulePlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModulePlayer mPlayer = new ModulePlayer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Buttons_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Image el = (Image)sender;
            DropShadowEffect dse = (DropShadowEffect)el.Effect;
            dse.Opacity = 0.75f;
            
        }
        private void Buttons_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Image el = (Image)sender;
            DropShadowEffect dse = (DropShadowEffect)el.Effect;
            dse.Opacity = 0.0f;
        }

        private void OpenSound_Click(object sender, RoutedEventArgs e)
        {
            var filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Browse MOD Files";
            //openFileDialog.InitialDirectory = "D:\\Progs\\music";
            openFileDialog.InitialDirectory = "d:\\!\\AS3_MOD_work2\\music\\mod\\";
            openFileDialog.Filter = "MOD Sound Files (*.mod)|*.mod|XM Sound Files (*.XM)|*.xm";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                //mPlayer = new ModulePlayer();
                mPlayer.OpenFromFile(filePath);
            }
        }

        private void StartSound_Click(object sender, RoutedEventArgs e)
        {
            //mPlayer.PlayInstrument(5);
            mPlayer.Play();
            //var strmPlayer = new StreamPlayer();
            //strmPlayer.Test();
        }

        private void StopSound_Click(object sender, RoutedEventArgs e)
        {
            mPlayer.Stop();
        }

        private void DebugMes(string mes)
        {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("MainForm -> " + mes);
            #endif
        }
    }
}

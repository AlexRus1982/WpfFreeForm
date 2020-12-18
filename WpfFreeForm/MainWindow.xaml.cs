using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WpfFreeFormModulePlayer.ModulePlayer;

namespace WpfFreeFormModulePlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModuleSoundSystem player = new ModuleSoundSystem();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            player.Play(1500);
        }

        private void DebugMes(string mes)
        {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine(mes);
            #endif
        }
    }
}

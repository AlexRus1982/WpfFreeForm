﻿using System;
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
using Microsoft.Win32;

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
                mPlayer.OpenFromFile(filePath);
            }
        }

        private void StartSound_Click(object sender, RoutedEventArgs e)
        {
            //mPlayer.PlayInstrument(7);
            mPlayer.Play();
        }

        private void StopSound_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DebugMes(string mes)
        {
            #if DEBUG
            System.Diagnostics.Debug.WriteLine("MainForm -> " + mes);
            #endif
        }
    }
}

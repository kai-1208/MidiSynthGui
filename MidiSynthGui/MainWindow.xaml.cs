// MainWindow.xaml.cs
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MidiSynthGui
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        private bool _isInitialized = false;

        public MainWindow()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            SetDllDirectory(appDir);

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "midi_synth_core.dll");
            if (!File.Exists(dllPath))
            {
                MessageBox.Show($"'midi_synth_core.dll' が見つかりません。\n配置先:\n{dllPath}",
                                "ファイル不足", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (!NativeMethods.Synth_Init())
                {
                    MessageBox.Show("C++ エンジンの初期化に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DLLの読み込みに失敗しました:\n{ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 1. デバイス一覧を取得
            PopulateMidiPorts();
            PopulateAudioDevices();

            // 2. SoundFont の自動読み込み
            string defaultSf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "piano.sf2");
            if (File.Exists(defaultSf))
            {
                LoadSoundFont(defaultSf);
            }
            else
            {
                TxtStatus.Text = "SoundFont (.sf2) を選択してください。";
            }

            // 3. 初期化フラグをONにし、選択中のデバイスを明示的に開始
            _isInitialized = true;

            if (CmbMidiPorts.SelectedIndex >= 0)
            {
                NativeMethods.Synth_OpenMidiPort(CmbMidiPorts.SelectedIndex);
            }

            if (CmbAudioDevices.SelectedIndex >= 0)
            {
                NativeMethods.Synth_StartAudio(CmbAudioDevices.SelectedIndex);
            }

            NativeMethods.Synth_SetMasterVolume(1.0f);
            NativeMethods.Synth_SetPedalEnabled(true);
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (_isInitialized)
            {
                NativeMethods.Synth_Shutdown();
            }
        }

        private void PopulateMidiPorts()
        {
            CmbMidiPorts.Items.Clear();
            int count = NativeMethods.Synth_GetMidiPortCount();
            StringBuilder sb = new StringBuilder(256);

            for (int i = 0; i < count; i++)
            {
                if (NativeMethods.Synth_GetMidiPortName(i, sb, sb.Capacity))
                {
                    CmbMidiPorts.Items.Add($"[{i}] {sb}");
                }
            }

            if (CmbMidiPorts.Items.Count > 0)
                CmbMidiPorts.SelectedIndex = 0;
        }

        private void PopulateAudioDevices()
        {
            CmbAudioDevices.Items.Clear();
            int count = NativeMethods.Synth_GetAudioDeviceCount();
            StringBuilder sb = new StringBuilder(256);

            for (int i = 0; i < count; i++)
            {
                if (NativeMethods.Synth_GetAudioDeviceName(i, sb, sb.Capacity))
                {
                    CmbAudioDevices.Items.Add($"[{i}] {sb}");
                }
            }

            if (CmbAudioDevices.Items.Count > 0)
                CmbAudioDevices.SelectedIndex = 0;
        }

        private void LoadSoundFont(string path)
        {
            if (NativeMethods.Synth_LoadSoundFont(path))
            {
                TxtSoundFontPath.Text = path;
                TxtStatus.Text = $"読み込み完了: {Path.GetFileName(path)}";
            }
            else
            {
                MessageBox.Show("SoundFont の読み込みに失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnBrowseSf2_Click(object? sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "SoundFont Files (*.sf2)|*.sf2|All Files (*.*)|*.*",
                Title = "SoundFont ファイルの選択"
            };

            if (ofd.ShowDialog() == true)
            {
                LoadSoundFont(ofd.FileName);
            }
        }

        private void CmbMidiPorts_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized && CmbMidiPorts.SelectedIndex >= 0)
            {
                NativeMethods.Synth_OpenMidiPort(CmbMidiPorts.SelectedIndex);
            }
        }

        private void CmbAudioDevices_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_isInitialized && CmbAudioDevices.SelectedIndex >= 0)
            {
                NativeMethods.Synth_StartAudio(CmbAudioDevices.SelectedIndex);
            }
        }

        private void SldVolume_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtVolumeVal != null)
            {
                int val = (int)e.NewValue;
                TxtVolumeVal.Text = $"{val}%";
                if (_isInitialized)
                {
                    NativeMethods.Synth_SetMasterVolume(val / 100.0f);
                }
            }
        }

        private void ChkPedal_Changed(object? sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                NativeMethods.Synth_SetPedalEnabled(ChkPedal.IsChecked == true);
            }
        }

        private void BtnPanic_Click(object? sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                NativeMethods.Synth_AllNotesOff();
            }
        }
    }
}
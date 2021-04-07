using HardwareRetroAchievements.Core.AchievementData;
using HardwareRetroAchievements.Core.Console;
using HardwareRetroAchievements.Core.Console.SNES;
using HardwareRetroAchievements.Core.Evaluator;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HardwareRetroAchievements
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Usb2Snes _console = new Usb2Snes();
        private SnesConsoleRam _consoleRam;
        private AchievementSet _achievementSet;
        private EvaluatorThread _evaluatorThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            connectToSNES();
        }

        private void buttonOpenAchievement_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Achievement Set (*.json)|*.json"
            };

            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                _achievementSet = AchievementSetParser.ParseFile(dialog.FileName);
                if (_achievementSet != null)
                {
                    outputLine($"Evaluating the achievements for {_achievementSet.Title}");
                    _consoleRam = new SnesConsoleRam(_console);

                    _evaluatorThread = new EvaluatorThread
                    {
                        RefreshTime = 0,
                        ConsoleRam = _consoleRam
                    };

                    _evaluatorThread.AchievementTriggered += OnAchievementTriggered;
                    _evaluatorThread.Setup(_achievementSet);
                    _evaluatorThread.Start();
                }
            }
        }

        private void OnAchievementTriggered(Achievement achievement)
        {
            outputLine($"Achievement unlocked! {achievement.Title} - {achievement.Description} ({achievement.Points} Points)");
        }

        private async void connectToSNES()
        {
            await Task.Run(async () => {
                CancellationTokenSource token = new CancellationTokenSource();

                if (!_console.IsConnected)
                {
                    output("Connecting...");
                    await _console.Connect(token);
                    outputLine("....Connected!" );

                    await _console.SetApplicationName("Hardware Retro Achievements", token);
                }

                var result = await _console.GetDeviceList(token);

                Usb2Snes.Info deviceInfo = null;

                if (result != null)
                {
                    outputLine($"Device(s) found: {string.Join(", ", result)}");
                    await _console.AttachToDevice(result[0], token);
                    outputLine($"Attached to device {result[0]}");

                    deviceInfo = await _console.GetInfo(token);
                    if (deviceInfo != null)
                    {
                        outputLine($"Firmware version: {deviceInfo.FirmwareVersion}");
                        outputLine($"Device Name: {deviceInfo.DeviceName}");
                        outputLine($"Current ROM: {deviceInfo.CurrentROM}");
                        outputLine($"Flags: {string.Join(", ", deviceInfo.Flags)}");
                    }
                    else
                    {
                        outputLine("No Info received!");
                    }
                }
                else
                {
                    outputLine("No device found!");
                    _console.Disconnect(token);
                }
            });
        }

        private void output(string message)
        {
            Dispatcher.Invoke(() =>
            {
                textOutput.Text += message;
            });
        }

        private void outputLine(string message)
        {
            Dispatcher.Invoke(() =>
            {
                textOutput.Text += message;
                textOutput.Text += "\n";
                textOutput.ScrollToEnd();
            });
        }
    }
}

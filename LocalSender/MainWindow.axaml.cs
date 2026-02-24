using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace LocalSender
{
    public partial class MainWindow : Window
    {

        Avalonia.Controls.TextBlock help;
        Avalonia.Controls.TextBlock textBlock;
        Avalonia.Controls.TextBlock TitleText;
        Avalonia.Controls.Border textBorder;
        string? fileToShare;
        public MainWindow()
        {
            InitializeComponent();

            var sendBtn = this.Find<Button>("SendButton");
            textBlock = this.Find<TextBlock>("TextBlock");
            var CloseBtn = this.Find<Button>("CloseButton");
            var MinBtn = this.Find<Button>("MinimizeButton");
            var TitleBar = this.Find<Grid>("Title");
            TitleText = this.Find<TextBlock>("textTitle");
            textBorder = this.Find<Border>("TextBorder");
            help = this.Find<TextBlock>("Help");

            help.PointerEntered += (s, e) =>
            {
                help.Foreground = SolidColorBrush.Parse("#007AFF");
            };
            help.PointerExited += (s, e) =>
            {
                help.Foreground = SolidColorBrush.Parse("#333333");
            };
            help.PointerPressed += async (s, e) =>
            {
                Process.Start("cmd.exe", "/k ipconfig");

                
                var box = MessageBoxManager.GetMessageBoxStandard(new MsBox.Avalonia.Dto.MessageBoxStandardParams
                {
                    ContentTitle = "Помощь",
                    ContentMessage = "В окне командной строки найдите ваше активное сетевое подключение. Скопируйте значение из строки IPv4-адрес, вставьте его в адресную строку браузера на другом устройстве и добавьте в конце :8085.\nНапример: 192.168.0.207:8085",

                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ButtonDefinitions = ButtonEnum.Ok,
                    Topmost = true

                });
                await box.ShowAsync();

            };
            textBorder.PointerEntered += (s, e) =>
            {
                if (fileToShare == null)
                {
                    textBorder.BorderBrush = SolidColorBrush.Parse("#007AFF");
                    textBorder.BorderThickness = new Thickness(2);
                }
            };
            textBorder.PointerExited += (s, e) =>
            {
                if (fileToShare == null)
                {
                    returnTextBlockToDefault();
                }
            };

            AddHandler(DragDrop.DropEvent, OnFileDrop);

            this.Activated += async (s, e) =>
            {
                this.Opacity = 0;
                for (double i = 0; i < 1; i+=0.1)
                {
                    this.Opacity = i;
                    await Task.Delay(20);
                }
            };

            CloseBtn.Click += async (s, e) =>
            {
                for (double i = 1.0; i >= 0; i -= 0.1)
                {
                    this.Opacity = i;
                    await Task.Delay(20);
                }
                this.Close();
            };

            MinBtn.Click += async (s, e) =>
            {
                for (double i = 1.0; i >= 0.8; i -= 0.05)
                {
                    this.Opacity = i;
                    await Task.Delay(20);
                }
                this.WindowState = WindowState.Minimized;
                this.Opacity = 1.0;
            };

            TitleBar.PointerPressed += (s, e) => this.BeginMoveDrag(e);

            sendBtn.Click += async (s, e) =>
            {
                if (fileToShare != null && File.Exists(fileToShare))
                {
                    Task.Run(() => LocalSend());
                    textBlock.Text = "Файл отправлен";
                }
            };

        }

        

        private void OnFileDrop(object? sender, DragEventArgs e)
        {
            var files = e.Data.GetFiles();

            if (files != null)
            {
                var firstFile = files.FirstOrDefault();
                if (firstFile != null)
                {
                    string? filePath = firstFile.TryGetLocalPath();

                    if (filePath != null)
                    {
                        fileToShare = filePath;
                        textBlock.Text = "Файл готов к отправке";
                        textBorder.BorderBrush = SolidColorBrush.Parse("#007AFF");
                        textBorder.BorderThickness = new Thickness(2);
                    }
                }
            }
        }

        private async Task LocalSend()
        {
            if (string.IsNullOrEmpty(fileToShare) || !File.Exists(fileToShare)) return;

            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://+:8085/");
                try
                {
                    listener.Start();
                }
                catch (HttpListenerException ex)
                {
                    string exPrefix = ex.ErrorCode == 5
                        ? "\nЧтобы исправить эту ошибку, выполните инструкцию в Readme"
                        : "";
                    await MessageBoxManager.GetMessageBoxStandard("Ошибка", $"{ex.Message}" + exPrefix, ButtonEnum.Ok).ShowAsync();
                    return;
                }

                var context = await listener.GetContextAsync();

                using (var response = context.Response)
                {
                    try
                    {
                        response.ContentType = "application/octet-stream";
                        string fileName = Path.GetFileName(fileToShare);
                        string RUSSIANFileName = Uri.EscapeDataString(fileName);
                        response.AddHeader("Content-Disposition", $"attachment; filename=\"{RUSSIANFileName}\"");

                        using (FileStream fs = new FileStream(fileToShare, FileMode.Open, FileAccess.Read))
                        {
                            response.ContentLength64 = fs.Length;
                            await fs.CopyToAsync(response.OutputStream);
                        }
                    }
                    finally
                    {
                        response.OutputStream.Close();
                    }
                }

                listener.Stop(); 
            }

            fileToShare = null;
            returnTextBlockToDefault();
        }
        private void returnTextBlockToDefault()
        {
            textBlock.Text = "Перенесите файл на окно";
            textBorder.BorderBrush = SolidColorBrush.Parse("#D1D1D6");
            textBorder.BorderThickness = new Thickness(1);
        }

    }
}
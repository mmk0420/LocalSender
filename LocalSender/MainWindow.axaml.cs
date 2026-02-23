using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;
using QRCoder;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Tmds.DBus.Protocol;

namespace LocalSender
{
    public partial class MainWindow : Window
    {

        Avalonia.Controls.TextBox textInput;
        Avalonia.Controls.Image Img;
        Avalonia.Controls.TextBlock TitleText;
        string? fileToShare;
        public MainWindow()
        {
            InitializeComponent();

            textInput = this.Find<TextBox>("ContentInput");
            var sendBtn = this.Find<Button>("SendButton");
            Img = this.Find<Avalonia.Controls.Image>("QRCode");
            var CloseBtn = this.Find<Button>("CloseButton");
            var MinBtn = this.Find<Button>("MinimizeButton");
            var TitleBar = this.Find<Grid>("Title");
            var ipBtn = this.Find<Button>("IPbtn");
            TitleText = this.Find<TextBlock>("textTitle");

            ipBtn.Click += (s, e) => LocalSend();

            AddHandler(DragDrop.DropEvent, OnFileDrop);

            this.Activated += async (s, e) =>
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard == null) return;

                var text = await clipboard.GetTextAsync();

                if (string.IsNullOrWhiteSpace(text)) return;
                text = text.Trim();

                bool isLink = text.StartsWith("http", StringComparison.OrdinalIgnoreCase);
                bool isFieldEmpty = string.IsNullOrWhiteSpace(textInput.Text);
                if (isLink && isFieldEmpty)
                {
                    textInput.Text = text;
                    textInput.BorderBrush = Brushes.LightGreen;
                    await Task.Delay(1000);
                    textInput.BorderBrush = new SolidColorBrush(Color.Parse("#007AFF"));
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
                if (string.IsNullOrWhiteSpace(textInput.Text))
                {
                    textInput.BorderBrush = Brushes.Crimson;
                    await Task.Delay(1000);
                    textInput.BorderBrush = new SolidColorBrush(Color.Parse("#D1D1D6"));
                }
                else
                {
                    string QRtext = textInput.Text;
                    textInput.BorderBrush = new SolidColorBrush(Color.Parse("#007AFF"));
                    Img.Source = QRgenerator(QRtext);
                }
            };
        }

        private Avalonia.Media.Imaging.Bitmap QRgenerator(string QRtext)
        {
            QRCodeGenerator QRgen = new QRCodeGenerator();
            QRCodeData QRdata = QRgen.CreateQrCode(QRtext, QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode = new PngByteQRCode(QRdata);
            byte[] QRpng = qrCode.GetGraphic(20);

            using (var ms = new MemoryStream(QRpng))
            {
                var bm = new Avalonia.Media.Imaging.Bitmap(ms);
                return bm;
            }
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

                        textInput.Text = $"Файл готов: {System.IO.Path.GetFileName(filePath)}";
                        textInput.BorderBrush = Brushes.DeepSkyBlue;
                        textInput.BorderThickness = new Avalonia.Thickness(2);
                    }
                }
            }
        }

        private async Task LocalSend()
        {
            if (fileToShare != null && File.Exists(fileToShare))
            {
                var listener = new HttpListener();
                listener.Prefixes.Add("http://+:8085/");
                try
                {
                    listener.Start();
                    Img.Source = QRgenerator("http://192.168.0.207:8085");
                    TitleText.Text = "zaebok";
                }
                catch (Exception ex) { TitleText.Text = $"Ошибка: {ex.Message}"; }

                while (true)
                {
                    var context = await listener.GetContextAsync();
                    var response = context.Response;

                    response.ContentType = "application/octet-stream";

                    string fileName = Path.GetFileName(fileToShare);
                    string RUSSIAfileName = Uri.EscapeDataString(fileName);

                    response.AddHeader("Content-Disposition", $"attachment; filename=\"{RUSSIAfileName}\""); 

                    using (FileStream fs = new FileStream(fileToShare, FileMode.Open, FileAccess.Read))
                    {
                        response.ContentLength64 = fs.Length;
                        byte[] buffer = new byte[81920];
                        int bytesRead;

                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            response.OutputStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    response.OutputStream.Close();
                }
            }
        }

    }
}
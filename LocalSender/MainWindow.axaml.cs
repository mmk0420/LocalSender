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

namespace LocalSender
{
    public partial class MainWindow : Window
    {
        string? fileToShare;
        public MainWindow()
        {
            InitializeComponent();

            var textInput = this.Find<TextBox>("ContentInput");
            var sendBtn = this.Find<Button>("SendButton");
            var Img = this.Find<Avalonia.Controls.Image>("QRCode");
            var CloseBtn = this.Find<Button>("CloseButton");
            var MinBtn = this.Find<Button>("MinimizeButton");
            var TitleBar = this.Find<Grid>("Title");
            var test = this.Find<Button>("testbtn");

            test.Click += (s, e) =>
            {
                string ip = GetLocalIp();
                string shareURL = $"http://{ip}:8080/";

                Img.Source = QRgenerator(shareURL);

                Task.Run(() => StartServer()); 
            };
            

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

        private string GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private async Task StartServer() //вы не представляете как мне нравится все это писать, я себя реально гением чувствую когда чтото получается
        {
            var listenner = new HttpListener();
            var ip = GetLocalIp();

            listenner.Prefixes.Add($"http://{ip}:8080/"); //КТО Ж СУКА ЗНАЛ ЧТО ПРОСТО {ip}:{port} ЭТОЙ МРАЗИ НЕДОСТАТОЧНО
            listenner.Start();

            string shareurl = $"http://{ip}:8080/"; 

            while(true)
            {
                var context = await listenner.GetContextAsync();
                var response = context.Response;

                if (fileToShare != null && File.Exists(fileToShare))
                {

                }
            }


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
            var textInput = this.Find<TextBox>("ContentInput");

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

    }
}
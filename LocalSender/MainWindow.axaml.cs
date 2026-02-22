using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Metadata;
using QRCoder;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LocalSender
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var textInput = this.Find<TextBox>("ContentInput");
            var sendBtn = this.Find<Button>("SendButton");
            var Img = this.Find<Avalonia.Controls.Image>("QRCode");
            var CloseBtn = this.Find<Button>("CloseButton");
            var MinBtn = this.Find<Button>("MinimizeButton");
            var TitleBar = this.Find<Grid>("Title");

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
                    textInput.BorderBrush = new SolidColorBrush(Color.Parse("#007AFF"));
                    QRCodeGenerator QRgen = new QRCodeGenerator();
                    QRCodeData QRdata = QRgen.CreateQrCode(textInput.Text, QRCodeGenerator.ECCLevel.Q);

                    PngByteQRCode qrCode = new PngByteQRCode(QRdata);
                    byte[] QRpng = qrCode.GetGraphic(20);

                    using (var ms = new MemoryStream(QRpng))
                    {
                        var bm = new Avalonia.Media.Imaging.Bitmap(ms);
                        Img.Source = bm;
                    }
                }
            };
        }

        private void AutoPasteFromClipboard()
        {
            
        }

    }
}
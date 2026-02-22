using Avalonia.Controls;
using QRCoder;
using Avalonia.Media.Imaging;
using System.IO;
using System.Drawing; 

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

            sendBtn.Click += (s, e) =>
            {
                var text = textInput.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    this.Title = $"Отправляю: {text}";
                    QRCodeGenerator QRgen = new QRCodeGenerator();
                    QRCodeData QRdata = QRgen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

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
    }
}
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QRReader.Models;
using QRReader.QrDB;
using System.Diagnostics;
using static QRCoder.PayloadGenerator;

namespace QRReader.Controllers
{
    public class HomeController(QrCodesDb db) : Controller
    {
        private readonly QRCodeGenerator qrGenerator = new();
        public IActionResult Index()
        {
            var qrCodes = new Dictionary<string, string>
            {
                {"Basic String", GenerateQRCodeString()},
                {"URL", GenerateQRCodeURL() },
                { "Phone Number", GenerateQRCodePhoneNumber() },
                { "Custom", GenerateQRCodeCustom() }
            };
            var model = new HomeModel(qrCodes);
            return View(model);
        }

        private string GenerateQRCodeString()
        {
            var qrCodeData = qrGenerator.CreateQrCode("Hello CodeMaze readers", QRCodeGenerator.ECCLevel.Q, requestedVersion: 3);
            return GeneratePng(qrCodeData);
        }

        private static string GeneratePng(QRCodeData data)
        {
            using var qrCode = new PngByteQRCode(data);
            var qrCodeImage = qrCode.GetGraphic(20, [255, 0, 0], [0, 0, 139]);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
        }

        private string GenerateQRCodeURL()
        {
            var qrCodeData = qrGenerator.CreateQrCode(new Url("https://www.code-maze.com"));
            return GeneratePng(qrCodeData);
        }

        private string GenerateQRCodePhoneNumber()
        {
            var qrCodeData = qrGenerator.CreateQrCode(new PhoneNumber("+123456789"));

            return GeneratePng(qrCodeData);
        }

        private string GenerateQRCodeCustom()
        {
            const string key = "Custom";
            var raw = db.Get(key);
            if (raw == null)
            {
                var qrCodeData = qrGenerator.CreateQrCode(new CustomPayload("Reader"));
                db.Add(key, qrCodeData);
                raw = qrCodeData.GetRawData(QRCodeData.Compression.Uncompressed);
            }

            return GeneratePng(new QRCodeData(raw, QRCodeData.Compression.Uncompressed));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    public class CustomPayload(string name) : Payload
    {
        private readonly string _name = name;
        public override string ToString()
        {
            return $"Hello {_name}, hope you're having a great time :) ";
        }
    }
}

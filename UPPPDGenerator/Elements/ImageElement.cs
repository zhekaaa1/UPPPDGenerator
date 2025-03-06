using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace UPPPDGenerator.Elements
{
    public class ImageElement
    {
        public string ElementId { get; private set; }
        public string ImageName { get; set; } = "image.png"; // Название файла
        public string Alignment { get; set; } = "Center"; // Выравнивание: Left, Center, Right

        // Отступы в относительных значениях (0.0 - 1.0)
        public double MarginLeft { get; set; } = 0.1;
        public double MarginTop { get; set; } = 0.1;
        public double MarginRight { get; set; } = 0.1;
        public double MarginBottom { get; set; } = 0.1;

        // Фиктивное изображение (по умолчанию будет заполнено при сохранении)
        public bool IsPlaceholder { get; set; } = false;
        public string PlaceholderImagePath { get; set; } = string.Empty; // Путь к фиктивному изображению
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 100;
        public Thickness Margin { get; set; } = new Thickness(0);

        public ImageElement()
        {
            ElementId = GenerateHash();
        }

        private string GenerateHash()
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
            }
        }

        public Image ToUIElement()
        {
            Image img = new Image
            {
                Width = Width,
                Height = Height,
                Margin = Margin,
                Stretch = Stretch.Uniform
            };

            if (!string.IsNullOrEmpty(PlaceholderImagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(PlaceholderImagePath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                img.Source = bitmap;
            }
            return img;
        }
    }
}

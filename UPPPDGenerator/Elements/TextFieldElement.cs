using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace UPPPDGenerator.Elements
{
    public class TextFieldElement
    {
        public int OrderIndex { get; set; } // Указывает на порядок следования
        public string ElementId { get; private set; }
        public string Placeholder { get; set; } = "Введите текст...";
        public string Description { get; set; } = "Описание поля";

        public TextFieldElement()
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

        public StackPanel ToUIElement()
        {
            StackPanel panel = new StackPanel();

            TextBox inputBox = new TextBox
            {
                Text = Placeholder,
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = Brushes.Black,
                FontSize = 14
            };

            TextBlock description = new TextBlock
            {
                Text = Description,
                FontSize = 10,
                Foreground = Brushes.Gray
            };

            panel.Children.Add(inputBox);
            panel.Children.Add(description);
            return panel;
        }
    }
}

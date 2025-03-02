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
    // Поле для подписи
    public class SignatureFieldElement
    {
        public int OrderIndex { get; set; } // Указывает на порядок следования
        public string ElementId { get; private set; }
        public string LeftLabel { get; set; } = "Подпись";
        public string RightLabel { get; set; } = "Расшифровка";

        public SignatureFieldElement()
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

            StackPanel lines = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
            Border leftLine = new Border { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, Width = 100, Height = 20 };
            TextBlock slash = new TextBlock { Text = "/", FontSize = 14, Margin = new Thickness(5, 0, 5, 0) };
            Border rightLine = new Border { BorderThickness = new Thickness(0, 0, 1, 1), BorderBrush = Brushes.Black, Width = 150, Height = 20 };

            lines.Children.Add(leftLine);
            lines.Children.Add(slash);
            lines.Children.Add(rightLine);

            StackPanel labels = new StackPanel { Orientation = Orientation.Horizontal };
            TextBlock leftLabel = new TextBlock { Text = LeftLabel, FontSize = 10, Margin = new Thickness(10, 0, 20, 0), Foreground = Brushes.Gray };
            TextBlock rightLabel = new TextBlock { Text = RightLabel, FontSize = 10, Foreground = Brushes.Gray };

            labels.Children.Add(leftLabel);
            labels.Children.Add(rightLabel);

            panel.Children.Add(lines);
            panel.Children.Add(labels);
            return panel;
        }
    }
}

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
    // Поле ввода даты
    public class DateFieldElement
    {
        public int OrderIndex { get; set; } // Указывает на порядок следования
        public string ElementId { get; private set; }
        public DateFieldElement()
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
            StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };

            TextBox day = new TextBox { Width = 20, BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = Brushes.Black, FontSize = 14 };
            TextBlock separator1 = new TextBlock { Text = "\"\"", FontSize = 14, Margin = new Thickness(5, 0, 5, 0) };
            TextBox month = new TextBox { Width = 50, BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = Brushes.Black, FontSize = 14 };
            TextBlock separator2 = new TextBlock { Text = "20", FontSize = 14, Margin = new Thickness(5, 0, 0, 0) };
            TextBox year = new TextBox { Width = 30, BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = Brushes.Black, FontSize = 14 };
            TextBlock endText = new TextBlock { Text = "г.", FontSize = 14, Margin = new Thickness(5, 0, 0, 0) };

            panel.Children.Add(day);
            panel.Children.Add(separator1);
            panel.Children.Add(month);
            panel.Children.Add(separator2);
            panel.Children.Add(year);
            panel.Children.Add(endText);

            return panel;
        }
    }
}

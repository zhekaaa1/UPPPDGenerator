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
    public class ParagraphElement
    {
        public string ElementId { get; private set; } // Уникальный идентификатор
        public string Text { get; set; } = "Введите текст..."; // Текст абзаца
        public string FontFamily { get; set; } = "Times New Roman"; // Шрифт
        public double FontSize { get; set; } = 14; // Размер шрифта
        public bool IsEmpty { get; set; } = true;
        public TextAlignment Alignment { get; set; } = TextAlignment.Left; // Выравнивание текста
        public Thickness Margin { get; set; } = new Thickness(0,0,0,0); // Отступы

        public ParagraphElement()
        {
            ElementId = GenerateHash();
        }
        // Свойства для отдельных отступов, которые обновляют общий Margin
        public double MarginLeft
        {
            get { return Margin.Left; }
            set { Margin = new Thickness(value, Margin.Top, Margin.Right, Margin.Bottom); }
        }

        public double MarginTop
        {
            get { return Margin.Top; }
            set { Margin = new Thickness(Margin.Left, value, Margin.Right, Margin.Bottom); }
        }

        public double MarginRight
        {
            get { return Margin.Right; }
            set { Margin = new Thickness(Margin.Left, Margin.Top, value, Margin.Bottom); }
        }

        public double MarginBottom
        {
            get { return Margin.Bottom; }
            set { Margin = new Thickness(Margin.Left, Margin.Top, Margin.Right, value); }
        }
        public double FirstLineIndentation { get; set; } = 1.25;

        public string LineSpacingType { get; set; } = "1.5"; // "1", "1.5", "2", "multiply"
        public int LineSpacingValue { get; set; } = 12; // Используется только если выбран "multiply"

        private string GenerateHash()
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString().Substring(0, 16); // Обрезаем до 16 символов
            }
        }

        public TextBlock ToUIElement()
        {
            return new TextBlock
            {
                Text = Text,
                FontFamily = new FontFamily(FontFamily),
                FontSize = FontSize,
                TextAlignment = Alignment,
                Margin = Margin,
                Foreground = Brushes.Black // Чёрный текст
            };
        }
    }
}

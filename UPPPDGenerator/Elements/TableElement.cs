using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace UPPPDGenerator.Elements
{
    public class TableElement
    {
        public string ElementId { get; private set; }
        public int Rows { get; set; } = 2;
        public int Columns { get; set; } = 2;
        public Thickness Margin { get; set; } = new Thickness(0);

        public TableElement()
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

        public Grid ToUIElement()
        {
            Grid table = new Grid { Margin = Margin };

            for (int i = 0; i < Rows; i++)
                table.RowDefinitions.Add(new RowDefinition());

            for (int j = 0; j < Columns; j++)
                table.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Border cell = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Child = new TextBlock
                        {
                            Text = "Текст",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    };
                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    table.Children.Add(cell);
                }
            }
            return table;
        }
    }
}

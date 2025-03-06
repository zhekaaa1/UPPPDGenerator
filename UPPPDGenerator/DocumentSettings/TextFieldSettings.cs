using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPPDGenerator.DocumentSettings
{
    public class TextFieldSettings
    {
        public string FontFamily { get; set; } // Название шрифта (Times New Roman, Arial и т. д.)
        public int FontSize { get; set; } // Размер шрифта в pt

        public string Alignment { get; set; } // Выравнивание (левый край, центр и т. д.)

        public double MarginLeft { get; set; } // Отступ слева в см
        public double MarginRight { get; set; } // Отступ справа в см
        public double MarginTop { get; set; } // Отступ сверху в см
        public double MarginBottom { get; set; } // Отступ снизу в см
        public double FirstLineIndentation { get; set; } // Отступ первой строки в см

        public string LineSpacingType { get; set; } // Тип межстрочного интервала (Одинарный, Полуторный, Двойной, Множитель)
        public double LineSpacingMultiplier { get; set; } // Множитель, если LineSpacingType = "Множитель"
    }
}

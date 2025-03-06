using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPPDGenerator.DocumentSettings
{
    public class TableSettings
    {
        public bool NumberColumns { get; set; } // Нумеровать столбцы в таблицах
        public bool NameTables { get; set; } // Давать названия таблицам
        public bool ColorizeHeaders { get; set; } // Закрашивать верхние ячейки
        public bool EnableNumbering { get; set; } // Нумеровать таблицы
        public string HeaderColor { get; set; } // Цвет заголовков (Голубой, Желтоватый и т. д.)

        public bool UseDefaultParagraphSettings { get; set; } // Использовать параметры абзацев

        public string FontFamily { get; set; } // Шрифт
        public int FontSize { get; set; } // Размер шрифта

        public string ParagraphAlignment { get; set; } // Выравнивание текста (По левому краю, По центру и т. д.)

        public double MarginLeft { get; set; } // Отступ слева
        public double MarginRight { get; set; } // Отступ справа
        public double MarginTop { get; set; } // Отступ сверху
        public double MarginBottom { get; set; } // Отступ снизу
        public double FirstLineIndentation { get; set; } // Отступ первой строки

        public string LineSpacing { get; set; } // Межстрочный интервал (Одинарный, Полуторный, Двойной, Множитель)
        public double LineSpacingMultiplier { get; set; } // Множитель для межстрочного интервала

        public string VerticalAlignment { get; set; } // Выравнивание текста по вертикали (Сверху, По центру, Снизу)
    }
}

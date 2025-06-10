using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;
using UPPPDGenerator.Elements;

namespace UPPPDGenerator.DocumentSettings
{
    public class ImageSettings
    {
        public bool EnableNumbering { get; set; } // Делать нумерацию изображений (Рисунок 1, 2 и т. д.)
        public bool EnableDescriptions { get; set; } // Давать названия изображениям (Рисунок 1 — Описание)
        public string Alignment { get; set; } // Выравнивание (По левому краю, По центру и т. д.)
    }
    public class PageSettings
    {
        public string Name { get; set; } // Название типа полей ("Обычные", "Узкие" и т. д.)
        public double TopMargin { get; set; } // Верхнее поле
        public double BottomMargin { get; set; } // Нижнее поле
        public double LeftMargin { get; set; } // Левое поле
        public double RightMargin { get; set; } // Правое поле
    }
    public class Settings
    {
        public PageSettings PageSettings { get; set; }
        public TextFieldSettings TextFieldSettings { get; set; }
        public ImageSettings ImageSettings { get; set; }
        public TableSettings TableSettings { get; set; }
    }
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
    public class TemplateJsonStructure
    {
        public string TemplateName { get; set; } = "";
        public string TemplatePasswordHash { get; set; } = "";
        public string TemplateDescription { get; set; } = "";
        public AuthorInfo CreatedBy { get; set; } = new AuthorInfo { Id = 0, FullName = "AnonymousUser" };
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool UseTitlePage { get; set; } = false;
        public string TitlePageXml { get; set; } = "";
        public List<TitlePageElement> TitlePageElements { get; set; } = new List<TitlePageElement>();
        public Settings DocumentSettings { get; set; } = new Settings();
        public int TemplateIdFromDatabase { get; set; } = 0;
    }
    public class AuthorInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
    }
}

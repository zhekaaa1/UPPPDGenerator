using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPPDGenerator.DocumentSettings
{
    public class PageSettings
    {
        public string Name { get; set; } // Название типа полей ("Обычные", "Узкие" и т. д.)
        public double TopMargin { get; set; } // Верхнее поле
        public double BottomMargin { get; set; } // Нижнее поле
        public double LeftMargin { get; set; } // Левое поле
        public double RightMargin { get; set; } // Правое поле
    }
}

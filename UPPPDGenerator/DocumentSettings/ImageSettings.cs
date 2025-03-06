using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPPDGenerator.DocumentSettings
{
    public class ImageSettings
    {
        public bool EnableNumbering { get; set; } // Делать нумерацию изображений (Рисунок 1, 2 и т. д.)
        public bool EnableDescriptions { get; set; } // Давать названия изображениям (Рисунок 1 — Описание)
        public string Alignment { get; set; } // Выравнивание (По левому краю, По центру и т. д.)
    }
}

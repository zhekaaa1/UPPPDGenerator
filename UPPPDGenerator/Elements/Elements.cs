using System;
using System.Collections.Generic;
using UPPPDGenerator.DocumentSettings;
namespace UPPPDGenerator.Elements
{
    public class TitlePageElement
    {
        public string ElementGuid { get; set; }
        public ElementType ElementType { get; set; }
        public object ElementProperties { get; set; } 
        public TitlePageElement() 
        {
            ElementGuid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "").Replace("/", "");
        }
    }
    public enum ElementType
    {
        DefaultParagraph,
        EmptyParagraph,
        DataField,
        Separator
    }
    public class DefaultFieldProperties
    {
        public string Content { get; set; } = string.Empty;
        public TextFieldSettings TextFieldSettings { get; set; } = new TextFieldSettings();
    }
    public class DataFieldProperties
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public ContentType ContentType { get; set; } = ContentType.String;
        public List<string> DataElements { get; set; } = new List<string>();
        public TextFieldSettings TextProperties { get; set; } = new TextFieldSettings();
    }

    public enum ContentType
    {
        String,
        Int,
        Enumerable
    }
    public class SeparatorProperties
    {
        public LineType LineType { get; set; } = LineType.Solid;
        public int LineHeight { get; set; } = 1; // от 1 до 5
        public string LineColor { get; set; } = "#000000"; // HEX цвет
    }
    public enum LineType
    {
        Solid,
        Double,
        Bold,
        Dotted,
        Wavy
    }
}
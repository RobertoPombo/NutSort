using System;

namespace NutSort.Models
{
    public class NutColor
    {
        public static readonly List<NutColor> List = [];

        public NutColor() { }
        public NutColor(string name, byte red, byte green, byte blue)
        {
            Name = name;
            Red = red;
            Green = green;
            Blue = blue;
            if (IsValid())
            {
                List.Add(this);
            }
        }

        public string Name { get; set; } = string.Empty;
        public byte Red { get; set; } = byte.MinValue;
        public byte Green { get; set; } = byte.MinValue;
        public byte Blue { get; set; } = byte.MinValue;

        public System.Drawing.Color Preview { get { return System.Drawing.Color.FromArgb(Red, Green, Blue); } }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}

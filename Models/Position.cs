using System;

namespace NutSort.Models
{
    public class Position
    {
        public Position() { }

        public Stack Stack { get; set; } = new();
        public byte Level { get; set; } = byte.MinValue;
    }
}

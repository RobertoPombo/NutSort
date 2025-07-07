using System;

namespace NutSort.Models
{
    public class Position
    {
        public Position() { }

        public byte StackNr { get; set; } = byte.MinValue;
        public byte Level { get; set; } = byte.MinValue;
    }
}

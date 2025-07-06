using System;

namespace NutSort.Models
{
    public class Position
    {
        public Position() { }
        public Position(Stack stack, byte level)
        {
            Stack = stack;
            Level = level;
        }

        public Stack Stack { get; set; } = new();
        public byte Level { get; set; } = byte.MinValue;
    }
}

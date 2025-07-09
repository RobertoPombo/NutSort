using System;

namespace NutSort.Models
{
    public class Nut
    {
        private static int nextId = 0;

        public Nut() { }
        public Nut(NutColor nutColor)
        {
            Id = nextId;
            NutColor = nutColor;
        }

        private int id = -1;
        public int Id
        {
            get { return id; }
            set { id = value; nextId = Math.Max(value + 1, nextId + 1); }
        }

        public NutColor NutColor { get; set; } = new();
    }
}

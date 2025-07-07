using System;

namespace NutSort.Models
{
    public class Nut
    {
        private static int nextId = 0;

        public Nut() { }
        public Nut(NutColor _nutColor)
        {
            Id = nextId;
            nutColor = _nutColor;
        }

        private int id = -1;
        public int Id
        {
            get { return id; }
            set { id = value; nextId = Math.Max(value + 1, nextId + 1); }
        }

        private NutColor nutColor = new();
        public NutColor NutColor
        {
            get { return nutColor; }
            set
            {
                NutColor? _nutColor = NutColor.GetByName(value.Name);
                if (_nutColor is null)
                {
                    NutColor.List.Add(value);
                    nutColor = value;
                }
                else { nutColor = _nutColor; }
            }
        }

        public List<Position> Positions { get; set; } = [];
    }
}

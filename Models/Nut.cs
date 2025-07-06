using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Nut
    {
        private static int nextId = -1;

        public Nut() { }
        public Nut(NutColor nutColor)
        {
            nextId++;
            Id = nextId;
            NutColor = nutColor;
            NutColor.List.Add(NutColor);
        }

        public int Id { get; set; } = -1;
        public NutColor NutColor { get; set; } = new();
        [JsonIgnore] public List<Position> Positions { get; set; } = [];
    }
}

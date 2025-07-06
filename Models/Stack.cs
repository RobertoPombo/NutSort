using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Stack
    {
        public Stack() { }
        public Stack(List<Nut> nuts, Boardstate boardstate)
        {
            Nuts = nuts;
            Boardstate = boardstate;
            for (byte nutNr = 0; nutNr < Nuts.Count; nutNr++)
            {
                Nuts[nutNr].Positions.Add(new(this, nutNr));
            }
        }

        [JsonIgnore] public Boardstate Boardstate { get; set; } = new();
        public List<Nut> Nuts { get; set; } = [];

        public Nut? TopNut
        {
            get
            {
                if (IsEmpty) { return null; }
                return Nuts[^1];
            }
        }

        public byte TopNutCount
        {
            get
            {
                if (IsEmpty) { return 0; }
                else
                {
                    byte amount = 1;
                    NutColor nutColor = TopNut!.NutColor;
                    foreach (Nut nut in Nuts)
                    {
                        if (nut.NutColor != nutColor) { break; }
                        else { amount++; }
                    }
                    return amount;
                }
            }
        }

        public bool IsEmpty
        {
            get { return Nuts.Count == 0; }
        }

        public bool IsFull
        {
            get { return Nuts.Count == Boardstate.Solution.Board.StackHeight; }
        }

        public bool IsMonochromatic
        {
            get
            {
                if (IsEmpty) { return false; }
                NutColor firstNutColor = Nuts[0].NutColor;
                foreach (Nut nut in Nuts)
                {
                    if (nut.NutColor != firstNutColor) { return false; }
                }
                return true;
            }
        }

        public bool IsFinished
        {
            get
            {
                if (IsEmpty) { return true; }
                if (!IsMonochromatic) { return false; }
                if (Nuts.Count != Boardstate.Solution.Board.ColorCount) { return false; }
                return true;
            }
        }
    }
}

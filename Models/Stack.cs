using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Stack
    {
        public Stack() { }

        public Boardstate Boardstate { get; set; } = new();
        public List<Nut> Nuts { get; set; } = [];

        [JsonIgnore] public byte StackHeight { get; set; } = 1;

        [JsonIgnore] public List<byte> EmptyNutSlots
        {
            get
            {
                List<byte> list = [];
                int extraSlots = (int)Math.Floor((double)(StackHeight + 3) / 10) + 1;
                for (byte nutNr = 0; nutNr < StackHeight + extraSlots; nutNr++) { list.Add(byte.MinValue); }
                return list;
            }
        }

        [JsonIgnore] public Nut? TopNut
        {
            get
            {
                if (IsEmpty) { return null; }
                return Nuts[^1];
            }
        }

        [JsonIgnore] public byte TopNutCount
        {
            get
            {
                if (TopNut is null) { return 0; }
                else
                {
                    byte amount = 1;
                    NutColor nutColor = TopNut.NutColor;
                    for (int nutNr = Nuts.Count - 2; nutNr >= 0; nutNr--)
                    {
                        if (Nuts[nutNr].NutColor.Name != nutColor.Name) { break; }
                        else { amount++; }
                    }
                    return amount;
                }
            }
        }

        [JsonIgnore] public bool IsEmpty
        {
            get { return Nuts.Count == 0; }
        }

        [JsonIgnore] public bool IsFull
        {
            get { return Nuts.Count == StackHeight; }
        }

        [JsonIgnore] public byte EmptySlotsCount
        {
            get { return (byte)(StackHeight - Nuts.Count); }
        }

        [JsonIgnore] public bool IsMonochromatic
        {
            get
            {
                if (IsEmpty) { return false; }
                NutColor firstNutColor = Nuts[0].NutColor;
                foreach (Nut nut in Nuts)
                {
                    if (nut.NutColor.Name != firstNutColor.Name) { return false; }
                }
                return true;
            }
        }

        [JsonIgnore] public bool IsFinished
        {
            get
            {
                if (IsEmpty) { return true; }
                if (!IsMonochromatic) { return false; }
                if (Nuts.Count != Boardstate.Solution.Board.NutSameColorCount) { return false; }
                return true;
            }
        }

        public override string ToString()
        {
            string description = Boardstate.ToString() + " - ";
            foreach (Nut nut in Nuts) { description += nut.ToString() + " | "; }
            if (description.Length > 3) { return description[..^3]; }
            return description;
        }
    }
}

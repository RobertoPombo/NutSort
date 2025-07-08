using System;

namespace NutSort.Models
{
    public class Move
    {
        public Move() { }

        public byte FromStackNr { get; set; } = 0;
        public byte ToStackNr { get; set; } = 0;

        public void Execute(Boardstate boardstate)
        {
            Stack fromStack = boardstate.Stacks[FromStackNr];
            Stack toStack = boardstate.Stacks[ToStackNr];
            if (fromStack.TopNut is not null)
            {
                if (fromStack.TopNut.Positions.Count > 0)
                {
                    fromStack.TopNut.Positions[^1].StackNr = ToStackNr;
                    fromStack.TopNut.Positions[^1].Level = (byte)toStack.Nuts.Count;
                }
                toStack.Nuts.Add(fromStack.TopNut);
                fromStack.Nuts.RemoveAt(fromStack.Nuts.Count - 1);
            }
        }
    }
}

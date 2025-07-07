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
            MoveNut(boardstate, FromStackNr, ToStackNr);
        }

        public void Undo(Boardstate boardstate)
        {
            MoveNut(boardstate, ToStackNr, FromStackNr);
        }

        private static void MoveNut(Boardstate boardstate, byte fromStackNr, byte toStackNr)
        {
            Stack fromStack = boardstate.Stacks[fromStackNr];
            Stack toStack = boardstate.Stacks[toStackNr];
            if (fromStack.TopNut is not null)
            {
                if (fromStack.TopNut.Positions.Count > 0)
                {
                    fromStack.TopNut.Positions[^1].StackNr = toStackNr;
                    fromStack.TopNut.Positions[^1].Level = (byte)toStack.Nuts.Count;
                }
                toStack.Nuts.Add(fromStack.TopNut);
                fromStack.Nuts.RemoveAt(fromStack.Nuts.Count - 1);
            }
        }
    }
}

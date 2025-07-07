using System;

namespace NutSort.Models
{
    public class Move
    {
        public Move() { }

        public int FromStackNr { get; set; } = 0;
        public int ToStackNr { get; set; } = 0;

        public void Execute(Boardstate boardstate)
        {
            Stack fromStack = boardstate.Stacks[FromStackNr];
            Stack toStack = boardstate.Stacks[ToStackNr];
            MoveNut(fromStack, toStack);
        }

        public void Undo(Boardstate boardstate)
        {
            Stack fromStack = boardstate.Stacks[ToStackNr];
            Stack toStack = boardstate.Stacks[FromStackNr];
            MoveNut(fromStack, toStack);
        }

        private static void MoveNut(Stack fromStack, Stack toStack)
        {
            if (fromStack.TopNut is not null)
            {
                if (fromStack.TopNut.Positions.Count > 0)
                {
                    fromStack.TopNut.Positions[^1].Stack = toStack;
                    fromStack.TopNut.Positions[^1].Level = (byte)toStack.Nuts.Count;
                }
                toStack.Nuts.Add(fromStack.TopNut);
                fromStack.Nuts.RemoveAt(fromStack.Nuts.Count - 1);
            }
        }
    }
}

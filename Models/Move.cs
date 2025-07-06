using System;

namespace NutSort.Models
{
    public class Move
    {
        public Move() { }
        public Move(int fromStackNr, int toStackNr)
        {
            FromStackNr = fromStackNr;
            ToStackNr = toStackNr;
        }

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
            toStack.Nuts.Add(fromStack.Nuts[^1]);
            fromStack.Nuts.RemoveAt(fromStack.Nuts.Count - 1);
        }
    }
}

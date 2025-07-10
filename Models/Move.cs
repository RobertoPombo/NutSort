using System;

namespace NutSort.Models
{
    public class Move
    {
        public Move() { }

        public int FromStackNr { get; set; } = -1;
        public int ToStackNr { get; set; } = -1;

        public void Execute(Boardstate boardstate)
        {
            Stack fromStack = boardstate.Stacks[FromStackNr];
            Stack toStack = boardstate.Stacks[ToStackNr];
            if (fromStack.TopNut is not null)
            {
                NutColor topNutColor = fromStack.TopNut.NutColor;
                while (fromStack.TopNut is not null && fromStack.TopNut.NutColor == topNutColor && toStack.Nuts.Count < boardstate.Solution.Board.StackHeight)
                {
                    toStack.Nuts.Add(fromStack.TopNut);
                    fromStack.Nuts.RemoveAt(fromStack.Nuts.Count - 1);
                }
            }
        }

        public bool IsPossibleAndReasonable(Boardstate boardstate)
        {
            Stack fromStack = boardstate.Stacks[FromStackNr];
            Stack toStack = boardstate.Stacks[ToStackNr];
            return (toStack.IsEmpty || fromStack.TopNut?.NutColor.Name == toStack.TopNut?.NutColor.Name) &&
                        FromStackNr != ToStackNr && !toStack.IsFull && !fromStack.IsFinished &&
                        (!fromStack.IsMonochromatic || fromStack.TopNutCount < fromStack.Nuts.Count || !toStack.IsEmpty);
        }

        public bool IsPossible(Boardstate boardstate)
        {
            Stack fromStack = boardstate.Stacks[FromStackNr];
            Stack toStack = boardstate.Stacks[ToStackNr];
            return (toStack.IsEmpty || fromStack.TopNut?.NutColor.Name == toStack.TopNut?.NutColor.Name) &&
                        FromStackNr != ToStackNr && !toStack.IsFull;
        }

        public override string ToString()
        {
            return "from #" + FromStackNr.ToString() + " to #" + ToStackNr.ToString();
        }
    }
}

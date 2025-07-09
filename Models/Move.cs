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
                NutColor topNutColor = fromStack.TopNut.NutColor;
                while (fromStack.TopNut is not null && fromStack.TopNut.NutColor == topNutColor && toStack.Nuts.Count < boardstate.Solution.Board.StackHeight)
                {
                    toStack.Nuts.Add(fromStack.TopNut);
                    fromStack.Nuts.RemoveAt(fromStack.Nuts.Count - 1);
                }
            }
        }

        public override string ToString()
        {
            return "from #" + FromStackNr.ToString() + " to #" + ToStackNr.ToString();
        }
    }
}

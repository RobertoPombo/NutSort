using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Boardstate
    {
        public Boardstate() { }
        public Boardstate(List<Stack> stacks)
        {
            foreach (Stack _stack in stacks)
            {
                List<Nut> nuts = [];
                foreach (Nut nut in _stack.Nuts) { nuts.Add(nut); }
                Stack stack = new() { Nuts = nuts, Boardstate = this };
                Stacks.Add(stack);
                for (byte nutNr = 0; nutNr < stack.Nuts.Count; nutNr++)
                {
                    stack.Nuts[nutNr].Positions.Add(new() { Stack = stack, Level = nutNr });
                }
            }
            UpdatePossibleMoves();
        }

        public Solution Solution { get; set; } = new();
        public List<Stack> Stacks { get; set; } = [];
        public List<Move> PossibleMoves { get; set; } = [];
        public int NextMoveIndex { get; set; } = 0;

        [JsonIgnore] public string Id
        {
            get
            {
                string id = string.Empty;
                foreach (Stack stack in Stacks)
                {
                    foreach (Nut nut in stack.Nuts)
                    {
                        id += nut.NutColor.Name + "|";
                    }
                }
                return id;
            }
        }

        [JsonIgnore] public bool IsFinished
        {
            get
            {
                foreach (Stack stack in Stacks)
                {
                    if (!stack.IsFinished) { return false; }
                }
                return true;
            }
        }

        public void TryMakeNextMove()
        {
            if (Solution.IsFinished)
            {
                if (Solution.Board.ShortestSolution is null || Solution.Boardstates.Count < Solution.Board.ShortestSolution.Boardstates.Count)
                {
                    Solution.Board.ShortestSolution = Solution;
                }
                Solution.Board.Solutions.Add(new(Solution.Boardstates));
            }
            else if (NextMoveIndex < PossibleMoves.Count || (Solution.Board.ShortestSolution is not null && Solution.Boardstates.Count > Solution.Board.ShortestSolution.Boardstates.Count))
            {
                if (NextMoveIndex > 0)
                {
                    PossibleMoves[NextMoveIndex - 1].Undo(this);
                }
                PossibleMoves[NextMoveIndex].Execute(this);
                NextMoveIndex++;
                Solution.Boardstates.Add(new(Stacks));
                Solution.Boardstates[^1].TryMakeNextMove();
            }
            else if (NextMoveIndex >= PossibleMoves.Count)
            {
                Solution.Board.Solutions.Remove(Solution);
            }
            else
            {
                foreach (Nut nut in Solution.Nuts)
                {
                    nut.Positions.RemoveAt(nut.Positions.Count - 1);
                }
                Solution.Boardstates.RemoveAt(Solution.Boardstates.Count - 1);
                Solution.Boardstates[^1].TryMakeNextMove();
            }
        }

        private void UpdatePossibleMoves()
        {
            for (int fromStackNr = 0; fromStackNr < Stacks.Count; fromStackNr++)
            {
                Stack fromStack = Stacks[fromStackNr];
                for (int toStackNr = 0; toStackNr < Stacks.Count; toStackNr++)
                {
                    Stack toStack = Stacks[toStackNr];
                    if ((toStack.IsEmpty || fromStack.TopNut?.NutColor.Name == toStack.TopNut?.NutColor.Name) &&
                        fromStackNr != toStackNr && !toStack.IsFull && !fromStack.IsFinished &&
                        (!fromStack.IsMonochromatic || fromStack.TopNutCount < fromStack.Nuts.Count || !toStack.IsEmpty))
                    {
                        PossibleMoves.Add(new() { FromStackNr = fromStackNr, ToStackNr = toStackNr });
                    }
                }
            }
            PrioritizeMoves();
        }

        private void PrioritizeMoves()
        {
            // todo not yet implemented: Siehe Excel-Ideen
        }
    }
}

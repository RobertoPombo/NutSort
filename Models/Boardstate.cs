using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Boardstate
    {
        public Boardstate() { }
        public Boardstate(List<Stack> stacks)
        {
            foreach (Stack _stack in Stacks)
            {
                Stack stack = new(_stack.Nuts, this);
                Stacks.Add(stack);
            }
            UpdatePossibleMoves();
        }

        [JsonIgnore] public Solution Solution { get; set; } = new();
        public List<Stack> Stacks { get; set; } = [];
        public List<Move> PossibleMoves { get; set; } = [];
        public int NextMoveIndex { get; set; } = 0;
        public string Id
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

        public bool IsFinished
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
                // todo not yet implemented: Solution deepcopy erstellen
            }
            else if (NextMoveIndex < PossibleMoves.Count)
            {
                if (NextMoveIndex > 0)
                {
                    PossibleMoves[NextMoveIndex - 1].Undo(this);
                }
                PossibleMoves[NextMoveIndex].Execute(this);
                NextMoveIndex++;
            }
            else
            {
                // todo not yet implemented: Diesen Boardstate löschen und beim vorherigen Boardstate .TryMakeNextMove() aufrufen
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
                        fromStackNr != toStackNr && !fromStack.IsEmpty && !toStack.IsFull && !fromStack.IsFinished &&
                        (!fromStack.IsMonochromatic || fromStack.TopNutCount < fromStack.Nuts.Count || !toStack.IsEmpty))
                    {
                        PossibleMoves.Add(new (fromStackNr, toStackNr));
                    }
                }
            }
            PrioritizeMoves();
        }

        private void PrioritizeMoves()
        {
            // todo not yet implemented: Siehe Excel
        }
    }
}

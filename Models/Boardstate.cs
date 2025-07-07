using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Boardstate
    {
        public Boardstate() { }
        public Boardstate(List<Stack> stacks)
        {
            for (byte stackNr = 0; stackNr < stacks.Count; stackNr++)
            {
                List<Nut> nuts = [];
                foreach (Nut nut in stacks[stackNr].Nuts) { nuts.Add(nut); }
                Stack stack = new() { Nuts = nuts, Boardstate = this };
                Stacks.Add(stack);
                for (byte nutNr = 0; nutNr < stack.Nuts.Count; nutNr++)
                {
                    stack.Nuts[nutNr].Positions.Add(new() { StackNr = stackNr, Level = nutNr });
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

        private void UpdatePossibleMoves()
        {
            for (byte fromStackNr = 0; fromStackNr < Stacks.Count; fromStackNr++)
            {
                Stack fromStack = Stacks[fromStackNr];
                for (byte toStackNr = 0; toStackNr < Stacks.Count; toStackNr++)
                {
                    Stack toStack = Stacks[toStackNr];
                    if ((toStack.IsEmpty || fromStack.TopNut?.NutColor.Name == toStack.TopNut?.NutColor.Name) &&
                        fromStackNr != toStackNr && !toStack.IsFull && !fromStack.IsFinished &&
                        (!fromStack.IsMonochromatic || fromStack.TopNutCount < fromStack.Nuts.Count || !toStack.IsEmpty))
                    {
                        bool foundReverseMove = false;
                        int fromStackLevel = fromStack.Nuts.Count;
                        int toStackLevel = toStack.Nuts.Count + 1;
                        List<Position> positions = fromStack.TopNut?.Positions ?? [];
                        for (int positionNr = 1; positionNr < positions.Count; positionNr++)
                        {
                            if (fromStackNr == positions[positionNr].StackNr && toStackNr == positions[positionNr - 1].StackNr && fromStackLevel == positions[positionNr].Level && toStackLevel == positions[positionNr - 1].Level)
                            {
                                foundReverseMove = true;
                                break;
                            }
                        }
                        if (!foundReverseMove)
                        {
                            PossibleMoves.Add(new() { FromStackNr = fromStackNr, ToStackNr = toStackNr });
                        }
                    }
                }
            }
            PrioritizeMoves();
        }

        private void PrioritizeMoves()
        {
            for (int moveNr1 = 0; moveNr1 < PossibleMoves.Count - 1; moveNr1++)
            {
                for (int moveNr2 = moveNr1 + 1; moveNr2 < PossibleMoves.Count; moveNr2++)
                {
                    Stack fromStack1 = Stacks[PossibleMoves[moveNr1].FromStackNr];
                    Stack toStack1 = Stacks[PossibleMoves[moveNr1].ToStackNr];
                    Stack fromStack2 = Stacks[PossibleMoves[moveNr2].FromStackNr];
                    Stack toStack2 = Stacks[PossibleMoves[moveNr2].ToStackNr];
                    bool isEmpty1 = toStack1.IsEmpty;
                    bool isEmpty2 = toStack2.IsEmpty;
                    if (isEmpty1 && !isEmpty2)
                    {
                        (PossibleMoves[moveNr1], PossibleMoves[moveNr2]) = (PossibleMoves[moveNr2], PossibleMoves[moveNr1]);
                    }
                    else if (isEmpty1 == !isEmpty2)
                    {
                        int fromStackHeight1 = fromStack1.Nuts.Count;
                        int fromStackHeight2 = fromStack2.Nuts.Count;
                        if (fromStackHeight1 > 1 && fromStackHeight2 == 1)
                        {
                            (PossibleMoves[moveNr1], PossibleMoves[moveNr2]) = (PossibleMoves[moveNr2], PossibleMoves[moveNr1]);
                        }
                        else if ((fromStackHeight1 == fromStackHeight2) || fromStackHeight1 > 1)
                        {
                            int topNutsCountByColor1 = GetTopNutsCountByColor(fromStack1.TopNut?.NutColor ?? null);
                            int topNutsCountByColor2 = GetTopNutsCountByColor(fromStack2.TopNut?.NutColor ?? null);
                            if (topNutsCountByColor1 < topNutsCountByColor2)
                            {
                                (PossibleMoves[moveNr1], PossibleMoves[moveNr2]) = (PossibleMoves[moveNr2], PossibleMoves[moveNr1]);
                            }
                            else if (topNutsCountByColor1 == topNutsCountByColor2)
                            {
                                if (fromStack1.TopNutCount < fromStack2.TopNutCount)
                                {
                                    (PossibleMoves[moveNr1], PossibleMoves[moveNr2]) = (PossibleMoves[moveNr2], PossibleMoves[moveNr1]);
                                }
                                else if (fromStack1.TopNutCount == fromStack2.TopNutCount)
                                {
                                    bool isMonochromatic1 = toStack1.IsMonochromatic;
                                    bool isMonochromatic2 = toStack2.IsMonochromatic;
                                    if (!isMonochromatic1 && isMonochromatic2)
                                    {
                                        (PossibleMoves[moveNr1], PossibleMoves[moveNr2]) = (PossibleMoves[moveNr2], PossibleMoves[moveNr1]);
                                    }
                                    else if (isMonochromatic1 == isMonochromatic2)
                                    {
                                        if (fromStackHeight1 > fromStackHeight2)
                                        {
                                            (PossibleMoves[moveNr1], PossibleMoves[moveNr2]) = (PossibleMoves[moveNr2], PossibleMoves[moveNr1]);
                                        }
                                        else if (fromStackHeight1 == fromStackHeight2)
                                        {
                                            int toStackHeight1 = toStack1.Nuts.Count;
                                            int toStackHeight2 = toStack2.Nuts.Count;
                                            if (toStackHeight1 < toStackHeight2)
                                            {
                                                (PossibleMoves[moveNr1], PossibleMoves[moveNr2]) = (PossibleMoves[moveNr2], PossibleMoves[moveNr1]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private int GetTopNutsCountByColor(NutColor? nutColor)
        {
            int count = 0;
            if (nutColor is null) { return count; }
            foreach (Stack stack in Stacks)
            {
                if (stack.TopNut is null) { break; }
                else if (stack.TopNut.NutColor.Name == nutColor.Name) { count += stack.TopNutCount; }
            }
            return count;
        }
    }
}

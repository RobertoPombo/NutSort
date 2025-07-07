using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Solution
    {
        public Solution() { }

        public Solution(List<Boardstate> boardstates)
        {
            if (boardstates is not null && boardstates.Count > 0)
            {
                Board = boardstates[0].Solution.Board;
                Dictionary<int, int> newNutIds = [];
                foreach (Stack stack in boardstates[0].Stacks)
                {
                    foreach (Nut nut in stack.Nuts)
                    {
                        Nuts.Add(new(nut.NutColor));
                        newNutIds[nut.Id] = Nuts[^1].Id;
                    }
                }
                foreach (Boardstate boardstate in boardstates)
                {
                    List<Stack> stacks = [];
                    foreach (Stack stack in boardstate.Stacks)
                    {
                        List<Nut> nuts = [];
                        foreach (Nut nut in stack.Nuts)
                        {
                            foreach (Nut newNut in Nuts)
                            {
                                if (newNut.Id == newNutIds[nut.Id])
                                {
                                    nuts.Add(newNut);
                                    break;
                                }
                            }
                        }
                        stacks.Add(new() { Nuts = nuts });
                    }
                    Boardstates.Add(new(stacks));
                    boardstate.Solution = this;
                }
            }
        }

        public Solution(Boardstate initialBoardstate)
        {
            Board = initialBoardstate.Solution.Board;
            List<Stack> stacks = [];
            foreach (Stack stack in initialBoardstate.Stacks)
            {
                List<Nut> nuts = [];
                foreach (Nut nut in stack.Nuts)
                {
                    Nuts.Add(new(nut.NutColor));
                    nuts.Add(new(nut.NutColor));
                }
                stacks.Add(new() { Nuts = nuts });
            }
            Boardstates.Add(new(stacks));
            Boardstates[^1].Solution = this;
        }

        public List<Boardstate> Boardstates { get; set; } = [];
        public List<Nut> Nuts { get; set; } = [];
        public Board Board { get; set; } = new();

        [JsonIgnore] public bool IsFinished
        {
            get
            {
                if (Boardstates.Count == 0) { return false; }
                return Boardstates[^1].IsFinished;
            }
        }
    }
}

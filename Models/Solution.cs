using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Solution
    {
        public Solution() { }

        public Solution(List<Boardstate> boardstates, long iterationCount, int totalProcessDurationSec)
        {
            if (boardstates is not null && boardstates.Count > 0)
            {
                Board = boardstates[0].Solution.Board;
                List<Nut> newNuts = [];
                Dictionary<int, int> newNutIds = [];
                foreach (Stack stack in boardstates[0].Stacks)
                {
                    foreach (Nut nut in stack.Nuts)
                    {
                        Nut newNut = new(nut.NutColor);
                        newNuts.Add(newNut);
                        newNutIds[nut.Id] = newNut.Id;
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
                            foreach (Nut newNut in newNuts)
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
                    Boardstate newBoardstate = new(stacks, this);
                    Boardstates.Add(newBoardstate);
                    newBoardstate.Solution = this;
                }
                IterationCount = iterationCount;
                TotalProcessDurationSec = totalProcessDurationSec;
            }
        }

        public Solution(Boardstate initialBoardstate, Board board)
        {
            Board = board;
            List<Stack> stacks = [];
            foreach (Stack stack in initialBoardstate.Stacks)
            {
                List<Nut> nuts = [];
                foreach (Nut nut in stack.Nuts)
                {
                    nuts.Add(new(nut.NutColor));
                }
                stacks.Add(new() { Nuts = nuts });
            }
            Boardstates.Add(new(stacks, this));
            Boardstates[0].Solution = this;
            //new Thread(Boardstates[0].TryMakeNextMove).Start();
        }

        public List<Boardstate> Boardstates { get; set; } = [];
        public Board Board { get; set; } = new();
        public long IterationCount { get; set; } = 0;
        public int TotalProcessDurationSec { get; set; } = 0;

        [JsonIgnore] public bool IsFinished
        {
            get
            {
                if (Boardstates.Count == 0) { return false; }
                return Boardstates[^1].IsFinished;
            }
        }

        private bool isAllowedToSolve = true;

        public void Solve()
        {
            DateTime startTime = DateTime.Now;
            while (Boardstates.Count > 0 && isAllowedToSolve)
            {
                Boardstate state = Boardstates[^1];
                if (IsFinished)
                {
                    if (Board.ShortestSolution is null || Boardstates.Count < Board.ShortestSolution.Boardstates.Count)
                    {
                        Board.ShortestSolution = this;
                    }
                    Solution newSolution = new(Boardstates, IterationCount, TotalProcessDurationSec);
                    Board.Solutions.Add(newSolution);
                    newSolution.TryExcecuteNextMove(newSolution.Boardstates[^1]);
                    new Thread(newSolution.Solve).Start();
                    isAllowedToSolve = false;
                }
                else
                {
                    TryExcecuteNextMove(state);
                }
                IterationCount++;
                TotalProcessDurationSec = (int)(DateTime.Now - startTime).TotalSeconds;
            }
        }

        public void StopSolving()
        {
            isAllowedToSolve = false;
        }

        public void TryExcecuteNextMove(Boardstate state)
        {
            if (state.NextMoveIndex >= state.PossibleMoves.Count || (Board.ShortestSolution is not null && Boardstates.Count > Board.ShortestSolution.Boardstates.Count))
            {
                if (Boardstates.Count < 2)
                {
                    Board.Solutions.Remove(this);
                    isAllowedToSolve = false;
                }
                DeleteLatestBoardstate();
            }
            else
            {
                ExecuteNextMove(state);
            }
        }

        private void ExecuteNextMove(Boardstate state)
        {
            UndoPreviousMove(state);
            state.PossibleMoves[state.NextMoveIndex].Execute(state);
            state.NextMoveIndex++;
            bool circleFound = false;
            for (int boardstateNr1 = 0; boardstateNr1 < Boardstates.Count - 1; boardstateNr1++)
            {
                if (Boardstates[boardstateNr1].Id == state.Id)
                {
                    for (int boardstateNr2 = boardstateNr1 + 1; boardstateNr2 < Boardstates.Count; boardstateNr2++)
                    {
                        DeleteLatestBoardstate();
                    }
                    circleFound = true;
                    break;
                }
            }
            if (!circleFound) { Boardstates.Add(new(state.Stacks, this)); }
        }

        private void UndoPreviousMove(Boardstate state)
        {
            if (state.NextMoveIndex > 0)
            {
                state.PossibleMoves[state.NextMoveIndex - 1].Undo(state);
            }
        }

        private void DeleteLatestBoardstate()
        {
            if (Boardstates.Count > 0)
            {
                Nut? movedNut = null;
                if (Boardstates.Count == 1)
                {
                    Boardstate? state = Board.InitialBoardstate?.Boardstates[0];
                    //movedNut = state?.Stacks[state.PossibleMoves[Board.Solutions.IndexOf(this)].FromStackNr].TopNut;
                }
                else if (Boardstates[^2].NextMoveIndex > 0)
                {
                    Boardstate state = Boardstates[^2];
                    movedNut = state.Stacks[state.PossibleMoves[state.NextMoveIndex - 1].FromStackNr].TopNut;
                }
                if (movedNut is not null && movedNut.Positions.Count > 0)
                {
                    movedNut.Positions.RemoveAt(movedNut.Positions.Count - 1);
                }
                Boardstates.RemoveAt(Boardstates.Count - 1);
            }
        }
    }
}

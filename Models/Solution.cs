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
                    foreach (Move move in boardstate.PossibleMoves) { newBoardstate.PossibleMoves.Add(move); }
                    newBoardstate.NextMoveIndex = boardstate.NextMoveIndex;
                }
                IterationCount = iterationCount;
                TotalProcessDurationSec = totalProcessDurationSec;
                SolveStartTime = boardstates[0].Solution.SolveStartTime;
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
            Boardstate newBoardstate = new(stacks, this);
            Boardstates.Add(newBoardstate);
            newBoardstate.Solution = this;
            foreach (Move move in initialBoardstate.PossibleMoves) { newBoardstate.PossibleMoves.Add(move); }
            newBoardstate.NextMoveIndex = initialBoardstate.NextMoveIndex;
        }

        public List<Boardstate> Boardstates { get; set; } = [];
        public Board Board { get; set; } = new();
        public long IterationCount { get; set; } = 0;
        public DateTime SolveStartTime { get; set; } = DateTime.Now;
        public int TotalProcessDurationSec { get; set; } = 0;

        [JsonIgnore] public bool IsFinished
        {
            get
            {
                if (Boardstates.Count == 0) { return false; }
                return Boardstates[^1].IsFinished;
            }
        }

        [JsonIgnore] public double Progress
        {
            get
            {
                double progress = 0;
                if (Boardstates.Count == 0) { return 0; }
                if (IsFinished) { return 1; }
                List<(double, double)> progressBoardstates = [];
                for (int boardstateNr = Boardstates.Count - 1; boardstateNr >= 0; boardstateNr--)
                {
                    if (boardstateNr < Boardstates.Count)
                    {
                        int moveIndex = Boardstates[boardstateNr].NextMoveIndex - 1;
                        int possibleMovesCount = Boardstates[boardstateNr].PossibleMoves.Count;
                        progressBoardstates.Add((possibleMovesCount, moveIndex));
                    }
                }
                double factor = 1;
                for (int boardstateNr = Boardstates.Count - 1; boardstateNr >= 0; boardstateNr--)
                {
                    if (progressBoardstates[boardstateNr].Item1 > 0 && progressBoardstates[boardstateNr].Item2 >= 0)
                    {
                        double progressBoardstate = progressBoardstates[boardstateNr].Item2 / progressBoardstates[boardstateNr].Item1;
                        progress += progressBoardstate * factor;
                        factor = factor / progressBoardstates[boardstateNr].Item1;
                    }
                }
                return progress;
            }
        }

        [JsonIgnore] public bool IsSolving { get; set; } = false;

        [JsonIgnore] private bool isAllowedToSolve = true;

        public void Solve()
        {
            while (Boardstates.Count > 0 && isAllowedToSolve)
            {
                IsSolving = true;
                Boardstate state = Boardstates[^1];
                if (IsFinished)
                {
                    if (Board.ShortestSolution is null || Boardstates.Count < Board.ShortestSolution.Boardstates.Count) { Board.ShortestSolution = this; }
                    if (Board.MostObviousSolution is null || IterationCount < Board.MostObviousSolution.IterationCount) { Board.MostObviousSolution = this; }
                    Solution newSolution = new(Boardstates, IterationCount, TotalProcessDurationSec);
                    Board.Solutions.Add(newSolution);
                    newSolution.TryExcecuteNextMove(newSolution.Boardstates[^1]);
                    if (isAllowedToSolve) { new Thread(newSolution.Solve).Start(); }
                    isAllowedToSolve = false;
                }
                else
                {
                    TryExcecuteNextMove(state);
                }
                IterationCount++;
                TotalProcessDurationSec = (int)(DateTime.Now - SolveStartTime).TotalSeconds;
            }
            IsSolving = false;
        }

        public void StopSolving()
        {
            isAllowedToSolve = false;
        }

        public void TryExcecuteNextMove(Boardstate state)
        {
            if (state.NextMoveIndex >= state.PossibleMoves.Count || (Board.ShortestSolution is not null && Board.ShortestSolution.Boardstates.Count <= Boardstates.Count + state.MinimumRequiredMovesCount))
            {
                DeleteLatestBoardstate();
                if (Boardstates.Count < 2)
                {
                    Board.Solutions.Remove(this);
                    isAllowedToSolve = false;
                }
            }
            else
            {
                ExecuteNextMove(state);
            }
        }

        private void ExecuteNextMove(Boardstate state0)
        {
            Boardstate state1 = new(state0.Stacks, this);
            Boardstates.Add(state1);
            state0.PossibleMoves[state0.NextMoveIndex].Execute(state1);
            state1.UpdatePossibleMoves();
            state0.NextMoveIndex++;
            for (int boardstateNr1 = 0; boardstateNr1 < Boardstates.Count - 1; boardstateNr1++)
            {
                if (Boardstates[boardstateNr1].Id == state1.Id)
                {
                    for (int boardstateNr2 = boardstateNr1 + 1; boardstateNr2 < Boardstates.Count; boardstateNr2++)
                    {
                        DeleteLatestBoardstate();
                    }
                    break;
                }
            }
        }

        private void DeleteLatestBoardstate()
        {
            Boardstate state = Boardstates[^2];
            Nut? movedNut = state.Stacks[state.PossibleMoves[state.NextMoveIndex - 1].FromStackNr].TopNut;
            Boardstates.RemoveAt(Boardstates.Count - 1);
        }

        public override string ToString()
        {
            string description = Board.ToString() + " - ";
            if (Board.Solutions.Contains(this)) { description += "Solution-Nr " + Board.Solutions.IndexOf(this).ToString(); }
            else if (Board.PlayerSolution == this) { description += "player solution"; }
            else if (Board.InitialBoardstate == this) { description += "initial boardstate"; }
            return description;
        }
    }
}

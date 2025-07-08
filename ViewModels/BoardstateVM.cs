using System;
using System.Collections.ObjectModel;

using GTRC_WPF;
using NutSort.Models;

namespace NutSort.ViewModels
{
    public class BoardstateVM : ObservableObject
    {
        public BoardstateVM()
        {
            Stacks = [];
            if (Board.List.Count > 0) { Board = Board.List[0]; }
            SolutionNr = -1;
            PreviousSolutionCmd = new UICmd((o) => PreviousSolution());
            NextSolutionCmd = new UICmd((o) => NextSolution());
            PreviousStepCmd = new UICmd((o) => PreviousStep());
            NextStepCmd = new UICmd((o) => NextStep());
            RaisePropertyChanged(nameof(Stacks));
        }

        private Board board = new();
        private ObservableCollection<Stack> stacks = [];
        private Solution? solution = null;
        private Boardstate? boardstate = null;
        private int solutionNr = 0;
        private int stepNr = 0;
        private long iterationCount = 0;
        private int totalProcessDurationSec = 0;

        public Board Board
        {
            get { return board; }
            set { board = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(SolutionCount)); }
        }

        public ObservableCollection<Stack> Stacks
        {
            get { return stacks; }
            set { stacks = value; RaisePropertyChanged(); }
        }

        public int SolutionCount
        {
            get { return board.Solutions.Count; }
        }

        public int StepCount
        {
            get
            {
                if (solutionNr < 0) { return 1; }
                return board.Solutions[solutionNr].Boardstates.Count;
            }
        }

        public int SolutionNr
        {
            get { return solutionNr; }
            set
            {
                solutionNr = value;
                if (solutionNr >= SolutionCount) { solutionNr = SolutionCount - 1; }
                if (solutionNr < 0) { solutionNr = -1; solution = Board.InitialBoardstate; }
                else { solution = Board.Solutions[SolutionNr]; }
                if (solution is null || solution.Boardstates.Count == 0) { stepNr = -1; }
                else if (stepNr >= solution.Boardstates.Count) { stepNr = solution.Boardstates.Count - 1; }
                boardstate = solution?.Boardstates[stepNr] ?? null;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(StepNr));
                LoadBoardstate();
            }
        }

        public int StepNr
        {
            get { return stepNr; }
            set
            {
                stepNr = value;
                if (stepNr < 0) { stepNr = 0; }
                if (stepNr >= StepCount) { stepNr = StepCount - 1; }
                boardstate = null;
                if (solution is not null && stepNr < solution.Boardstates.Count) { boardstate = solution.Boardstates[stepNr]; }
                RaisePropertyChanged();
                LoadBoardstate();
            }
        }

        public long IterationCount
        {
            get { return solution?.IterationCount ?? -1; }
        }

        public int TotalProcessDurationSec
        {
            get { return solution?.TotalProcessDurationSec ?? -1; }
        }

        public string State
        {
            get
            {
                if (solutionNr < 0)
                {
                    if (board.IsFinished) { return "(finished)"; }
                    else if (board.ShortestSolution is not null) { return "(solution found)"; }
                    else { return "(ongoing)"; }
                }
                else if (solution is null) { return string.Empty; }
                else if (solution.IsFinished) { return "(finished)"; }
                else { return "(ongoing)"; }
            }
        }

        public string GlobalState
        {
            get
            {
                int runningCount = 0;
                int finishedCount = 0;
                foreach (Solution _solution in Board.Solutions)
                {
                    if (_solution.IsFinished) { finishedCount++; }
                    else { runningCount++; }
        }
                return "Running: " + runningCount.ToString() + " - Finished: " + finishedCount.ToString();
            }
        }

        private void LoadBoardstate()
        {
            RaisePropertyChanged(nameof(SolutionCount));
            RaisePropertyChanged(nameof(StepCount));
            RaisePropertyChanged(nameof(IterationCount));
            RaisePropertyChanged(nameof(TotalProcessDurationSec));
            RaisePropertyChanged(nameof(State));
            RaisePropertyChanged(nameof(GlobalState));
            Stacks = [];
            if (boardstate is not null)
            {
                foreach (Stack stack in boardstate.Stacks)
                {
                    Stacks.Add(stack);
                }
            }
            RaisePropertyChanged(nameof(Stacks));
        }

        private void PreviousSolution()
        {
            SolutionNr--;
        }

        private void NextSolution()
        {
            SolutionNr++;
        }

        private void PreviousStep()
        {
            StepNr--;
        }

        private void NextStep()
        {
            StepNr++;
        }

        public UICmd PreviousSolutionCmd { get; set; }
        public UICmd NextSolutionCmd { get; set; }
        public UICmd PreviousStepCmd { get; set; }
        public UICmd NextStepCmd { get; set; }
    }
}

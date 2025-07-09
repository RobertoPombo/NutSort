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
            BoardstateRows = [];
            if (Board.List.Count > 0) { Board = Board.List[0]; }
            SolutionNr = -1;
            PreviousSolutionCmd = new UICmd((o) => PreviousSolution());
            NextSolutionCmd = new UICmd((o) => NextSolution());
            InitialBoardstateCmd = new UICmd((o) => InitialBoardstate());
            ShortestSolutionCmd = new UICmd((o) => ShortestSolution());
            PreviousStepCmd = new UICmd((o) => PreviousStep());
            NextStepCmd = new UICmd((o) => NextStep());
            FirstStepCmd = new UICmd((o) => FirstStep());
            LastStepCmd = new UICmd((o) => LastStep());
            PlayCmd = new UICmd((o) => PlayAnimation());
            StopCmd = new UICmd((o) => StopAnimation());
            ReverseCmd = new UICmd((o) => ReverseAnimation());
            ResetCmd = new UICmd((o) => ResetAnimation());
            RaisePropertyChanged(nameof(BoardstateRows));
        }

        private Board board = new();
        private ObservableCollection<BoardstateRow> boardstateRows = [];
        private Solution? solution = null;
        private Boardstate? boardstate = null;
        private int solutionNr = 0;
        private int stepNr = 0;
        private int animationDelayMs = 750;
        private bool animationIsReversed = false;
        private bool animationIsSleeping = false;

        public Board Board
        {
            get { return board; }
            set { board = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(SolutionCount)); }
        }

        public ObservableCollection<BoardstateRow> BoardstateRows
        {
            get { return boardstateRows; }
            set { boardstateRows = value; RaisePropertyChanged(); }
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

        public int AnimationDelayMs
        {
            get { return animationDelayMs; }
            set
            {
                animationDelayMs = value;
                if (animationDelayMs < 0) { animationDelayMs = 0; }
                if (animationDelayMs > 9999) { animationDelayMs = 9999; }
                RaisePropertyChanged();
            }
        }

        public bool AnimationIsRunning { get; set; } = false;

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

        public int RunningCount
        {
            get
            {
                int count = 0;
                foreach (Solution _solution in Board.Solutions) { if (!_solution.IsFinished) { count++; } }
                return count;
            }
        }

        public int FinishedCount
        {
            get
            {
                int count = 0;
                foreach (Solution _solution in Board.Solutions) { if (_solution.IsFinished) { count++; } }
                return count;
            }
        }

        private void LoadBoardstate()
        {
            RaisePropertyChanged(nameof(SolutionCount));
            RaisePropertyChanged(nameof(StepCount));
            RaisePropertyChanged(nameof(IterationCount));
            RaisePropertyChanged(nameof(TotalProcessDurationSec));
            RaisePropertyChanged(nameof(State));
            RaisePropertyChanged(nameof(RunningCount));
            RaisePropertyChanged(nameof(FinishedCount));
            boardstateRows = [];
            if (boardstate is not null)
            {
                List<Stack> stacks = [];
                for (int stackNr = 0; stackNr < boardstate.Stacks.Count; stackNr++)
                {
                    stacks.Add(boardstate.Stacks[stackNr]);
                    if (stacks.Count >= board.MaxColumnsCount)
                    {
                        boardstateRows.Add(new() { Stacks = stacks });
                        stacks = [];
                    }
                }
                if (stacks.Count > 0) { boardstateRows.Add(new() { Stacks = stacks }); }
            }
            RaisePropertyChanged(nameof(BoardstateRows));
        }

        private void PreviousSolution()
        {
            SolutionNr--;
        }

        private void NextSolution()
        {
            SolutionNr++;
        }

        private void InitialBoardstate()
        {
            SolutionNr = -1;
        }

        private void ShortestSolution()
        {
            if (Board.ShortestSolution is null) { SolutionNr = -1; }
            else { SolutionNr = Board.Solutions.IndexOf(Board.ShortestSolution); }
        }

        private void PreviousStep()
        {
            StepNr--;
        }

        private void NextStep()
        {
            StepNr++;
        }

        private void FirstStep()
        {
            StepNr = 0;
        }

        private void LastStep()
        {
            StepNr = StepCount - 1;
        }

        private void PlayAnimation()
        {
            animationIsReversed = false;
            if (!AnimationIsRunning)
            {
                while (animationIsSleeping) { Thread.Sleep(10); }
                AnimationIsRunning = true;
                new Thread(PlayAnimationThread).Start();
            }
        }

        private void StopAnimation()
        {
            AnimationIsRunning = false;
        }

        private void ReverseAnimation()
        {
            animationIsReversed = true;
            if (!AnimationIsRunning)
            {
                while (animationIsSleeping) { Thread.Sleep(10); }
                AnimationIsRunning = true;
                new Thread(PlayAnimationThread).Start();
            }
        }

        private void ResetAnimation()
        {
            StepNr = 0;
        }

        private void PlayAnimationThread()
        {
            while (StepNr < StepCount && AnimationIsRunning)
            {
                if (animationIsReversed) { StepNr--; }
                else { StepNr++; }
                animationIsSleeping = true;
                Thread.Sleep(animationDelayMs);
                animationIsSleeping = false;
            }
            AnimationIsRunning = false;
        }

        public UICmd PreviousSolutionCmd { get; set; }
        public UICmd NextSolutionCmd { get; set; }
        public UICmd InitialBoardstateCmd { get; set; }
        public UICmd ShortestSolutionCmd { get; set; }
        public UICmd PreviousStepCmd { get; set; }
        public UICmd NextStepCmd { get; set; }
        public UICmd FirstStepCmd { get; set; }
        public UICmd LastStepCmd { get; set; }
        public UICmd PlayCmd { get; set; }
        public UICmd StopCmd { get; set; }
        public UICmd ReverseCmd { get; set; }
        public UICmd ResetCmd { get; set; }
    }
}

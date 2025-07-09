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
            Boards = [];
            foreach (Board _board in Board.List) { Boards.Add(_board); }
            if (boards.Count > 0) { Board = boards[0]; }
            BoardstateRows = [];
            SolutionNr = -1;
            PlayBoardCmd = new UICmd((o) => PlayBoard());
            SolveBoardCmd = new UICmd((o) => SolveBoard());
            EditBoardCmd = new UICmd((o) => EditBoard());
            RandomizeBoardCmd = new UICmd((o) => RandomizeBoard());
            CreateNewBoardCmd = new UICmd((o) => CreateNewBoard());
            SaveBoardCmd = new UICmd((o) => SaveBoard());
            PreviousSolutionCmd = new UICmd((o) => PreviousSolution());
            NextSolutionCmd = new UICmd((o) => NextSolution());
            InitialBoardstateCmd = new UICmd((o) => InitialBoardstate());
            ShortestSolutionCmd = new UICmd((o) => ShortestSolution());
            PreviousStepCmd = new UICmd((o) => PreviousStep());
            NextStepCmd = new UICmd((o) => NextStep());
            FirstStepCmd = new UICmd((o) => FirstStep());
            LastStepCmd = new UICmd((o) => LastStep());
            PlayAnimationCmd = new UICmd((o) => PlayAnimation());
            StopAnimationCmd = new UICmd((o) => StopAnimation());
            ReverseAnimationCmd = new UICmd((o) => ReverseAnimation());
            ResetAnimationCmd = new UICmd((o) => ResetAnimation());
        }

        private ObservableCollection<Board> boards = [];
        private Board board = new();
        private ObservableCollection<BoardstateRow> boardstateRows = [];
        private Solution? solution = null;
        private Boardstate? boardstate = null;
        private int solutionNr = 0;
        private int stepNr = 0;
        private int animationDelayMs = 750;
        private bool animationIsReversed = false;
        private bool animationIsSleeping = false;

        public ObservableCollection<Board> Boards
        {
            get { return boards; }
            set { boards = value; }
        }

        public Board Board
        {
            get { return board; }
            set
            {
                board = value;
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        public ObservableCollection<BoardstateRow> BoardstateRows
        {
            get { return boardstateRows; }
            set { boardstateRows = value; }
        }

        public byte StackCount
        {
            get { return board.StackCount; }
            set
            {
                board.StackCount = value;
                if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        public byte StackHeight
        {
            get { return board.StackHeight; }
            set
            {
                board.StackHeight = value;
                if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        public uint NutCount
        {
            get { return (uint)board.NutSameColorCount * board.ColorCount; }
            set
            {
                board.NutSameColorCount = (byte)Math.Round((double)value / board.ColorCount, 0);
                if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        public byte ColorCount
        {
            get { return board.ColorCount; }
            set
            {
                board.ColorCount = value;
                if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        public string BoardColorCode
        {
            get { return boardstate?.Id ?? string.Empty; }
            set
            {
                board.CreateInitialBoardstate(value);
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        public byte MaxColumnsCount
        {
            get { return board.MaxColumnsCount; }
            set
            {
                board.MaxColumnsCount = value;
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
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
                    if (board.IsFinished) { return "Finished"; }
                    else if (board.ShortestSolution is not null) { return "Solution found"; }
                    else { return "Ongoing"; }
                }
                else if (solution is null) { return string.Empty; }
                else if (solution.IsFinished) { return "Finished"; }
                else { return "Ongoing"; }
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
            RaisePropertyChanged(nameof(Boards));
            RaisePropertyChanged(nameof(Board));
            RaisePropertyChanged(nameof(StackCount));
            RaisePropertyChanged(nameof(StackHeight));
            RaisePropertyChanged(nameof(NutCount));
            RaisePropertyChanged(nameof(ColorCount));
            RaisePropertyChanged(nameof(BoardColorCode));
            RaisePropertyChanged(nameof(MaxColumnsCount));
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

        private void PlayBoard()
        {

        }

        private void SolveBoard()
        {
            board.Solve();
        }

        private void EditBoard()
        {

        }

        private void RandomizeBoard()
        {

        }

        private void CreateNewBoard()
        {

        }

        private void SaveBoard()
        {

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

        public UICmd PlayBoardCmd { get; set; }
        public UICmd SolveBoardCmd { get; set; }
        public UICmd EditBoardCmd { get; set; }
        public UICmd RandomizeBoardCmd { get; set; }
        public UICmd CreateNewBoardCmd { get; set; }
        public UICmd SaveBoardCmd { get; set; }
        public UICmd PreviousSolutionCmd { get; set; }
        public UICmd NextSolutionCmd { get; set; }
        public UICmd InitialBoardstateCmd { get; set; }
        public UICmd ShortestSolutionCmd { get; set; }
        public UICmd PreviousStepCmd { get; set; }
        public UICmd NextStepCmd { get; set; }
        public UICmd FirstStepCmd { get; set; }
        public UICmd LastStepCmd { get; set; }
        public UICmd PlayAnimationCmd { get; set; }
        public UICmd StopAnimationCmd { get; set; }
        public UICmd ReverseAnimationCmd { get; set; }
        public UICmd ResetAnimationCmd { get; set; }
    }
}

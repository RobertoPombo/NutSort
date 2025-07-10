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
            InitialBoardstate();
            PlayBoardCmd = new UICmd((o) => PlayBoard());
            SelectStackCmd = new UICmd((o) => SelectStack(o));
            SolveBoardCmd = new UICmd((o) => SolveBoard());
            LoadBoardCmd = new UICmd((o) => LoadBoard());
            EditBoardCmd = new UICmd((o) => EditBoard());
            RandomizeBoardCmd = new UICmd((o) => RandomizeBoard());
            CreateNewBoardCmd = new UICmd((o) => CreateNewBoard());
            DeleteBoardCmd = new UICmd((o) => DeleteBoard());
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
        private Board? board = new();
        private ObservableCollection<BoardstateRow> boardstateRows = [];
        private Solution? solution = null;
        private Boardstate? boardstate = null;
        private int solutionNr = 0;
        private int stepNr = 0;
        private int animationDelayMs = 1000;
        private bool animationIsReversed = false;
        private bool animationIsSleeping = false;
        private Move? move = null;

        public ObservableCollection<Board> Boards
        {
            get { return boards; }
            set { boards = value; }
        }

        public Board? Board
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
            get { return board?.StackCount ?? byte.MinValue; }
            set
            {
                if (board is not null)
                {
                    board.StackCount = value;
                    if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                    board.ResetSolutions();
                    InitialBoardstate();
                    FirstStep();
                    LoadBoardstate();
                }
            }
        }

        public byte StackHeight
        {
            get { return board?.StackHeight ?? byte.MinValue; }
            set
            {
                if (board is not null)
                {
                    board.StackHeight = value;
                    if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                    board.ResetSolutions();
                    InitialBoardstate();
                    FirstStep();
                    LoadBoardstate();
                }
            }
        }

        public uint NutCount
        {
            get { return (uint)(board?.NutSameColorCount ?? uint.MinValue) * (board?.ColorCount ?? uint.MinValue); }
            set
            {
                if (board is not null)
                {
                    board.NutSameColorCount = (byte)Math.Round((double)value / board.ColorCount, 0);
                    if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                    board.ResetSolutions();
                    InitialBoardstate();
                    FirstStep();
                    LoadBoardstate();
                }
            }
        }

        public byte ColorCount
        {
            get { return board?.ColorCount ?? byte.MinValue; }
            set
            {
                if (board is not null)
                {
                    board.ColorCount = value;
                    board.ResetSolutions();
                    if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                    InitialBoardstate();
                    FirstStep();
                    LoadBoardstate();
                }
            }
        }

        public string BoardColorCode
        {
            get { return boardstate?.Id ?? string.Empty; }
            set
            {
                if (board is not null)
                {
                    board.ResetSolutions();
                    board.CreateInitialBoardstate(value);
                    InitialBoardstate();
                    FirstStep();
                    LoadBoardstate();
                }
            }
        }

        public byte MaxColumnsCount
        {
            get { return board?.MaxColumnsCount ?? byte.MinValue; }
            set
            {
                if (board is not null)
                {
                    board.MaxColumnsCount = value;
                    InitialBoardstate();
                    FirstStep();
                    LoadBoardstate();
                }
            }
        }

        public int SolutionCount
        {
            get { return board?.Solutions.Count ?? 0; }
        }

        public int StepCount
        {
            get
            {
                if (solutionNr < 0) { return 1; }
                return board?.Solutions[solutionNr].Boardstates.Count ?? 0;
            }
        }

        public int SolutionNr
        {
            get { return solutionNr; }
            set
            {
                solutionNr = value;
                if (solutionNr >= SolutionCount) { solutionNr = SolutionCount - 1; }
                if (solutionNr < 0) { solutionNr = -1; solution = Board?.InitialBoardstate ?? null; }
                else { solution = Board?.Solutions[SolutionNr] ?? null; }
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
                if (board is null) { return string.Empty; }
                if (board.Solutions.Count == 0)
                {
                    if (board.InitialBoardstate is not null && board.InitialBoardstate.Boardstates.Count > 0 && board.InitialBoardstate.Boardstates[0].NextMoveIndex > 0)
                    {
                        return "Not solvable";
                    }
                    else { return "Not started"; }
                }
                else if (solutionNr < 0)
                {
                    if (board.IsFinished) { return "Board finished"; }
                    else if (board.ShortestSolution is not null) { return "Solutions found"; }
                    else { return "No solution found yet"; }
                }
                else if (solution is null) { return string.Empty; }
                else if (solution.IsFinished) { return "Solution finished"; }
                else { return "Solution is running"; }
            }
        }

        public int RunningCount
        {
            get
            {
                int count = 0;
                if (Board is not null)
                {
                    for (int solutionNr = Board.Solutions.Count - 1; solutionNr >= 0; solutionNr--)
                    {
                        if (solutionNr < Board.Solutions.Count && !Board.Solutions[solutionNr].IsFinished) { count++; }
                    }
                }
                return count;
            }
        }

        public int FinishedCount
        {
            get
            {
                int count = 0;
                if (Board is not null)
                {
                    for (int solutionNr = Board.Solutions.Count - 1; solutionNr >= 0; solutionNr--)
                    {
                        if (solutionNr < Board.Solutions.Count && Board.Solutions[solutionNr].IsFinished) { count++; }
                    }
                }
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
                    if (stacks.Count >= board?.MaxColumnsCount)
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
            if (board is not null && boardstate is not null)
            {
                move = new();
                board.StopSolving();
                solution = new(boardstate, board);
                board.Solutions.Insert(0, solution);
                solution.SolveStartTime = DateTime.Now;
                solution.Boardstates[0].NextMoveIndex = 0;
                SolutionNr = -1;
                SolutionNr = 0;
            }
        }

        private void SelectStack(object obj)
        {
            if (obj.GetType() == typeof(Stack) && boardstate is not null && solution is not null && move is not null)
            {
                Stack stack = (Stack)obj;
                int stackNr = boardstate.Stacks.IndexOf(stack);
                if (move.FromStackNr < 0 && boardstate.Stacks[stackNr].TopNut is not null)
                {
                    move.FromStackNr = stackNr;
                    for (int nutNr = 1; nutNr < boardstate.Stacks[stackNr].TopNutCount + 1; nutNr++)
                    {
                        NutColor color = boardstate.Stacks[stackNr].Nuts[^nutNr].NutColor;
                        boardstate.Stacks[stackNr].Nuts[^nutNr].NutColor = new((byte)Math.Floor((double)byte.MaxValue / 2)) { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
                    }
                }
                else if (move.FromStackNr >= 0 && boardstate.Stacks[move.FromStackNr].TopNut is not null)
                {
                    if (stackNr == move.FromStackNr)
                    {
                        for (int nutNr = 1; nutNr < boardstate.Stacks[stackNr].TopNutCount + 1; nutNr++)
                        {
                            NutColor color = boardstate.Stacks[stackNr].Nuts[^nutNr].NutColor;
                            boardstate.Stacks[stackNr].Nuts[^nutNr].NutColor = NutColor.GetByName(color.Name) ?? new() { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
                        }
                        move = new();
                    }
                    else
                    {
                        move.ToStackNr = stackNr;
                        if (move.IsPossible(boardstate))
                        {
                            for (int nutNr = 1; nutNr < boardstate.Stacks[move.FromStackNr].TopNutCount + 1; nutNr++)
                            {
                                NutColor color = boardstate.Stacks[move.FromStackNr].Nuts[^nutNr].NutColor;
                                boardstate.Stacks[move.FromStackNr].Nuts[^nutNr].NutColor = NutColor.GetByName(color.Name) ?? new() { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
                            }
                            boardstate = new(boardstate.Stacks, boardstate.Solution);
                            solution.Boardstates.Add(boardstate);
                            move.Execute(boardstate);
                            move = new();
                            LastStep();
                        }
                    }
                }
                LoadBoardstate();
            }
        }

        private void SolveBoard()
        {
            move = null;
            board?.Solve();
        }

        private void LoadBoard()
        {
            Board.List = [];
            NutColor.LoadJson();
            Board.LoadJson();
            Boards = [];
            foreach (Board _board in Board.List) { Boards.Add(_board); }
            RaisePropertyChanged(nameof(Boards));
            if (boards.Count > 0) { Board = boards[0]; }
        }

        private void EditBoard()
        {

        }

        private void RandomizeBoard()
        {
            if (board is not null)
            {
                board.CreateInitialBoardstate();
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        private void CreateNewBoard()
        {
            board ??= new();
            string newInitialBoardstate = boardstate?.Id ?? string.Empty;
            Board = new(board.StackCount, board.StackHeight, board.NutSameColorCount, board.ColorCount, board.MaxColumnsCount);
            Board.CreateInitialBoardstate(newInitialBoardstate);
            Boards.Add(board);
            InitialBoardstate();
            FirstStep();
            LoadBoardstate();
        }

        private void DeleteBoard()
        {
            if (board is not null && Board.List.Count > 0)
            {
                Board.List.Remove(board);
                Boards = [];
                foreach (Board _board in Board.List) { Boards.Add(_board); }
                RaisePropertyChanged(nameof(Boards));
                if (boards.Count > 0) { Board = boards[0]; }
                else { Board = null; }
            }
        }

        private void SaveBoard()
        {
            Board.SaveJson();
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
            if (Board?.ShortestSolution is null) { SolutionNr = -1; }
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
            AnimationIsRunning = false;
            StepNr = 0;
        }

        private void PlayAnimationThread()
        {
            while (((!animationIsReversed && StepNr < StepCount - 1) || (animationIsReversed && StepNr > 0)) && AnimationIsRunning)
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
        public UICmd SelectStackCmd { get; set; }
        public UICmd SolveBoardCmd { get; set; }
        public UICmd LoadBoardCmd { get; set; }
        public UICmd EditBoardCmd { get; set; }
        public UICmd RandomizeBoardCmd { get; set; }
        public UICmd CreateNewBoardCmd { get; set; }
        public UICmd DeleteBoardCmd { get; set; }
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

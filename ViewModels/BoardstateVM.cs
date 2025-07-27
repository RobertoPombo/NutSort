using System;
using System.Collections.ObjectModel;
using System.Windows;

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
            SelectNutCmd = new UICmd((o) => SelectNut(o));
            SelectNutColorCmd = new UICmd((o) => SelectNutColor(o));
            RandomizeBoardCmd = new UICmd((o) => RandomizeBoard());
            CreateNewBoardCmd = new UICmd((o) => CreateNewBoard());
            DeleteBoardCmd = new UICmd((o) => DeleteBoard());
            SaveBoardCmd = new UICmd((o) => SaveBoard());
            PreviousSolutionCmd = new UICmd((o) => PreviousSolution());
            NextSolutionCmd = new UICmd((o) => NextSolution());
            FirstSolutionCmd = new UICmd((o) => FirstSolution());
            LastSolutionCmd = new UICmd((o) => LastSolution());
            InitialBoardstateCmd = new UICmd((o) => InitialBoardstate());
            PlayerSolutionCmd = new UICmd((o) => PlayerSolution());
            ShortestSolutionCmd = new UICmd((o) => ShortestSolution());
            MostObviousSolutionCmd = new UICmd((o) => MostObviousSolution());
            PreviousStepCmd = new UICmd((o) => PreviousStep());
            NextStepCmd = new UICmd((o) => NextStep());
            FirstStepCmd = new UICmd((o) => FirstStep());
            LastStepCmd = new UICmd((o) => LastStep());
            PlayAnimationCmd = new UICmd((o) => PlayAnimation());
            StopAnimationCmd = new UICmd((o) => StopAnimation());
            ReverseAnimationCmd = new UICmd((o) => ReverseAnimation());
            ResetAnimationCmd = new UICmd((o) => ResetAnimation());
            PreviousLevelCmd = new UICmd((o) => PreviousLevel());
            NextLevelCmd = new UICmd((o) => NextLevel());
            ShowHideTopMenuCmd = new UICmd((o) => ShowHideTopMenu());
            ShowHideBottomMenuCmd = new UICmd((o) => ShowHideBottomMenu());
            PlayBoard();
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
        private Nut? selectedNut = null;
        private bool isPlaying = false;
        private bool isEditableBoard = false;

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
                if (actualNutCount < NutCount)
                {
                    IsEditableBoard = false;
                    VisibilityNutColorMenu = Visibility.Visible;
                    RaisePropertyChanged(nameof(VisibilityNutColorMenu));
                    RaisePropertyChanged(nameof(NutColors));
                }
            }
        }

        public ObservableCollection<BoardstateRow> BoardstateRows
        {
            get { return boardstateRows; }
            set { boardstateRows = value; }
        }

        public ObservableCollection<NutColor> NutColors
        {
            get
            {
                ObservableCollection<NutColor> list = [];
                foreach (NutColor nutColor in NutColor.List) { list.Add(nutColor); }
                if (board is not null && actualNutCount < NutCount)
                {
                    Dictionary<NutColor, int> nutColorsCount = board.ForceColorCount();
                    for (int nutColorNr = list.Count - 1; nutColorNr >= 0; nutColorNr--)
                    {
                        if (nutColorsCount.TryGetValue(list[nutColorNr], out int value))
                        {
                            if (value >= board.NutSameColorCount) { list.RemoveAt(nutColorNr); }
                        }
                        else if (nutColorsCount.Count >= ColorCount) { list.RemoveAt(nutColorNr); }
                    }
                }
                return list;
            }
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
                    EditBoard();
                }
            }
        }

        public string StackHeight
        {
            get
            {
                string stackHeight = string.Empty;
                foreach (byte rowStackHeight in board?.StackHeight ?? []) { stackHeight += rowStackHeight.ToString() + " "; }
                if (stackHeight.Length > 1) { stackHeight = stackHeight[..^1]; }
                return stackHeight;
            }
            set
            {
                if (board is not null)
                {
                    board.StackHeight.Clear();
                    value = value.Replace(".", " ").Replace(",", " ").Replace(":", " ").Replace(";", " ").Replace("/", " ").Replace("\\", " ").Replace("-", " ").Replace("_", " ").Replace("  ", " ");
                    string[] rowList = value.Split(" ");
                    for (int rowNr = 0; rowNr < rowList.Length; rowNr++)
                    {
                        if (byte.TryParse(rowList[rowNr], out byte stackHeight)) { board.StackHeight.Add(stackHeight); }
                    }
                    if (board.StackHeight.Count == 0) { board.StackHeight = [1]; }
                    if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                    EditBoard();
                }
                RaisePropertyChanged();
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
                    EditBoard();
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
                    if (boardstate is not null) { board.CreateInitialBoardstate(boardstate.Id); }
                    EditBoard();
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
                    board.CreateInitialBoardstate(value);
                    EditBoard();
                }
            }
        }

        public string MaxColumnsCount
        {
            get
            {
                string maxColumnsCount = string.Empty;
                foreach (byte rowMaxColumnsCount in board?.MaxColumnsCount ?? []) { maxColumnsCount += rowMaxColumnsCount.ToString() + " "; }
                if (maxColumnsCount.Length > 1) { maxColumnsCount = maxColumnsCount[..^1]; }
                return maxColumnsCount;
            }
            set
            {
                if (board is not null)
                {
                    board.MaxColumnsCount = [];
                    value = value.Replace(".", " ").Replace(",", " ").Replace(":", " ").Replace(";", " ").Replace("/", " ").Replace("\\", " ").Replace("-", " ").Replace("_", " ").Replace("  ", " ");
                    string[] rowList = value.Split(" ");
                    for (int rowNr = 0; rowNr < rowList.Length; rowNr++)
                    {
                        if (byte.TryParse(rowList[rowNr], out byte maxColumnsCount)) { board.MaxColumnsCount.Add(maxColumnsCount); }
                    }
                    EditBoard();
                }
                RaisePropertyChanged();
            }
        }

        public int SolutionCount
        {
            get { return board?.Solutions.Count ?? 0; }
        }

        public int StepCount
        {
            get { return Solution?.Boardstates.Count ?? 0; }
        }

        public string SolutionCountStr
        {
            get { return "Solutions [" + SolutionCount.ToString() + "]"; }
        }

        public string StepCountStr
        {
            get { return "Steps [" + StepCount.ToString() + "]"; }
        }

        public Solution? Solution
        {
            get { return solution; }
            set
            {
                solution = value;
                if (board is not null && solution is not null && board.Solutions.Contains(solution)) { solutionNr = board.Solutions.IndexOf(solution) + 1; }
                else { solutionNr = 0; }
                RaisePropertyChanged(nameof(SolutionNr));
                StepNr = 1;
            }
        }

        public Boardstate? Boardstate
        {
            get { return boardstate; }
            set
            {
                boardstate = value;
                if (solution is not null && boardstate is not null && solution.Boardstates.Contains(boardstate)) { stepNr = solution.Boardstates.IndexOf(boardstate) + 1; }
                else { stepNr = 0; }
                RaisePropertyChanged(nameof(StepNr));
                LoadBoardstate();
            }
        }

        public int SolutionNr
        {
            get { return solutionNr; }
            set
            {
                solutionNr = value;
                if (solutionNr > SolutionCount) { solutionNr = SolutionCount; }
                if (solutionNr < 1) { solutionNr = 1;  }
                if (Board is not null && solutionNr <= Board.Solutions.Count) { Solution = Board.Solutions[solutionNr - 1]; }
                else { Solution = null; }
            }
        }

        public int StepNr
        {
            get { return stepNr; }
            set
            {
                stepNr = value;
                if (stepNr > StepCount) { stepNr = StepCount; }
                if (stepNr < 1) { stepNr = 1; }
                if (solution is not null && stepNr <= solution.Boardstates.Count) { Boardstate = solution.Boardstates[stepNr - 1]; }
                else { Boardstate = null; }
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
            get
            {
                if (board is null || board.Solutions.Count == 0) { return -1; }
                else if (Solution == board.InitialBoardstate)
                {
                    long totalIterationCount = 0;
                    for (int _solutionNr = board.Solutions.Count - 1; _solutionNr >= 0; _solutionNr--)
                    {
                        if (_solutionNr < board.Solutions.Count) { totalIterationCount += board.Solutions[_solutionNr].IterationCount; }
                    }
                    return totalIterationCount;
                }
                return solution?.IterationCount ?? -1;
            }
        }

        public int TotalProcessDurationSec
        {
            get
            {
                if (board is null || board.Solutions.Count == 0) { return -1; }
                else if (Solution == board.InitialBoardstate)
                {
                    int maxProcessDurationSec = 0;
                    for (int _solutionNr = board.Solutions.Count - 1; _solutionNr >= 0; _solutionNr--)
                    {
                        if (_solutionNr < board.Solutions.Count && board.Solutions[_solutionNr].TotalProcessDurationSec > maxProcessDurationSec) { maxProcessDurationSec = board.Solutions[_solutionNr].TotalProcessDurationSec; }
                    }
                    return maxProcessDurationSec;
                }
                return solution?.TotalProcessDurationSec ?? -1;
            }
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
                else if (Solution == board.InitialBoardstate)
                {
                    if (board.IsFinished) { return "Board finished - 100%"; }
                    else if (board.ShortestSolution is not null) { return "Solutions found - " + Math.Round(board.Progress * 100, 0).ToString() + "%"; }
                    else { return "No solution found yet - " + Math.Round(board.Progress * 100, 0).ToString() + "%"; }
                }
                else if (solution is null) { return string.Empty; }
                else if (solution.IsFinished) { return "Solution finished - 100%"; }
                else { return "Solution is running - " + Math.Round(solution.Progress * 100, 0).ToString() + "%"; }
            }
        }

        public int RunningCount
        {
            get
            {
                int count = 0;
                if (Board is not null)
                {
                    for (int _solutionNr = Board.Solutions.Count - 1; _solutionNr >= 0; _solutionNr--)
                    {
                        if (_solutionNr < Board.Solutions.Count && !Board.Solutions[_solutionNr].IsFinished) { count++; }
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
                    for (int _solutionNr = Board.Solutions.Count - 1; _solutionNr >= 0; _solutionNr--)
                    {
                        if (_solutionNr < Board.Solutions.Count && Board.Solutions[_solutionNr].IsFinished) { count++; }
                    }
                }
                return count;
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    if (isPlaying) { IsEditableBoard = false; }
                }
                if (!isPlaying) { move = null; }
                RaisePropertyChanged();
            }
        }

        private int actualNutCount
        {
            get
            {
                if (boardstate is null) { return 0; }
                int count = 0;
                foreach (Stack stack in boardstate.Stacks)
                {
                    count += stack.Nuts.Count;
                }
                return count;
            }
        }

        public Visibility VisibilityTopMenu { get; set; } = Visibility.Collapsed;
        public Visibility VisibilityBottomMenu { get; set; } = Visibility.Collapsed;
        public Visibility VisibilityNutColorMenu { get; set; } = Visibility.Hidden;

        public bool IsEditableBoard
        {
            get { return isEditableBoard; }
            set
            {
                isEditableBoard = value;
                if (!isEditableBoard) { VisibilityNutColorMenu = Visibility.Hidden; selectedNut = null; ResetNutColors(); }
                else { IsPlaying = false; }
                RaisePropertyChanged(nameof(VisibilityNutColorMenu));
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
            RaisePropertyChanged(nameof(SolutionCountStr));
            RaisePropertyChanged(nameof(StepCountStr));
            UpdateStats();
            boardstateRows = [];
            if (boardstate is not null)
            {
                List<Stack> stacks = [];
                for (int stackNr = 0; stackNr < boardstate.Stacks.Count; stackNr++)
                {
                    stacks.Add(boardstate.Stacks[stackNr]);
                    if (board is not null && board.MaxColumnsCount.Count > boardstateRows.Count && stacks.Count >= board.MaxColumnsCount[boardstateRows.Count])
                    {
                        boardstateRows.Add(new() { Stacks = stacks });
                        stacks = [];
                    }
                }
                if (stacks.Count > 0) { boardstateRows.Add(new() { Stacks = stacks }); }
            }
            RaisePropertyChanged(nameof(BoardstateRows));
        }

        private void UpdateStats()
        {
            RaisePropertyChanged(nameof(IterationCount));
            RaisePropertyChanged(nameof(TotalProcessDurationSec));
            RaisePropertyChanged(nameof(State));
            RaisePropertyChanged(nameof(RunningCount));
            RaisePropertyChanged(nameof(FinishedCount));
        }

        private void ThreadUpdateStats()
        {
            while (board is not null && board.IsSolving)
            {
                Thread.Sleep(AnimationDelayMs);
                UpdateStats();
            }
            Thread.Sleep(AnimationDelayMs);
            UpdateStats();
        }

        private void PlayBoard(bool isManualLevelChange = false)
        {
            IsEditableBoard = false;
            if (board is not null && board.InitialBoardstate is not null)
            {
                boardstate ??= board.InitialBoardstate.Boardstates[0];
                IsPlaying = true;
                move = new();
                board.StopSolving();
                if (board.PlayerSolution is null || (!isManualLevelChange && solution != board.PlayerSolution))
                {
                    if (SolutionNr == 0 && board.InitialBoardstate.Boardstates.Count > 0)
                    {
                        board.PlayerSolution = new(board.InitialBoardstate.Boardstates[0], board) { SolveStartTime = DateTime.Now };
                    }
                    else { board.PlayerSolution = new(boardstate, board) { SolveStartTime = DateTime.Now }; }
                    board.PlayerSolution.Boardstates[0].NextMoveIndex = 0;
                }
                PlayerSolution();
            }
        }

        private void SelectStack(object obj)
        {
            if (obj.GetType() == typeof(Stack) && board is not null && boardstate is not null && solution is not null && move is not null)
            {
                Stack stack = (Stack)obj;
                int stackNr = boardstate.Stacks.IndexOf(stack);
                if (solution != board.PlayerSolution)
                {
                    board.PlayerSolution = new(boardstate, board) { SolveStartTime = DateTime.Now };
                    board.PlayerSolution.Boardstates[0].NextMoveIndex = 0;
                    PlayerSolution();
                }
                int currentBoardstateNr = solution.Boardstates.IndexOf(boardstate);
                if (currentBoardstateNr < solution.Boardstates.Count - 1)
                {
                    for (int boardstateNr = solution.Boardstates.Count - 1; boardstateNr > currentBoardstateNr; boardstateNr--)
                    {
                        solution.Boardstates.RemoveAt(boardstateNr);
                    }
                }
                if (move.FromStackNr < 0 && boardstate.Stacks[stackNr].TopNut is not null)
                {
                    move.FromStackNr = stackNr;
                    for (int nutNr = 1; nutNr < boardstate.Stacks[stackNr].TopNutCount + 1; nutNr++)
                    {
                        NutColor color = boardstate.Stacks[stackNr].Nuts[^nutNr].NutColor;
                        boardstate.Stacks[stackNr].Nuts[^nutNr].NutColor = new((byte)Math.Floor((double)byte.MaxValue * 0.75)) { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
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
                            Boardstate newBoardstate = new(boardstate.Stacks, boardstate.Solution);
                            solution.Boardstates.Add(newBoardstate);
                            move.Execute(newBoardstate);
                            move = new();
                            solution.IterationCount++;
                            solution.TotalProcessDurationSec = (int)(DateTime.Now - solution.SolveStartTime).TotalSeconds;
                            Boardstate = newBoardstate;
                        }
                    }
                }
                LoadBoardstate();
            }
        }

        private void SolveBoard()
        {
            IsPlaying = false;
            IsEditableBoard = false;
            board?.Solve();
            new Thread(ThreadUpdateStats).Start();
        }

        private void LoadBoard()
        {
            foreach (Board board in Board.List) { board.StopSolving(); }
            IsEditableBoard = false;
            int levelNr = 0;
            if (board is not null) { levelNr = Board.List.IndexOf(board); }
            Board.List = [];
            NutColor.LoadJson();
            Board.LoadJson();
            Boards = [];
            foreach (Board _board in Board.List) { Boards.Add(_board); }
            RaisePropertyChanged(nameof(Boards));
            if (boards.Count > 0) { Board = boards[Math.Min(levelNr, boards.Count - 1)]; }
        }

        private void EditBoard()
        {
            if (board is not null)
            {
                VisibilityBottomMenu = Visibility.Collapsed;
                VisibilityTopMenu = Visibility.Visible;
                board.ResetSolutions();
                board.PlayerSolution = null;
                InitialBoardstate();
                IsEditableBoard = true;
                if (actualNutCount < NutCount)
                {
                    VisibilityNutColorMenu = Visibility.Visible;
                    RaisePropertyChanged(nameof(VisibilityNutColorMenu));
                    RaisePropertyChanged(nameof(NutColors));
                }
            }
        }

        private void SelectNut(object obj)
        {
            if (obj.GetType() == typeof(Nut) && boardstate is not null && solution is not null && IsEditableBoard)
            {
                selectedNut = (Nut)obj;
                VisibilityNutColorMenu = Visibility.Visible;
                RaisePropertyChanged(nameof(VisibilityNutColorMenu));
                foreach (Stack stack in boardstate.Stacks)
                {
                    foreach (Nut nut in stack.Nuts)
                    {
                        if (nut.Id == selectedNut.Id)
                        {
                            NutColor color = nut.NutColor;
                            nut.NutColor = NutColor.GetByName(color.Name) ?? new() { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
                        }
                        else
                        {
                            NutColor color = nut.NutColor;
                            color = NutColor.GetByName(color.Name) ?? new() { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
                            nut.NutColor = new((byte)Math.Floor((double)byte.MaxValue * 0.5)) { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
                        }
                    }
                }
                LoadBoardstate();
            }
        }

        private void SelectNutColor(object obj)
        {
            if (obj.GetType() == typeof(NutColor) && board is not null && boardstate is not null && solution is not null)
            {
                NutColor newNutColor = (NutColor)obj;
                int selectedNutStackNr = -1;
                int selectedNutNr = -1;
                if (selectedNut is not null)
                {
                    for (int stackNr = 0; stackNr < boardstate.Stacks.Count; stackNr++)
                    {
                        Stack stack = boardstate.Stacks[stackNr];
                        for (int nutNr = 0; nutNr < stack.Nuts.Count; nutNr++)
                        {
                            if (stack.Nuts[nutNr].Id == selectedNut.Id)
                            {
                                selectedNutStackNr = stackNr;
                                selectedNutNr = nutNr;
                                break;
                            }
                        }
                        if (selectedNutStackNr > -1) { break; }
                    }
                    bool isChanged = false;
                    for (int stackNr = boardstate.Stacks.Count - 1; stackNr >= 0; stackNr--)
                    {
                        Stack stack = boardstate.Stacks[stackNr];
                        for (int nutNr = stack.Nuts.Count - 1; nutNr >= 0; nutNr--)
                        {
                            if (stack.Nuts[nutNr].NutColor.Name == newNutColor.Name)
                            {
                                (boardstate.Stacks[stackNr].Nuts[nutNr], boardstate.Stacks[selectedNutStackNr].Nuts[selectedNutNr]) = (boardstate.Stacks[selectedNutStackNr].Nuts[selectedNutNr], boardstate.Stacks[stackNr].Nuts[nutNr]);
                                isChanged = true;
                            }
                        }
                    }
                    if (!isChanged)
                    {
                        string selectedNutColorName = selectedNut.NutColor.Name;
                        foreach (Stack stack in boardstate.Stacks)
                        {
                            foreach (Nut nut in stack.Nuts)
                            {
                                if (nut.NutColor.Name == selectedNutColorName) { nut.NutColor = newNutColor; }
                            }
                        }
                    }
                    ResetNutColors();
                    selectedNut = null;
                }
                else if (actualNutCount < NutCount)
                {
                    selectedNutStackNr++;
                    while (selectedNutStackNr < boardstate.Stacks.Count && boardstate.Stacks[selectedNutStackNr].IsFull) { selectedNutStackNr++; }
                    if (selectedNutStackNr < boardstate.Stacks.Count && boardstate.Stacks[selectedNutStackNr].Nuts.Count < boardstate.Stacks[selectedNutStackNr].StackHeight) { selectedNutNr = boardstate.Stacks[selectedNutStackNr].Nuts.Count; }
                    if (selectedNutNr > -1) { boardstate.Stacks[selectedNutStackNr].Nuts.Add(new(newNutColor)); }
                    RaisePropertyChanged(nameof(NutColors));
                }
                if (actualNutCount >= NutCount)
                {
                    VisibilityNutColorMenu = Visibility.Hidden;
                    RaisePropertyChanged(nameof(VisibilityNutColorMenu));
                }
                LoadBoardstate();
            }
        }

        private void ResetNutColors()
        {
            foreach (Stack stack in boardstate?.Stacks ?? [])
            {
                foreach (Nut nut in stack.Nuts)
                {
                    if (nut.Id != (selectedNut?.Id ?? -1))
                    {
                        NutColor color = nut.NutColor;
                        nut.NutColor = NutColor.GetByName(color.Name) ?? new() { Name = color.Name, Red = color.Red, Green = color.Green, Blue = color.Blue };
                    }
                }
            }
        }

        private void RandomizeBoard()
        {
            if (board is not null)
            {
                IsEditableBoard = false;
                board.ResetSolutions();
                board.CreateInitialBoardstate(nameof(Boardstate));
                board.RandomizeInitialBoardstate();
                board.PlayerSolution = null;
                InitialBoardstate();
                FirstStep();
                LoadBoardstate();
            }
        }

        private void CreateNewBoard()
        {
            board ??= new();
            Board = new(board.StackCount, board.StackHeight, board.NutSameColorCount, board.ColorCount, board.MaxColumnsCount);
            Board.CreateInitialBoardstate();
            Boards.Add(board);
            InitialBoardstate();
            FirstStep();
            LoadBoardstate();
            IsEditableBoard = true;
            VisibilityNutColorMenu = Visibility.Visible;
            RaisePropertyChanged(nameof(VisibilityNutColorMenu));
            RaisePropertyChanged(nameof(NutColors));
        }

        private void DeleteBoard()
        {
            IsEditableBoard = false;
            if (board is not null && Board.List.Count > 0)
            {
                board.ResetSolutions();
                int levelNr = Board.List.IndexOf(board);
                Board.List.Remove(board);
                Boards = [];
                foreach (Board _board in Board.List) { Boards.Add(_board); }
                RaisePropertyChanged(nameof(Boards));
                if (boards.Count > 0) { Board = boards[Math.Min(levelNr, boards.Count - 1)]; }
                else { Board = null; }
            }
        }

        private void SaveBoard()
        {
            IsEditableBoard = false;
            Board.SaveJson();
            if (actualNutCount < NutCount)
            {
                VisibilityNutColorMenu = Visibility.Visible;
                RaisePropertyChanged(nameof(VisibilityNutColorMenu));
                RaisePropertyChanged(nameof(NutColors));
            }
        }

        private void PreviousSolution()
        {
            IsEditableBoard = false;
            SolutionNr--;
        }

        private void NextSolution()
        {
            IsEditableBoard = false;
            SolutionNr++;
        }

        private void FirstSolution()
        {
            IsEditableBoard = false;
            SolutionNr = 1;
        }

        private void LastSolution()
        {
            IsEditableBoard = false;
            SolutionNr = Board?.Solutions.Count ?? 1;
        }

        private void InitialBoardstate()
        {
            IsEditableBoard = false;
            Solution = Board?.InitialBoardstate ?? null;
            RaisePropertyChanged(nameof(SolutionNr));
            RaisePropertyChanged(nameof(StepNr));
        }

        private void PlayerSolution()
        {
            IsEditableBoard = false;
            Solution = Board?.PlayerSolution ?? null;
            LastStep();
            RaisePropertyChanged(nameof(SolutionNr));
        }

        private void ShortestSolution()
        {
            IsEditableBoard = false;
            Solution = Board?.ShortestSolution ?? null;
            RaisePropertyChanged(nameof(SolutionNr));
            RaisePropertyChanged(nameof(StepNr));
        }

        private void MostObviousSolution()
        {
            IsEditableBoard = false;
            Solution = Board?.MostObviousSolution ?? null;
            RaisePropertyChanged(nameof(SolutionNr));
            RaisePropertyChanged(nameof(StepNr));
        }

        private void PreviousStep()
        {
            IsEditableBoard = false;
            StepNr--;
        }

        private void NextStep()
        {
            IsEditableBoard = false;
            StepNr++;
        }

        private void FirstStep()
        {
            IsEditableBoard = false;
            StepNr = 1;
        }

        private void LastStep()
        {
            IsEditableBoard = false;
            StepNr = StepCount;
        }

        private void PlayAnimation()
        {
            IsEditableBoard = false;
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
            IsEditableBoard = false;
            AnimationIsRunning = false;
        }

        private void ReverseAnimation()
        {
            IsEditableBoard = false;
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
            IsEditableBoard = false;
            AnimationIsRunning = false;
            StepNr = 0;
        }

        private void PlayAnimationThread()
        {
            IsEditableBoard = false;
            while (((!animationIsReversed && StepNr < StepCount) || (animationIsReversed && StepNr > 1)) && AnimationIsRunning)
            {
                if (animationIsReversed) { StepNr--; }
                else { StepNr++; }
                animationIsSleeping = true;
                Thread.Sleep(animationDelayMs);
                animationIsSleeping = false;
            }
            AnimationIsRunning = false;
        }

        private void PreviousLevel()
        {
            IsEditableBoard = false;
            if (board is not null && boards.Contains(board))
            {
                int newBoardNr = boards.IndexOf(board) - 1;
                if (newBoardNr >= 0)
                {
                    Board = boards[newBoardNr];
                    if (IsPlaying) { IsPlaying = false; PlayBoard(true); }
                }
            }
        }

        private void NextLevel()
        {
            IsEditableBoard = false;
            if (board is not null && boards.Contains(board))// && board.PlayerSolution is not null && board.PlayerSolution.IsFinished)
            {
                int newBoardNr = boards.IndexOf(board) + 1;
                if (newBoardNr < boards.Count)
                {
                    Board = boards[newBoardNr];
                    if (IsPlaying) { IsPlaying = false; PlayBoard(true); }
                }
            }
        }

        private void ShowHideTopMenu()
        {
            if (VisibilityTopMenu == Visibility.Visible) { VisibilityTopMenu = Visibility.Collapsed; }
            else { VisibilityTopMenu = Visibility.Visible; }
            RaisePropertyChanged(nameof(VisibilityTopMenu));
        }

        private void ShowHideBottomMenu()
        {
            if (VisibilityBottomMenu == Visibility.Visible) { VisibilityBottomMenu = Visibility.Collapsed; }
            else { VisibilityBottomMenu = Visibility.Visible; }
            RaisePropertyChanged(nameof(VisibilityBottomMenu));
        }

        public UICmd PlayBoardCmd { get; set; }
        public UICmd SelectStackCmd { get; set; }
        public UICmd SolveBoardCmd { get; set; }
        public UICmd LoadBoardCmd { get; set; }
        public UICmd EditBoardCmd { get; set; }
        public UICmd SelectNutCmd { get; set; }
        public UICmd SelectNutColorCmd { get; set; }
        public UICmd RandomizeBoardCmd { get; set; }
        public UICmd CreateNewBoardCmd { get; set; }
        public UICmd DeleteBoardCmd { get; set; }
        public UICmd SaveBoardCmd { get; set; }
        public UICmd PreviousSolutionCmd { get; set; }
        public UICmd NextSolutionCmd { get; set; }
        public UICmd FirstSolutionCmd { get; set; }
        public UICmd LastSolutionCmd { get; set; }
        public UICmd InitialBoardstateCmd { get; set; }
        public UICmd PlayerSolutionCmd { get; set; }
        public UICmd ShortestSolutionCmd { get; set; }
        public UICmd MostObviousSolutionCmd { get; set; }
        public UICmd PreviousStepCmd { get; set; }
        public UICmd NextStepCmd { get; set; }
        public UICmd FirstStepCmd { get; set; }
        public UICmd LastStepCmd { get; set; }
        public UICmd PlayAnimationCmd { get; set; }
        public UICmd StopAnimationCmd { get; set; }
        public UICmd ReverseAnimationCmd { get; set; }
        public UICmd ResetAnimationCmd { get; set; }
        public UICmd PreviousLevelCmd { get; set; }
        public UICmd NextLevelCmd { get; set; }
        public UICmd ShowHideTopMenuCmd { get; set; }
        public UICmd ShowHideBottomMenuCmd { get; set; }
    }
}

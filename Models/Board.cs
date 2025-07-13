using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace NutSort.Models
{
    public class Board
    {
        private static readonly string path = GlobalValues.DataDirectory + nameof(Board) + ".json";
        private static readonly Random random = new();
        public static List<Board> List { get; set; } = [];

        public Board() { }
        public Board(byte stackCount, byte stackHeight, byte nutSameColorCount, byte colorCount, byte maxColumnsCount, string? _levelName = null)
        {
            StackCount = stackCount;
            StackHeight = stackHeight;
            NutSameColorCount = nutSameColorCount;
            ColorCount = colorCount;
            MaxColumnsCount = maxColumnsCount;
            List.Add(this);
            if (_levelName is null) { LevelName += List.Count.ToString(); }
            else { LevelName = _levelName; }
        }

        public Solution? InitialBoardstate { get; set; } = null;
        public List<Solution> Solutions { get; set; } = [];
        public Solution? PlayerSolution { get; set; } = null;
        public Solution? ShortestSolution { get; set; } = null;
        public Solution? MostObviousSolution { get; set; } = null;

        private string levelName = "#";
        public string LevelName
        {
            get { return levelName; }
            set
            {
                bool levelExists = false;
                foreach (Board board in List)
                {
                    if (board.levelName == value) { levelExists = true; break; }
                }
                if (!levelExists) { levelName = value; }
            }
        }

        private byte stackCount = 1;
        public byte StackCount
        {
            get { return stackCount; }
            set
            {
                if (value < 2) { value = 2; }
                if (value < colorCount) { value = colorCount; }
                if (colorCount * nutSameColorCount > value * stackHeight - 1) { value = (byte)(Math.Min(byte.MaxValue, Math.Floor((double)colorCount * nutSameColorCount / stackHeight) + 1)); }
                stackCount = value;
            }
        }

        private byte stackHeight = 1;
        public byte StackHeight
        {
            get { return stackHeight; }
            set
            {
                if (value < 2) { value = 2; }
                if (value < nutSameColorCount) { value = nutSameColorCount; }
                if (colorCount * nutSameColorCount > value * stackCount - 1) { value = (byte)(Math.Min(byte.MaxValue, Math.Floor((double)colorCount * nutSameColorCount / stackCount) + 1)); }
                stackHeight = value;
            }
        }

        private byte nutSameColorCount = 1;
        public byte NutSameColorCount
        {
            get { return nutSameColorCount; }
            set
            {
                if (value < 1) { value = 1; }
                if (value > stackHeight) { value = stackHeight; }
                if (value * colorCount > stackCount * stackHeight - 1) { value = (byte)(Math.Min(byte.MaxValue, Math.Round((double)stackCount * stackHeight / colorCount, 0) - 1)); }
                nutSameColorCount = value;
            }
        }

        private byte colorCount = 1;
        public byte ColorCount
        {
            get { return colorCount; }
            set
            {
                if (value < 1) { value = 1; }
                if (value > NutColor.List.Count) { value = (byte)NutColor.List.Count; }
                if (value > stackCount) { value = stackCount; }
                if (value * nutSameColorCount > stackCount * stackHeight - 1) { value = (byte)(Math.Min(byte.MaxValue, Math.Round((double)stackCount * stackHeight / nutSameColorCount, 0) - 1)); }
                colorCount = value;
            }
        }

        private byte maxColumnsCount = 1;
        public byte MaxColumnsCount
        {
            get { return maxColumnsCount; }
            set
            {
                if (value < 1) { value = 1; }
                maxColumnsCount = value;
            }
        }

        [JsonIgnore] public bool IsFinished
        {
            get
            {
                if (Solutions.Count == 0) { return false; }
                for (int solutionNr = Solutions.Count - 1; solutionNr >= 0; solutionNr--)
                {
                    if (solutionNr < Solutions.Count && !Solutions[solutionNr].IsFinished) { return false; }
                }
                return true;
            }
        }

        public void Solve()
        {
            ResetSolutions();
            if (InitialBoardstate?.Boardstates.Count > 0)
            {
                InitialBoardstate.SolveStartTime = DateTime.Now;
                InitialBoardstate.Boardstates[0].NextMoveIndex = 0;
                InitialBoardstate.Boardstates[0].UpdatePossibleMoves();
                for (int moveNr = 0; moveNr < InitialBoardstate.Boardstates[0].PossibleMoves.Count; moveNr++)
                {
                    Solutions.Add(new(InitialBoardstate.Boardstates[0], this) { IterationCount = moveNr });
                    InitialBoardstate.Boardstates[0].NextMoveIndex++;
                }
                for (int solutionNr = Solutions.Count - 1; solutionNr >= 0; solutionNr--)
                {
                    if (solutionNr < Solutions.Count) { new Thread(Solutions[solutionNr].Solve).Start(); }
                }
                // Thread(Solutions[0].Solve).Start();
            }
        }

        public void ResetSolutions()
        {
            StopSolving();
            int solutionsCount = Solutions.Count;
            for (int solutionNr = solutionsCount - 1; solutionNr >= 0; solutionNr--)
            {
                while (Solutions.Count > solutionNr && Solutions[solutionNr].IsSolving) { Thread.Sleep(100); }
            }
            Solutions = [];
            ShortestSolution = null;
            MostObviousSolution = null;
        }

        public void StopSolving()
        {
            int solutionsCount = Solutions.Count;
            for (int solutionNr = solutionsCount - 1; solutionNr >= 0; solutionNr--)
            {
                if (Solutions.Count > solutionNr) { Solutions[solutionNr].StopSolving(); }
            }
        }

        public static void LoadJson()
        {
            if (!File.Exists(path)) { File.WriteAllText(path, JsonConvert.SerializeObject(new List<Board>(), Formatting.Indented), Encoding.Unicode); }
            try
            {
                List = JsonConvert.DeserializeObject<List<Board>>(File.ReadAllText(path, Encoding.Unicode)) ?? [];
                GlobalValues.CurrentLogText = "Boards restored.";
                NutColor.UpdateList();
            }
            catch { GlobalValues.CurrentLogText = "Restore boards failed!"; }
        }

        public static void SaveJson()
        {
            for (int boardNr1 = 0; boardNr1 < List.Count - 1; boardNr1++)
            {
                for (int boardNr2 = boardNr1 + 1; boardNr2 < List.Count; boardNr2++)
                {
                    if (List[boardNr1].LevelName.Length > 1 && List[boardNr2].LevelName.Length > 1 &&
                        int.TryParse(List[boardNr1].LevelName[1..], out int levelNr1) && int.TryParse(List[boardNr2].LevelName[1..], out int levelNr2) && levelNr1 > levelNr2)
                    {
                        (List[boardNr1], List[boardNr2]) = (List[boardNr2], List[boardNr1]);
                    }
                }
            }
            string text = JsonConvert.SerializeObject(List, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
            File.WriteAllText(path, text, Encoding.Unicode);
            GlobalValues.CurrentLogText = "Boards saved.";
        }

        public void CreateInitialBoardstate(string? id = null)
        {
            ResetSolutions();
            InitialBoardstate = new() { Board = this };
            List<Stack> stacks = [];
            string[] ids = id?.Split('|') ?? [];
            List<Nut> nuts = [];
            int slotsCount = 0;
            //id soweit möglich aufschlüsseln und auf die Stacks verteilen
            for (byte nutNr = 0; nutNr < ids.Length; nutNr++)
            {
                slotsCount++;
                NutColor? nutColor = NutColor.GetByName(ids[nutNr]);
                if (nutColor is not null) { nuts.Add(new(nutColor)); }
                if (slotsCount >= stackHeight)
                {
                    stacks.Add(new() { Nuts = nuts });
                    if (stacks.Count >= stackCount) { break; }
                    nuts = [];
                    slotsCount = 0;
                }
            }
            //verbleibende zunächst leere Stacks hinzufügen
            for (int _stackNr = stacks.Count; _stackNr < stackCount; _stackNr++) { stacks.Add(new()); }
            InitialBoardstate.Boardstates.Add(new(stacks, InitialBoardstate));
            string emptyId = string.Empty;
            for (int stackNr = 0; stackNr < StackCount; stackNr++) { emptyId += "#|"; }
            int idLength = Math.Max(0, Math.Min(emptyId.Length - 1, id?.Length ?? 0));
            if (!string.IsNullOrEmpty(id) && id[..idLength] != emptyId[..idLength])
            {
                FillInitialBoardstate();
            }
        }

        public void FillInitialBoardstate()
        {
            //Stacks mit Nuts aufstocken bis korrekte Anzahl an verschiedener Farben und Nuts gleicher Farben erreicht ist
            Dictionary<NutColor, int> nutColorsCount = ForceColorCount();
            List<Stack> stacks = InitialBoardstate?.Boardstates[0].Stacks ?? [];
            while (nutColorsCount.Keys.Count < ColorCount)
            {
                foreach (NutColor nutColor in NutColor.List)
                {
                    if (!nutColorsCount.ContainsKey(nutColor)) { nutColorsCount[nutColor] = 0; break; }
                }
            }
            foreach (NutColor nutColor in nutColorsCount.Keys)
            {
                int stackNr = 0;
                while (nutColorsCount[nutColor] < NutSameColorCount)
                {
                    if (stacks[stackNr].Nuts.Count < StackHeight)
                    {
                        stacks[stackNr].Nuts.Add(new(nutColor));
                        nutColorsCount[nutColor]++;
                    }
                    else { stackNr++; }
                }
            }
            InitialBoardstate?.Boardstates.Add(new(stacks, InitialBoardstate));
        }

        public Dictionary<NutColor, int> ForceColorCount()
        {
            //Nut entfernen, falls zu viele verschiedene Farben oder zu viele Nuts der gleichen Farbe verteilt
            List<Stack> stacks = InitialBoardstate?.Boardstates[0].Stacks ?? [];
            Dictionary<NutColor, int> nutColors = [];
            for (int _stackNr = 0; _stackNr < stacks.Count; _stackNr++)
            {
                for (int nutNr = stacks[_stackNr].Nuts.Count - 1; nutNr >= 0; nutNr--)
                {
                    if (nutColors.TryGetValue(stacks[_stackNr].Nuts[nutNr].NutColor, out int nutSameColorCount))
                    {
                        if (nutSameColorCount >= NutSameColorCount) { stacks[_stackNr].Nuts.RemoveAt(nutNr); }
                        else { nutColors[stacks[_stackNr].Nuts[nutNr].NutColor] += 1; }
                    }
                    else
                    {
                        if (nutColors.Keys.Count >= ColorCount) { stacks[_stackNr].Nuts.RemoveAt(nutNr); }
                        else { nutColors[stacks[_stackNr].Nuts[nutNr].NutColor] = 1; }
                    }
                }
            }
            return nutColors;
        }

        public void RandomizeInitialBoardstate(int iterationCount = 1000)
        {
            if (InitialBoardstate is not null && InitialBoardstate.Boardstates.Count > 0)
            {
                Boardstate randomBoard = InitialBoardstate.Boardstates[0];
                for (int stackNr0 = randomBoard.Stacks.Count - 1; stackNr0 >= 0; stackNr0--)
                {
                    for (int stackNr1 = 0; stackNr1 < stackNr0; stackNr1++)
                    {
                        int slotNr0 = randomBoard.Stacks[stackNr0].Nuts.Count - 1;
                        int slotNr1 = randomBoard.Stacks[stackNr1].Nuts.Count;
                        while (slotNr0 >= 0 && slotNr1 < stackHeight)
                        {
                            randomBoard.Stacks[stackNr1].Nuts.Add(randomBoard.Stacks[stackNr0].Nuts[slotNr0]);
                            randomBoard.Stacks[stackNr0].Nuts.RemoveAt(slotNr0);
                            slotNr0 -= 1;
                            slotNr1 += 1;
                        }
                    }
                }
                for (int iterationNr = 0; iterationNr < iterationCount; iterationNr++)
                {
                    int stackNr1 = random.Next(randomBoard.Stacks.Count);
                    int stackNr2 = random.Next(randomBoard.Stacks.Count);
                    if (randomBoard.Stacks[stackNr1].Nuts.Count > 0 && randomBoard.Stacks[stackNr2].Nuts.Count > 0)
                    {
                        int slotNr1 = random.Next(randomBoard.Stacks[stackNr1].Nuts.Count);
                        int slotNr2 = random.Next(randomBoard.Stacks[stackNr2].Nuts.Count);
                        (randomBoard.Stacks[stackNr1].Nuts[slotNr1], randomBoard.Stacks[stackNr2].Nuts[slotNr2]) = (randomBoard.Stacks[stackNr2].Nuts[slotNr2], randomBoard.Stacks[stackNr1].Nuts[slotNr1]);
                    }
                }
            }
        }

        public override string ToString()
        {
            return levelName;
        }
    }
}

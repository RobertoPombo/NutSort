using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace NutSort.Models
{
    public class Board
    {
        private static readonly string path = GlobalValues.DataDirectory + nameof(Board) + ".json";
        public static List<Board> List { get; set; } = [];

        public Board() { }
        public Board(byte stackCount, byte stackHeight, byte nutSameColorCount, byte colorCount, byte rowsCount)
        {
            StackCount = stackCount;
            StackHeight = stackHeight;
            NutSameColorCount = nutSameColorCount;
            ColorCount = colorCount;
            MaxColumnsCount = rowsCount;
            List.Add(this);
        }

        public Solution? InitialBoardstate { get; set; } = null;
        public List<Solution> Solutions { get; set; } = [];
        public Solution? ShortestSolution { get; set; } = null;

        private byte stackCount = 14;
        public byte StackCount
        {
            get { return stackCount; }
            set
            {
                if (value < 2) { value = 2; }
                stackCount = value;
            }
        }

        private byte stackHeight = 4;
        public byte StackHeight
        {
            get { return stackHeight; }
            set
            {
                if (value < 2) { value = 2; }
                stackHeight = value;
            }
        }

        private byte nutSameColorCount = 4;
        public byte NutSameColorCount
        {
            get { return nutSameColorCount; }
            set
            {
                if (value < 1) { value = 1; }
                if (value > stackHeight) { value = stackHeight; }
                nutSameColorCount = value;
            }
        }

        private byte colorCount = 12;
        public byte ColorCount
        {
            get { return colorCount; }
            set
            {
                if (value < 1) { value = 1; }
                if (value > NutColor.List.Count) { value = (byte)NutColor.List.Count; }
                if (value > stackCount) { value = (byte)(stackCount); }
                if (value * nutSameColorCount > stackCount * stackHeight - 1) { value = (byte)(Math.Min(byte.MaxValue, Math.Round((double)stackCount * stackHeight / nutSameColorCount, 0) - 1)); }
                colorCount = value;
            }
        }

        private byte maxColumnsCount = 5;
        public byte MaxColumnsCount
        {
            get { return maxColumnsCount; }
            set
            {
                if (value < 1) { value = 1; }
                maxColumnsCount = value;
            }
        }

        public void Solve()
        {
            if (InitialBoardstate?.Boardstates.Count > 0)
            {
                foreach (Move move in InitialBoardstate.Boardstates[0].PossibleMoves)
                {
                    move.Execute(InitialBoardstate.Boardstates[0]);
                    Solutions.Add(new (InitialBoardstate.Boardstates[0], this));
                    move.Undo(InitialBoardstate.Boardstates[0]);
                }
                foreach (Solution solution in Solutions)
                {
                    new Thread(solution.Solve).Start();
                }
                //new Thread(Solutions[0].Solve).Start();
            }
        }

        public void StopSolving()
        {
            foreach (Solution solution in Solutions)
            {
                solution.StopSolving();
            }
        }

        public static void LoadJson()
        {
            if (!File.Exists(path)) { File.WriteAllText(path, JsonConvert.SerializeObject(new List<Board>(), Formatting.Indented), Encoding.Unicode); }
            try
            {
                List = JsonConvert.DeserializeObject<List<Board>>(File.ReadAllText(path, Encoding.Unicode)) ?? [];
                GlobalValues.CurrentLogText = "Boards restored.";
            }
            catch { GlobalValues.CurrentLogText = "Restore boards failed!"; }
            NutColor.SaveJson();
        }

        public static void SaveJson()
        {
            string text = JsonConvert.SerializeObject(List, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
            File.WriteAllText(path, text, Encoding.Unicode);
            GlobalValues.CurrentLogText = "Boards saved.";
        }

        public void CreateInitialBoardstate(string id)
        {
            string[] ids = id.Split('|');
            InitialBoardstate = new() { Board = this };
            List<Stack> stacks = [];
            List<Nut> nuts = [];
            for (byte nutNr = 0; nutNr < ids.Length; nutNr++)
            {
                NutColor? nutColor = NutColor.GetByName(ids[nutNr]);
                if (nutColor is not null)
                {
                    nuts.Add(new(nutColor));
                    if (nuts.Count >= stackHeight)
                    {
                        stacks.Add(new() { Nuts = nuts });
                        nuts = [];
                    }
                }
            }
            for (byte stackNr = (byte)stacks.Count; stackNr < stackCount; stackNr++)
            {
                stacks.Add(new());
            }
            InitialBoardstate.Boardstates.Add(new(stacks, InitialBoardstate));
        }
    }
}

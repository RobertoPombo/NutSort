using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using Newtonsoft.Json;

namespace NutSort.Models
{
    public class NutColor
    {
        private static readonly string path = GlobalValues.DataDirectory + nameof(NutColor) + ".json";
        public static List<NutColor> List { get; set; } = [];

        public NutColor() { }
        public NutColor(byte _alpha) { alpha = _alpha; }

        public string Name { get; set; } = string.Empty;
        public byte Red { get; set; } = byte.MinValue;
        public byte Green { get; set; } = byte.MinValue;
        public byte Blue { get; set; } = byte.MinValue;

        private byte alpha = byte.MaxValue;
        [JsonIgnore] public byte Alpha { get { return alpha; } }

        [JsonIgnore] public Brush Preview { get { return new SolidColorBrush(Color.FromArgb(Alpha, Red, Green, Blue)); } }

        public static NutColor? GetByName(string name)
        {
            foreach (NutColor nutColor in List)
            {
                if (nutColor.Name.ToLower() == name.ToLower()) { return nutColor; }
            }
            return null;
        }

        public static void UpdateList()
        {
            foreach (Board board in Board.List)
            {
                if (board.InitialBoardstate is not null) { UpdateList(board.InitialBoardstate); }
                foreach (Solution solution in board.Solutions) { UpdateList(solution); }
            }
            SaveJson();
        }

        private static void UpdateList(Solution solution)
        {
            foreach (Boardstate boardstate in solution.Boardstates)
            {
                foreach (Stack stack in boardstate.Stacks)
                {
                    foreach (Nut nut in stack.Nuts)
                    {
                        NutColor? nutColor = GetByName(nut.NutColor.Name);
                        if (nutColor is null) { List.Add(nut.NutColor); }
                        else { nut.NutColor = nutColor; }
                    }
                }
            }
        }

        public static void LoadJson()
        {
            if (!File.Exists(path)) { File.WriteAllText(path, JsonConvert.SerializeObject(new List<NutColor>(), Formatting.Indented), Encoding.Unicode); }
            try
            {
                List = JsonConvert.DeserializeObject<List<NutColor>>(File.ReadAllText(path, Encoding.Unicode)) ?? [];
                GlobalValues.CurrentLogText = "Nut colors restored.";
            }
            catch { GlobalValues.CurrentLogText = "Restore nut colors failed!"; }
        }

        public static void SaveJson()
        {
            string text = JsonConvert.SerializeObject(List, Formatting.Indented);
            File.WriteAllText(path, text, Encoding.Unicode);
            GlobalValues.CurrentLogText = "Nut colors saved.";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

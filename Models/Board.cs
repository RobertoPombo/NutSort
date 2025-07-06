using System;

namespace NutSort.Models
{
    public class Board
    {
        public static readonly List<Board> List = [];

        public Board() { }
        public Board(byte stackCount, byte stackHeight, byte nutSameColorCount, byte colorCount, byte rowsCount)
        {
            StackCount = stackCount;
            StackHeight = stackHeight;
            NutSameColorCount = nutSameColorCount;
            ColorCount = colorCount;
            RowsCount = rowsCount;
            if (IsValid()) { List.Add(this); }
        }

        public List<Solution> Solutions { get; set; } = [];
        public Solution? ShortestSolution { get; set; } = null;
        public byte StackCount { get; set; } = 11;
        public byte StackHeight { get; set; } = 4;
        public byte NutSameColorCount { get; set; } = 4;
        public byte ColorCount { get; set; } = 9;
        public byte RowsCount { get; set; } = 5;

        public bool IsValid()
        {
            if (RowsCount > StackCount) { return false; }
            if (StackHeight < NutSameColorCount) { return false; }
            if (ColorCount < StackCount + 1) { return false; }
            if (StackCount * StackHeight < NutSameColorCount * ColorCount + 1) { return false; }
            if (StackCount < 1 || StackHeight < 1 || NutSameColorCount < 1 || ColorCount < 1 || RowsCount < 1) { return false; }
            return true;
        }
    }
}

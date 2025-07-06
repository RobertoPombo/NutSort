using System;
using System.Text.Json.Serialization;

namespace NutSort.Models
{
    public class Solution
    {
        public Solution() { }
        public List<Boardstate> BoardStates { get; set; } = [];
        public List<Nut> Nuts { get; set; } = [];
        [JsonIgnore] public Board Board { get; set; } = new();

        public bool IsFinished
        {
            get
            {
                if (BoardStates.Count == 0) { return false; }
                return BoardStates[^1].IsFinished;
            }
        }
    }
}

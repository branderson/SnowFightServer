using System.Collections.Generic;

namespace Networking.Data
{
    public enum LeaderboardDataType
    {
        Player = 1,
        Team = 2,
    }

    public class LeaderboardDataEntry
    {
        public LeaderboardDataType Type;
        public int Rank;
        public string Name;
        public int Score;
    }
}
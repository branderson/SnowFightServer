using System;
using System.Collections.Generic;

namespace Game.State
{
    [Serializable]
    public class GameSave
    {
        public SerializablePlayer[] Players;
        public SerializableTeam[] Teams;
    }
}
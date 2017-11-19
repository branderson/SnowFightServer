using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.State
{
    public class Team
    {
        public string Name { get; private set; }
        public readonly List<string> Members;

        public int Score
        {
            get { return Members.Sum(userID => World.Instance.GetPlayer(userID).Score); }
        }

        public Team(string name)
        {
            Name = name;
            Members = new List<string>();
        }

        public void AddUser(string userID)
        {
            if (Members.Contains(userID)) return;
            Members.Add(userID);
        }

        public void RemoveUser(string userID)
        {
            Members.Remove(userID);
        }
    }

    [Serializable]
    public class SerializableTeam
    {
        public string Name;
        public string[] Members;

        public SerializableTeam(Team team)
        {
            Members = new string[team.Members.Count];
            Name = team.Name;
            team.Members.CopyTo(Members);
        }
    }
}
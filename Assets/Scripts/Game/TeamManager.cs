using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Utility;
using Game.State;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class TeamManager : Singleton<TeamManager>
    {
        // Dictionary<userID, Team>
        private readonly Dictionary<string, Team> _userToTeam = new Dictionary<string, Team>();

        // Dictionary<teamName, Team>
        private readonly Dictionary<string, Team> _teamNameToTeam = new Dictionary<string, Team>();

        protected TeamManager() { }

        private Team CreateTeam(string teamName)
        {
            Team team = new Team(teamName);
            _teamNameToTeam[teamName] = team;
            Debug.Log(string.Format("Event: Create Team, Team Name: {0}", teamName));
            return team;
        }

        private void RemoveTeam(Team team)
        {
            _teamNameToTeam.Remove(team.Name);
        }

        /// <summary>
        /// Add the given user to the given team, creating the team if it does not exist.
        /// Will also remove player from previous team, if any.
        /// Will not add player to team if team name is blank
        /// </summary>
        /// <param name="userID">
        /// UserID to add to team
        /// </param>
        /// <param name="teamName">
        /// Name of team to add user to
        /// </param>
        /// <returns>
        /// Whether the user was successfully added to the team
        /// </returns>
        public bool AddUserToTeam(string userID, string teamName)
        {
            if (string.IsNullOrEmpty(teamName)) return false;
            Team team;
            if (!_teamNameToTeam.TryGetValue(teamName, out team))
            {
                team = CreateTeam(teamName);
            }
            team.AddUser(userID);
            Team oldTeam;
            if (_userToTeam.TryGetValue(userID, out oldTeam))
            {
                oldTeam.RemoveUser(userID);
                if (oldTeam.Members.Count == 0) RemoveTeam(oldTeam);
                Debug.Log(string.Format("Event: Remove From Team, UserID: {0}, Team Name: {1}", userID, oldTeam.Name));
            }
            _userToTeam[userID] = team;
            Debug.Log(string.Format("Event: Join Team, UserID: {0}, Team Name: {1}", userID, teamName));
            return true;
        }

        public Team GetUserTeam(string userID)
        {
            Team team;
            if (_userToTeam.TryGetValue(userID, out team))
            {
                return team;
            }
            return new Team("");
        }

        public List<Team> GetTeams()
        {
            return _teamNameToTeam.Values.ToList();
        }

        public void LoadTeams(SerializableTeam[] teams)
        {
            _teamNameToTeam.Clear();
            _userToTeam.Clear();
            foreach (SerializableTeam team in teams)
            {
                foreach (string userID in team.Members)
                {
                    AddUserToTeam(userID, team.Name);
                }
            }
        }
    }
}
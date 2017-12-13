using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Game.State;
using UnityEngine;

namespace Game
{
    public class GameSaveManager : MonoBehaviour
    {
        [SerializeField] private string _savePath = "stateSave.sav";

        public void Save()
        {
            List<Player> players = World.Instance.GetPlayers();
            List<Team> teams = TeamManager.Instance.GetTeams();
            SerializablePlayer[] serializablePlayers = players.Select(player => new SerializablePlayer(player)).ToArray();
            SerializableTeam[] serializableTeams = teams.Select(team => new SerializableTeam(team)).ToArray();

            GameSave save = new GameSave
            {
                Players = serializablePlayers,
                Teams = serializableTeams,
            };
            FileStream file = File.Create(Application.dataPath + "/" + _savePath);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, save);
            file.Close();
        }

        public void Load()
        {
            if (File.Exists(Application.dataPath + "/" + _savePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/" + _savePath, FileMode.Open);
                GameSave loaded = (GameSave)bf.Deserialize(file);
                file.Close();
                TeamManager.Instance.LoadTeams(loaded.Teams);
                World.Instance.LoadPlayers(loaded.Players);
            }
        }

        public void SaveAndQuit()
        {
            Save();
            Application.Quit();
        }
    }
}
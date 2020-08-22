using TMPro;
using System.IO;
using UnityEngine;

namespace HexDeniz
{
    public class StatsManager : MonoBehaviour
    {
        public static StatsManager Instance;

        public TMP_Text HighScoreText;
        public TMP_Text CurrentScoreText;
        public TMP_Text CurrentMovesText;

        public SaveData Data { get; private set; }

        public bool HasSave => Data.HasSave;
        public uint Score => Data.CurrentScore;

        private static string SavePath;

        private void Awake()
        {
            Instance = this;
            SavePath = Application.persistentDataPath + "/save.dat";

            if (File.Exists(SavePath))
                Data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            else
                Data = new SaveData();
        }

        //Start game
        public void NewGame()
        {
            //Reset values for the new game
            Data.HasSave = false;
            Data.CurrentScore = 0;
            Data.CurrentMoves = 0;

            //Refresh UI
            RefreshUI();
        }

        public void LoadGame()
        {
            //Note:
            //We don't need to fiddle with save data or anything, data is already loaded
            //GridManager will fetch data it needs, no need to do something special here

            //Refresh UI
            RefreshUI();
        }

        //UI
        private void RefreshUI()
        {
            CurrentScoreText.SetText($"Score: {Data.CurrentScore}");
            HighScoreText.SetText($"High: {Data.HighScore}");
            CurrentMovesText.SetText($"#{Data.CurrentMoves}");
        }

        //Data
        public void AddMove()
        {
            Data.CurrentMoves++;
            RefreshUI();
        }

        public void AddScore(uint value)
        {
            Data.CurrentScore += value;

            //Update highscore if needed
            if (Data.CurrentScore > Data.HighScore)
                Data.HighScore = Data.CurrentScore;

            RefreshUI();
        }

        public void ClearGame()
        {
            Data.HasSave = false;
            Data.CurrentScore = 0;
            Data.CurrentMoves = 0;

            //Write to file
            File.WriteAllText(SavePath, JsonUtility.ToJson(Data));
        }

        /// <summary>
        /// Saves the current data, call this once a move is completed and grid is changed.
        /// </summary>
        public void SaveGame()
        {
            var grid = GridManager.Instance;
            Data.HasSave = true;

            //Set scale
            Data.Width = grid.Width;
            Data.Height = grid.Height;

            //Set hexagons
            Data.HexagonColors = new int[grid.Width][];
            for (int x = 0; x < grid.Width; x++)
            {
                Data.HexagonColors[x] = new int[grid.Height];
                for (int y = 0; y < grid.Height; y++)
                    Data.HexagonColors[x][y] = grid.Get(x, y).Color;
            }

            //Set bombs
            Data.Bombs = new int[grid.Bombs.Count][];
            for (int i = 0; i < Data.Bombs.Length; i++)
                Data.Bombs[i] = new[] { grid.Bombs[i].Index.x, grid.Bombs[i].Index.y };

            //Write to file
            File.WriteAllText(SavePath, JsonUtility.ToJson(Data));
        }

    }
}
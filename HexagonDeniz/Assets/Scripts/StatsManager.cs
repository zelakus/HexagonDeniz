using TMPro;
using UnityEngine;

namespace HexDeniz
{
    public class StatsManager : MonoBehaviour
    {
        public static StatsManager Instance;

        public TMP_Text HighScoreText;
        public TMP_Text CurrentScoreText;
        public TMP_Text CurrentMovesText;

        private SaveData Data;

        public bool HasSave => Data.HasSave;

        private void Awake()
        {
            Instance = this;
            //TODO: try to load SaveData
            Data = new SaveData();
        }

        //Start game
        public void NewGame()
        {
            Data.HasSave = true;
            Data.CurrentScore = 0;
            Data.CurrentMoves = 0;
            RefreshUI();
        }

        public void LoadGame()
        {
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
            RefreshUI();
        }
    }
}
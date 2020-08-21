using System;

namespace HexDeniz
{
    [Serializable]
    public class SaveData
    {
        public uint HighScore = 0;

        public bool HasSave = false;

        //Session
        public int Width, Height;
        public uint CurrentScore = 0;
        public uint CurrentMoves = 0;
        public int[][] HexagonColors;
        public int[][] Bombs;
    }
}
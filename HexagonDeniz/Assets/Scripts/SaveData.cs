using System;

namespace HexDeniz
{
    [Serializable]
    public class SaveData
    {
        public uint HighScore = 0;

        public bool HasSave = false;

        //Session
        public uint CurrentScore = 0;
        public uint CurrentMoves = 0;
    }
}
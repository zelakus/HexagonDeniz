using UnityEngine;

namespace HexDeniz
{
    public class BombHexagon : Hexagon
    {
        public int Counter;

        public BombHexagon(GameObject obj, int x, int y) : base(obj, x, y)
        {
            Counter = Random.Range(5, 10);
        }

        public bool Tick()
        {
            return --Counter <= 0;
        }
    }
}
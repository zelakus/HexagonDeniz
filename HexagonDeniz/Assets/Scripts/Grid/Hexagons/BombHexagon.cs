using UnityEngine;

namespace HexDeniz
{
    public class BombHexagon : Hexagon
    {
        public int Counter;

        public BombHexagon(GameObject obj) : base(obj)
        {
            Counter = Random.Range(0, 10);
        }

        public bool Tick()
        {
            return --Counter <= 0;
        }
    }
}
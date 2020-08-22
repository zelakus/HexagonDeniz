using UnityEngine;

namespace HexDeniz
{
    public class BombHexagon : Hexagon
    {
        public int Counter;

        public BombHexagon(GameObject obj, int x, int y) : base(obj, x, y)
        {
            Counter = Random.Range(5, 10); 
            UpdateUI();
        }

        public void UpdateUI()
        {
            Obj.GetComponentInChildren<TMPro.TMP_Text>().SetText(Counter.ToString());
        }

        public bool Tick()
        {
            Counter--;
            UpdateUI();

            //Return if we exploded
            return Counter <= 0;
        }
    }
}
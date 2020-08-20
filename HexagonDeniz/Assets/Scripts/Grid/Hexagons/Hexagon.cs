using UnityEngine;

namespace HexDeniz
{
    public class Hexagon
    {
        public HexagonType HexaType;
        public int Color;
        public RectTransform Rect;
        public GameObject Obj;
        public Vector2 TargetPosition;

        public Hexagon(GameObject obj)
        {
            Obj = obj;
            Rect = obj.GetComponent<RectTransform>();
        }

        public void Reposition()
        {
            Rect.anchoredPosition = TargetPosition;
            Rect.transform.rotation = Quaternion.identity;
        }
    }
}
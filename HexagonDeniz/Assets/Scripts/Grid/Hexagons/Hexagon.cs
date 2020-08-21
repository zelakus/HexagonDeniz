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
        public Vector2Int Index;

        public Hexagon(GameObject obj, int x, int y)
        {
            Obj = obj;
            Rect = obj.GetComponent<RectTransform>();
            Index = new Vector2Int(x, y);
        }

        public void Reposition()
        {
            Rect.anchoredPosition = TargetPosition;
            Rect.transform.rotation = Quaternion.identity;
        }

        public void Reposition(float t)
        {
            //Using current=>target instead of start=>target, which should give us a descending curve
            Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, TargetPosition, t);
            Rect.transform.rotation = Quaternion.identity;
        }

        public void PositionOnSpawnPoint()
        {
            TargetPosition = GridManager.Instance.IndexToPosition(Index.x, -1);
            Reposition();
        }

        public void Destroy(bool withVFX = false)
        {
            if (withVFX)
            {
                //TODO: vfx
            }
            GameObject.Destroy(Obj);
        }
    }
}
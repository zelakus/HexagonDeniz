using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HexDeniz
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance;

        //Grid Options
        public Color[] Colors = new Color[5];
        public int Width = 8, Height = 9;
        public Transform Content;
        public float Spacing = 3f;

        //Object Pool
        public readonly List<GameObject> FreeHexaPool = new List<GameObject>();

        //Grid
        private Hexagon[,] Hexagons;
        private readonly List<BombHexagon> Bombs = new List<BombHexagon>();

        //Resources
        private GameObject HexObjNormal;

        void Awake()
        {
            Instance = this;

            //Check color count
            if (Colors.Length < 2)
            {
                Debug.LogError("Expecting at least 2 hexagon colors");
                return;
            }

            //Check if content is set
            if (Content == null)
                throw new System.NullReferenceException("Grid Content is null");

            //Load Resources
            HexObjNormal = Res.LoadGameObject("Hexagon");
        }

        private void Start()
        {
            //Calculate Params
            size = HexObjNormal.GetComponent<RectTransform>().sizeDelta;
            w = 3 * size.x / 4f + Spacing;
            h = size.y + Spacing;
            
            var rect = Content.GetComponent<RectTransform>().rect;
            offset = (new Vector2(rect.width, rect.height) - GetBoundingBox())/2f;
            offset = new Vector2(offset.x, -offset.y);
            
            //Generate Grid
            GenerateGrid();
        }
        private void GenerateGrid()
        {
            Hexagons = new Hexagon[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    //Create & Set Object
                    var obj = Instantiate(HexObjNormal, Content);
                    Hexagons[x, y].Obj = obj;
                    //Set Hexagon Type
                    Hexagons[x, y].HexaType = HexagonType.Normal;
                    //Set Position
                    var rect = obj.GetComponent<RectTransform>();
                    rect.anchoredPosition = IndexToPosition(x, y);
                    //Set Color
                    var color = Random.Range(0, Colors.Length);
                    Hexagons[x, y].Color = color;
                    obj.transform.GetChild(0).GetComponent<Image>().color = Colors[color];
                }
        }

        #region Grid Helpers
        Vector2 offset, size;
        float w, h;
        public Vector2 GetBoundingBox()
        {
            return new Vector2(size.x / 4f - Spacing * 2 + w * Width, h * (Height + 0.5f));
        }

        public Vector2 IndexToPosition(int x, int y)
        {
            var yOffset = size.y / 2f;
            if (x % 2 == 0)
                yOffset += h / 2f;

            return new Vector2(offset.x + size.x / 2f + x * w,
                offset.y - yOffset- y * h);
        }

        public PointInfo GetPositionInfo(float x, float y)
        {
            //Take out the offsets from local position
            x -= offset.x;
            y += offset.y;

            //Find the column
            x -= size.x / 2f;

            if (x < 0)
                return new PointInfo(false); //Out of selection bounds (left side)

            if (GetBoundingBox().x - x < size.x)
                return new PointInfo(false); //Out of selection bounds (right side)

            var column = Mathf.FloorToInt(x / w);
            
            //Find the row
            y -= size.y / 2f;
            if (y < 0)
                return new PointInfo(false); //Out of selection bounds (top side)

            if (GetBoundingBox().y - y < size.y)
                return new PointInfo(false); //Out of selection bounds (bottom side)


            var row = Mathf.FloorToInt(y / h);

            //Get relative coords
            var rx = x - column * w;
            var ry = h - (y - (row - 0.5f) * h);

            //Check triangle
            if (column % 2 == 0)
            {
                var angle = Vector2.Angle(new Vector2(w, 0), new Vector2(rx, ry));
                if (angle > 30)
                {
                    if (ry > 0)
                    {
                        if (row - 1 < 0)
                            return new PointInfo(false);
                        //Upper triangle
                        return new PointInfo(isLeftTri: false,
                            new Vector2Int(column, row - 1),
                            new Vector2Int(column, row),
                            new Vector2Int(column + 1, row));
                    }
                    else
                    {
                        if (row + 2 > Height)
                            return new PointInfo(false);
                        //Lower triangle
                        return new PointInfo(isLeftTri: false,
                            new Vector2Int(column, row),
                            new Vector2Int(column, row + 1),
                            new Vector2Int(column + 1, row + 1));
                    }
                }
                else
                {
                    if (row + 2 > Height)
                        return new PointInfo(false);
                    //Middle triangle
                    return new PointInfo(isLeftTri: true,
                        new Vector2Int(column, row),
                        new Vector2Int(column + 1, row),
                        new Vector2Int(column + 1, row + 1));
                }
            }
            else
            {
                var angle = Vector2.Angle(new Vector2(-w, 0), new Vector2(-rx, ry));
                if (angle > 30)
                {
                    if (ry > 0)
                    {
                        if (row - 1 < 0)
                            return new PointInfo(false);
                        //Upper triangle
                        return new PointInfo(isLeftTri: true,
                            new Vector2Int(column, row),
                            new Vector2Int(column + 1, row-1),
                            new Vector2Int(column + 1, row));
                    }
                    else
                    {
                        if (row + 2 > Height)
                            return new PointInfo(false);
                        //Lower triangle
                        return new PointInfo(isLeftTri: true,
                            new Vector2Int(column, row + 1),
                            new Vector2Int(column + 1, row),
                            new Vector2Int(column + 1, row + 1));
                    }
                }
                else
                {
                    if (row + 2 > Height)
                        return new PointInfo(false);
                    //Middle triangle
                    return new PointInfo(isLeftTri: false,
                        new Vector2Int(column, row),
                        new Vector2Int(column, row + 1),
                        new Vector2Int(column + 1, row));
                }
            }
        }
        #endregion
    }
}
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
                    Hexagons[x, y] = new Hexagon(obj)
                    {
                        //Set Hexagon Type
                        HexaType = HexagonType.Normal
                    };
                    //Set Position
                    Hexagons[x, y].TargetPosition = IndexToPosition(x, y);
                    Hexagons[x, y].Reposition();
                    //Set Color
                    var color = Random.Range(0, Colors.Length);
                    Hexagons[x, y].Color = color;
                    obj.transform.GetChild(0).GetComponent<Image>().color = Colors[color];
                }
        }

        public void Replace(Vector2Int a, Vector2Int b)
        {
            var A = Get(a);
            var B = Get(b);

            //Set B at A's place
            Hexagons[a.x, a.y] = B;
            //Set new positon
            B.TargetPosition = IndexToPosition(a.x, a.y);
            B.Reposition();

            //Set A at B's place
            Hexagons[b.x, b.y] = A;
            //Set new positon
            A.TargetPosition = IndexToPosition(b.x, b.y);
            A.Reposition();
        }

        public Hexagon Get(Vector2Int vector)
        {
            if (vector.x < 0 || vector.x >= Width || vector.y < 0 || vector.y >= Height)
                return null;

            return Hexagons[vector.x, vector.y];
        }

        public int Refresh()
        {
            //Explode neighbour hexagons with same color
            List<Vector2Int> destroyedHexagons = new List<Vector2Int>();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var index = new Vector2Int(x, y);
                    if (Get(index) == null)
                        continue;

                    var neighbours = GetNeighbours(new List<Vector2Int>() { index }, 1);
                    if (neighbours.Count >= 3)
                    {
                        destroyedHexagons.Add(index);
                        //Explode each neighbour hexagon
                        foreach (var hexa in neighbours)
                        {
                            //Get current neighbour
                            var current = Get(hexa);
                            //If it was a bomb, remove from list
                            if (current.HexaType == HexagonType.Bomb)
                                Bombs.Remove(current as BombHexagon);
                            //Destroy the hexagon
                            current.Destroy();
                            Hexagons[hexa.x, hexa.y] = null;
                        }
                    }
                }

            //If this was a valid move with explosions, tick the bombs
            if (destroyedHexagons.Count != 0)
            {
                //Tick bombs
                foreach (var bomb in Bombs)
                    if (bomb.Tick())
                        return -1; //Bomb exploded
            }

            //TODO: Spawn new hexagons for destroyed ones


            //Return destroyed hexagons for score calculation
            return destroyedHexagons.Count;
        }

        public List<Vector2Int> GetNeighbours(List<Vector2Int> hexas, int oldCount)
        {
            var list = new List<Vector2Int>(hexas);
            var rootColor = Get(hexas[0]).Color;

            foreach (var hexa in hexas)
            {
                Vector2Int cInd;
                //Left
                cInd = hexa + Vector2Int.left;
                if (Get(cInd)?.Color == rootColor && !list.Contains(cInd))
                    list.Add(cInd);
                //Right
                cInd = hexa + Vector2Int.right;
                if (Get(cInd)?.Color == rootColor && !list.Contains(cInd))
                    list.Add(cInd);
                //Up
                cInd = hexa + Vector2Int.up;
                if (Get(cInd)?.Color == rootColor && !list.Contains(cInd))
                    list.Add(cInd);
                //Down
                cInd = hexa + Vector2Int.down;
                if (Get(cInd)?.Color == rootColor && !list.Contains(cInd))
                    list.Add(cInd);
            }

            //If there are no new ones then return, else check for more
            if (list.Count == oldCount)
                return list;
            else
                return GetNeighbours(list, list.Count);
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

        public PointInfo GetPositionInfo(Vector2 pos)
        {
            //Take out the offsets from local position
            pos.x -= offset.x;
            pos.y += offset.y;

            //Find the column
            pos.x -= size.x / 2f;

            if (pos.x < 0)
                return new PointInfo(false); //Out of selection bounds (left side)

            if (GetBoundingBox().x - pos.x < size.x)
                return new PointInfo(false); //Out of selection bounds (right side)

            var column = Mathf.FloorToInt(pos.x / w);
            
            //Find the row
            pos.y -= size.y / 2f;
            if (pos.y < 0)
                return new PointInfo(false); //Out of selection bounds (top side)

            if (GetBoundingBox().y - pos.y < size.y)
                return new PointInfo(false); //Out of selection bounds (bottom side)


            var row = Mathf.FloorToInt(pos.y / h);

            //Get relative coords
            var rx = pos.x - column * w;
            var ry = h - (pos.y - (row - 0.5f) * h);

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
                            new Vector2Int(column, row),
                            new Vector2Int(column, row - 1),
                            new Vector2Int(column + 1, row));
                    }
                    else
                    {
                        if (row + 2 > Height)
                            return new PointInfo(false);
                        //Lower triangle
                        return new PointInfo(isLeftTri: false,
                            new Vector2Int(column, row),
                            new Vector2Int(column + 1, row + 1),
                            new Vector2Int(column, row + 1));
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
                            new Vector2Int(column + 1, row),
                            new Vector2Int(column + 1, row - 1));
                    }
                    else
                    {
                        if (row + 2 > Height)
                            return new PointInfo(false);
                        //Lower triangle
                        return new PointInfo(isLeftTri: true,
                            new Vector2Int(column, row + 1),
                            new Vector2Int(column + 1, row + 1),
                            new Vector2Int(column + 1, row));
                    }
                }
                else
                {
                    if (row + 2 > Height)
                        return new PointInfo(false);
                    //Middle triangle
                    return new PointInfo(isLeftTri: false,
                        new Vector2Int(column, row),
                        new Vector2Int(column + 1, row),
                        new Vector2Int(column, row + 1));
                }
            }
        }
        #endregion
    }
}
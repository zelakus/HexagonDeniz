using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [System.NonSerialized]
        public Hexagon[,] Hexagons;
        [System.NonSerialized]
        public readonly List<BombHexagon> Bombs = new List<BombHexagon>();

        //Resources
        private readonly Dictionary<HexagonType, GameObject> HexObjs = new Dictionary<HexagonType, GameObject>();

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
            HexObjs.Add(HexagonType.Normal, Res.LoadGameObject("Hexagon"));
            HexObjs.Add(HexagonType.Bomb, Res.LoadGameObject("BombHexagon"));
        }

        private void OnEnable()
        {
            //Calculate Params
            size = HexObjs[HexagonType.Normal].GetComponent<RectTransform>().sizeDelta;
            w = 3 * size.x / 4f + Spacing;
            h = size.y + Spacing;
            
            var rect = Content.GetComponent<RectTransform>().rect;
            offset = (new Vector2(rect.width, rect.height) - GetBoundingBox())/2f;
            offset = new Vector2(offset.x, -offset.y);

            //Check if there is a saved game
            if (StatsManager.Instance.HasSave)
            {
                //Load the existing grid
                LoadGrid(StatsManager.Instance.Data);
            }
            else
            {
                //Generate Grid
                GenerateGrid();
                int counter = 0;
                while (counter++ < 50)
                    if (Clear())
                        break;
            }
        }

        private void OnDisable()
        {
            //Clear content
            for (int i = 0; i < Content.childCount; i++)
                Destroy(Content.GetChild(i).gameObject);
            //Clear bombs
            Bombs.Clear();
        }

        private void LoadGrid(SaveData data)
        {
            //TODO: generate it from the save data
        }

        private void GenerateGrid()
        {
            Hexagons = new Hexagon[Width, Height];

            //Loop through whole grid and generate randomly
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    GenerateRandomHexagon(x, y);
        }

        private Hexagon GenerateRandomHexagon(int x, int y)
        {
            //Create & Set Object
            var obj = Instantiate(HexObjs[HexagonType.Normal], Content);
            Hexagons[x, y] = new Hexagon(obj, x, y)
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

            return Hexagons[x,y];
        }

        public void GenerateRandomBomb()
        {
            int x = Random.Range(0, Width);
            int y = Random.Range(0, Height);

            int color = -1;

            var orig = Get(x, y);
            if (orig != null)
            {
                color = orig.Color;
                orig?.Destroy(withVFX: false);
            }

            //Create & Set Object
            var obj = Instantiate(HexObjs[HexagonType.Bomb], Content);
            Hexagons[x, y] = new BombHexagon(obj, x, y)
            {
                //Set Hexagon Type
                HexaType = HexagonType.Bomb
            };

            //Set Position
            Hexagons[x, y].TargetPosition = IndexToPosition(x, y);
            Hexagons[x, y].Reposition();

            //Set Color
            if (color == -1)
                color = Random.Range(0, Colors.Length);
            Hexagons[x, y].Color = color;
            obj.transform.GetChild(0).GetComponent<Image>().color = Colors[color];

            Bombs.Add(Hexagons[x, y] as BombHexagon);
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
            B.Index = a;

            //Set A at B's place
            Hexagons[b.x, b.y] = A;
            //Set new positon
            A.TargetPosition = IndexToPosition(b.x, b.y);
            A.Reposition();
            A.Index = b;
        }

        public Hexagon Get(int x, int y)
        {
            //Check if the coordinates are valid
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;

            return Hexagons[x, y];
        }

        public Hexagon Get(Vector2Int vector)
        {
            return Get(vector.x, vector.y);
        }

        public bool Clear()
        {
            List<Vector2Int> destroyedHexagons = new List<Vector2Int>();

            //Explode neighbour hexagons with same color
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var index = new Vector2Int(x, y); 
                    
                    if (Get(index) == null)
                        continue;

                    var neighbours = GetNeighbours(new List<Vector2Int>() { index });
                    if (neighbours.Count >= 3)
                    {
                        foreach (var neighbour in neighbours)
                            if (!destroyedHexagons.Contains(neighbour))
                                destroyedHexagons.Add(neighbour);
                    }
                }

            //Destroy and recreate each exploded hexagon
            foreach (var hexa in destroyedHexagons)
            {
                //Destroy the hexagon
                Get(hexa).Destroy(withVFX: false);
                Hexagons[hexa.x, hexa.y] = null;
                //Create new one to replace
                GenerateRandomHexagon(hexa.x, hexa.y);
            }

            //Return if we have destroyed any hexagons
            return destroyedHexagons.Count != 0;
        }

        /// <summary>
        /// Used for system generated explosions, won't use any moves
        /// </summary>
        /// <returns>Destroyed hexagon count</returns>
        public int ExplodeHexagons()
        {
            List<Vector2Int> destroyedHexagons = new List<Vector2Int>();

            //Explode neighbour hexagons with same color, loop through everything
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var index = new Vector2Int(x, y);

                    if (Get(index) == null)
                        continue;

                    //Get neighbours for this index
                    var neighbours = GetNeighbours(new List<Vector2Int>() { index });
                    //If we have more than 3, add them to destroy list
                    if (neighbours.Count >= 3)
                    {
                        foreach (var neighbour in neighbours)
                            if (!destroyedHexagons.Contains(neighbour))
                                destroyedHexagons.Add(neighbour);
                    }
                }

            //Explode each exploded hexagon
            foreach (var hexa in destroyedHexagons)
            {
                //Get current neighbour
                var current = Get(hexa);
                //If it was a bomb, remove from list
                if (current.HexaType == HexagonType.Bomb)
                    Bombs.Remove(Bombs.Single(b => b.Index == current.Index));
                //Destroy the hexagon
                current.Destroy();
                Hexagons[hexa.x, hexa.y] = null;
            }

            //Return destroyed hexagons for score calculation
            return destroyedHexagons.Count;
        }

        /// <summary>
        /// Used for player caused explosions, uses moves and ticks bombs
        /// </summary>
        /// <param name="changes">Changed hexagon indices</param>
        /// <returns>Exploded hexagon count, returns -1 if a bomb blows up</returns>
        public int ExplodeHexagons(Vector2Int[] changes)
        {
            List<Vector2Int> destroyedHexagons = new List<Vector2Int>();

            //Explode neighbour hexagons with same color, loop through changes
            foreach (var index in changes)
            {
                if (Get(index) == null)
                    continue;

                //Get neighbours for this index
                var neighbours = GetNeighbours(new List<Vector2Int>() { index });
                //If we have more than 3, add them to destroy list
                if (neighbours.Count >= 3)
                {
                    foreach (var neighbour in neighbours)
                        if (!destroyedHexagons.Contains(neighbour))
                            destroyedHexagons.Add(neighbour);
                }
            }

            //If this was a valid move with explosions, tick the bombs
            if (destroyedHexagons.Count != 0)
            {
                //Tick bombs
                foreach (var bomb in Bombs)
                    if (!destroyedHexagons.Contains(bomb.Index) && bomb.Tick())
                        return -1; //Bomb exploded
            }

            //Explode each exploded hexagon
            foreach (var hexa in destroyedHexagons)
            {
                //Get current neighbour
                var current = Get(hexa);
                //If it was a bomb, remove from list
                if (current.HexaType == HexagonType.Bomb)
                {
                    Bombs.Remove(Bombs.Single(b => b.Index == current.Index));
                }
                //Destroy the hexagon
                current.Destroy();
                Hexagons[hexa.x, hexa.y] = null;
            }

            //Return destroyed hexagons for score calculation
            return destroyedHexagons.Count;
        }

        public IEnumerator Refresh()
        {
            yield return new WaitForSeconds(0.4f); //Just a break to see what's going on

            var droppedHexagons = new List<Vector2Int>();
            for (int x = 0; x < Width; x++)
                for (int y = Height - 1; y >= 0; y--)
                {
                    var current = new Vector2Int(x, y);
                    //If hexagon slot is empty, drop the line
                    if (Get(current) == null)
                    {
                        droppedHexagons.AddRange(DropLine(current));
                        break;
                    }
                }

            //Set target positions
            foreach (var hexa in droppedHexagons)
                Get(hexa).TargetPosition = IndexToPosition(hexa.x, hexa.y);

            //Play drop animation
            float percentage = 0;
            while (percentage != 1f)
            {
                //Increase value till 1
                percentage += Time.deltaTime * 2f;
                if (percentage > 1f)
                    percentage = 1f;

                //Respotion all dropped hexagons with current percantage
                foreach (var hexa in droppedHexagons)
                    Get(hexa).Reposition(percentage);

                //Wait for one frame
                yield return null;
            }

            yield return new WaitForSeconds(0.5f); //Just a break to see what's going on
        }

        public List<Vector2Int> DropLine(Vector2Int root)
        {
            var list = new List<Vector2Int>();

            for (int i = root.y; i >= 0; i--)
            {
                int j;
                //Loop through column to start of it
                for (j = i - 1; j >= 0; j--)
                {
                    var hex = Get(root.x, j);
                    //We have found a valid hexagon above us
                    if (hex != null)
                    {
                        //Place it into (x, i)
                        Hexagons[root.x, i] = hex;
                        hex.Index = new Vector2Int(root.x, i);
                        hex.TargetPosition = IndexToPosition(root.x, i);

                        //Set taken hexagon as null
                        Hexagons[root.x, j] = null;

                        //Since we have filled the null position on (x, i), we can stop searching now
                        break;
                    }
                }

                //Check if search has hit the end of loop (we couldn't find any hexagons above), if so generate new one
                if (j == -1)
                {
                    //Create randomly
                    //Since we are dropping hexagons, place this one higher up
                    GenerateRandomHexagon(root.x, i).PositionOnSpawnPoint();
                }

                //Add current index to dropped hexagons list
                list.Add(new Vector2Int(root.x, i));
            }

            return list;
        }

        public List<Vector2Int> GetNeighbours(List<Vector2Int> hexas)
        {
            var list = new List<Vector2Int>(hexas);
            var rootColor = Get(hexas[0]).Color;

            foreach (var hexa in hexas)
            {
                //Check what kind of column is this hexagon placed on
                if (hexa.x % 2 == 0)
                {
                    //On lower column
                    AddIfColorMatches(hexa + Vector2Int.down, hexa + Vector2Int.right, rootColor, ref list);
                    AddIfColorMatches(hexa + Vector2Int.down, hexa + Vector2Int.left, rootColor, ref list);
                    
                    AddIfColorMatches(hexa + Vector2Int.up, hexa + Vector2Int.up + Vector2Int.right, rootColor, ref list);
                    AddIfColorMatches(hexa + Vector2Int.up, hexa + Vector2Int.up + Vector2Int.left, rootColor, ref list);

                    AddIfColorMatches(hexa + Vector2Int.up + Vector2Int.left, hexa + Vector2Int.left, rootColor, ref list);
                    AddIfColorMatches(hexa + Vector2Int.up + Vector2Int.right, hexa + Vector2Int.right, rootColor, ref list);
                }
                else
                {
                    //On higher/ood column
                    AddIfColorMatches(hexa + Vector2Int.up, hexa + Vector2Int.right, rootColor, ref list);
                    AddIfColorMatches(hexa + Vector2Int.up, hexa + Vector2Int.left, rootColor, ref list);

                    AddIfColorMatches(hexa + Vector2Int.down, hexa + Vector2Int.down + Vector2Int.right, rootColor, ref list);
                    AddIfColorMatches(hexa + Vector2Int.down, hexa + Vector2Int.down + Vector2Int.left, rootColor, ref list);
                    
                    AddIfColorMatches(hexa + Vector2Int.down + Vector2Int.left, hexa + Vector2Int.left, rootColor, ref list);
                    AddIfColorMatches(hexa + Vector2Int.down + Vector2Int.right, hexa + Vector2Int.right, rootColor, ref list);
                }
            }

            return list;
        }

        void AddIfColorMatches(Vector2Int hex1, Vector2Int hex2, int color, ref List<Vector2Int> list)
        {
            //Check if the given color and 2 other hexagon colors match
            if (Get(hex1)?.Color == color && Get(hex2)?.Color == color)
            {
                //Add hexagon 1 to the list if it's not already in
                if (!list.Contains(hex1))
                    list.Add(hex1);
                //Add hexagon 2 to the list if it's not already in
                if (!list.Contains(hex2))
                    list.Add(hex2);
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
                            new Vector2Int(column + 1, row - 1),
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
                        new Vector2Int(column + 1, row),
                        new Vector2Int(column, row + 1));
                }
            }
        }
        #endregion
    }
}
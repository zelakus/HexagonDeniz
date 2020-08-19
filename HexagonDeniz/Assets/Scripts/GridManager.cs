using System.Collections;
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

            //Load Resources
            HexObjNormal = Resources.Load("Hexagon") as GameObject;
        }

        private void Start()
        {
            GenerateGrid();
        }

        private Vector2 IndexToPosition(int x, int y)
        {
            return new Vector2(25 + x * 25, -25 - y * 25);
        }

        private void GenerateGrid()
        {
            Hexagons = new Hexagon[Width, Height];
            for (int x=0;x<Width;x++)
                for (int y=0;y<Height;y++)
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
    }
}
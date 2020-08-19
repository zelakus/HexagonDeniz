﻿using System.Collections;
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

            //Load Resources
            HexObjNormal = Resources.Load("Hexagon") as GameObject;
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

        Vector2 offset, size;
        float w, h;
        public Vector2 GetBoundingBox()
        {
            return new Vector2(size.x / 4f - Spacing * 2 + w * Width, h * (Height + 0.5f));
        }

        private Vector2 IndexToPosition(int x, int y)
        {
            var yOffset = size.y / 2f;
            if (x % 2 == 0)
                yOffset = yOffset * 2 + Spacing / 2f;

            return new Vector2(offset.x + size.x / 2f + x * w,
                offset.y - yOffset- y * h);
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
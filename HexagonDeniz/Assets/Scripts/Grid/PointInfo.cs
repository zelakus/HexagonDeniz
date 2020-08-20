using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexDeniz
{
    public struct PointInfo
    {
        public bool InArea;
        public Vector2 MiddlePoint;
        public bool LeftTriangle;
        public Vector2Int[] Hexagons;

        public PointInfo(bool isLeftTri, params Vector2Int[] hexagons)
        {
            InArea = true;
            Hexagons = hexagons;
            LeftTriangle = isLeftTri;
            MiddlePoint = Vector2.zero;
            //Calculate mid point
            foreach (var hex in hexagons)
                MiddlePoint += GridManager.Instance.IndexToPosition(hex.x, hex.y);
            MiddlePoint /= hexagons.Length;
        }

        public PointInfo(bool inArea)
        {
            InArea = inArea;
            Hexagons = new Vector2Int[0];
            LeftTriangle = true;
            MiddlePoint = Vector2.zero;
        }
    }
}
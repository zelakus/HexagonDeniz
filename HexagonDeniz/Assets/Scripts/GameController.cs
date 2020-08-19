﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexDeniz
{
    public class GameController : MonoBehaviour
    {
        void Update()
        {
            RectTransform rect = GetComponent<RectTransform>();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var point = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (!RectTransformUtility.RectangleContainsScreenPoint(rect, point))
                    return;

                var localPoint = rect.InverseTransformPoint(point);

                var hexas = GridManager.Instance.PositionToIndices(localPoint.x, rect.rect.height - localPoint.y);
                //Debug.Log($"Found {hexas.Count} hexagons");
                foreach (var hex in hexas)
                {
                    var rect2 = (GameObject.Instantiate(Resources.Load("test"), GridManager.Instance.Content) as GameObject).GetComponent<RectTransform>();
                    rect2.anchoredPosition =  GridManager.Instance.IndexToPosition(hex.x, hex.y);
                }
            }    
        }
    }
}
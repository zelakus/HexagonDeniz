using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexDeniz
{
    public class GameController : MonoBehaviour
    {
        RectTransform rect;
        PointInfo LastSelection;
        RectTransform SelectionObj;

        //Resources
        private GameObject TriLeft, TriRight;

        private void Awake()
        {
            LastSelection = new PointInfo(false);
            rect = GetComponent<RectTransform>();

            //Load resources
            TriLeft = Res.LoadGameObject("TriLeft");
            TriRight = Res.LoadGameObject("TriRight");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var point = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Select(point);
            }    
        }

        private void Select(Vector2 point)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(rect, point))
                return;

            var localPoint = rect.InverseTransformPoint(point);

            var info = GridManager.Instance.GetPositionInfo(localPoint.x, rect.rect.height - localPoint.y);
            Select(info);
        }

        private void Select(PointInfo info)
        {
            //Delete old selection obj
            if (SelectionObj != null)
                Destroy(SelectionObj.gameObject);

            LastSelection = info;

            //Return if we are out of area
            if (!info.InArea)
                return;

            //Create & position selection shape
            if (info.LeftTriangle)
                SelectionObj = Instantiate(TriLeft, GridManager.Instance.Content).GetComponent<RectTransform>();
            else
                SelectionObj = Instantiate(TriRight, GridManager.Instance.Content).GetComponent<RectTransform>();
            SelectionObj.anchoredPosition = info.MiddlePoint;
        }
    }
}
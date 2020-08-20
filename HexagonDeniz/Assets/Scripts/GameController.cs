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
            HandleInputs();
        }

        Vector2 clickStart;
        void HandleInputs()
        {
            //Get point & check if in rect
            var point = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (!RectTransformUtility.RectangleContainsScreenPoint(rect, point))
                return;

            //Convert point to rect's local
            var localPoint = rect.InverseTransformPoint(point);

            //Up & down events
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //Fix localpoint and set as start point (unity coordinate system difference)
                clickStart = new Vector2(localPoint.x, rect.rect.height - localPoint.y);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                //If we have no selection, select it no matter the distance
                if (!LastSelection.InArea)
                {
                    var info = GridManager.Instance.GetPositionInfo(localPoint.x, rect.rect.height - localPoint.y);
                    Select(info);
                    return;
                }

                //Fix localpoint (unity coordinate system difference)
                localPoint = new Vector2(localPoint.x, rect.rect.height - localPoint.y);

                //Check if movement distance is enough to rotate
                var dist = (clickStart - point).magnitude / Mathf.Min(Screen.width, Screen.height);
                if (dist>0.05f)
                {
                    var angleDelta = Mathf.DeltaAngle((clickStart.NegateY() - LastSelection.MiddlePoint).Angle(),
                        (new Vector2(localPoint.x, -localPoint.y) - LastSelection.MiddlePoint).Angle());

                    if (angleDelta > 0)
                        Debug.Log("CounterClockwise");
                    else
                        Debug.Log("Clockwise");
                    //TODO: Rotate
                }
            }
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
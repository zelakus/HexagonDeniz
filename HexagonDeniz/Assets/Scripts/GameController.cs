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

        public uint BombSpawnScore = 1000;

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

        uint lastK = 0;
        private void OnEnable()
        {
            rotating = false;
            lastK = StatsManager.Instance.Score / BombSpawnScore;
        }

        void Update()
        {
            HandleInputs();
        }

        Vector2 clickStart;
        void HandleInputs()
        {
            //Don't accept inputs while we are rotating
            if (rotating)
                return;

            //Get point & check if in rect
            var point = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (!RectTransformUtility.RectangleContainsScreenPoint(rect, point))
                return;

            //Convert point to rect's local
            var rawLocalPoint = rect.InverseTransformPoint(point);

            //Fix localpoint and set as start point(unity coordinate system difference)
            var localPoint = new Vector2(rawLocalPoint.x, rect.rect.height - rawLocalPoint.y);

            //Up & down events
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //Fix localpoint and set as start point (unity coordinate system difference)
                clickStart = localPoint;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                //If we have no selection, select it no matter the distance
                if (!LastSelection.InArea)
                {
                    var info = GridManager.Instance.GetPositionInfo(localPoint);
                    Select(info);
                    return;
                }

                //Check if movement distance is enough to rotate
                var dist = (clickStart - localPoint).magnitude / Mathf.Min(Screen.width, Screen.height); //Divide by screen size
                if (dist >= 0.01f) //Min move threshold
                {
                    //Calculate angle
                    var angleDelta = Mathf.DeltaAngle((clickStart.NegateY() - LastSelection.MiddlePoint).Angle(),
                        (new Vector2(localPoint.x, -localPoint.y) - LastSelection.MiddlePoint).Angle());

                    StartCoroutine(Rotate(angleDelta < 0));
                }
                else
                {
                    //Select
                    var info = GridManager.Instance.GetPositionInfo(localPoint);
                    Select(info);
                }
            }
        }

        bool rotating = false;
        IEnumerator Rotate(bool isClockwise)
        {
            //Set us as rotating so that user input can be blocked during animation
            rotating = true;
            
            //Set direction
            float dir = 1;
            if (isClockwise)
                dir = -1f;

            //Rotate 3 times, if nothing happens we will be back to start
            for (int i = 0; i < 3; i++)
            {
                float rotation = 0;
                while (rotation < 120)
                {
                    //Rotate
                    SetRotation(rotation * dir);

                    //Increase and continue
                    rotation += 120f * Time.deltaTime * 5;
                    yield return null;
                }
                SetRotation(120 * dir);
                //Change pieces in array
                if (isClockwise)
                {
                    GridManager.Instance.Replace(LastSelection.Hexagons[0], LastSelection.Hexagons[1]);
                    GridManager.Instance.Replace(LastSelection.Hexagons[0], LastSelection.Hexagons[2]);
                }
                else
                {
                    GridManager.Instance.Replace(LastSelection.Hexagons[0], LastSelection.Hexagons[2]);
                    GridManager.Instance.Replace(LastSelection.Hexagons[0], LastSelection.Hexagons[1]);
                }

                //Get results from move
                var result = GridManager.Instance.ExplodeHexagons(LastSelection.Hexagons);
                if (result == 0)
                {
                    //Nothing exploded, continue
                    continue;
                }
                else if (result == -1)
                {
                    //Destroy selection object
                    Destroy(SelectionObj.gameObject);
                    //Game end
                    MainMenu.Instance.ShowMenu();
                    StatsManager.Instance.ClearGame();
                    MessageBox.Show("The end!", "A bomb blew up!\nThe end.");
                    SoundEffectManager.Instance.Play(SoundEffects.GameEnd);
                }
                else
                {
                    //Destroy selection object
                    Destroy(SelectionObj.gameObject);
                    //Change session stats
                    StatsManager.Instance.AddMove();
                    StatsManager.Instance.AddScore((uint)result * 5); //Given score per exploded block is 5
                    
                    //Generate and drop hexagons
                    yield return StartCoroutine(GridManager.Instance.Refresh());

                    //Check if there are hexagons to explode due to new spawns
                    int counter = 0;
                    while (counter++ < 10) //Limit max system explosions to 10
                    {
                        //System explosions
                        var sysResult = GridManager.Instance.ExplodeHexagons();
                        StatsManager.Instance.AddScore((uint)sysResult * 5); //Given score per exploded block is 5

                        //If nothing changed, we can quit this loop
                        if (sysResult == 0)
                            break;

                        //Drop new ones
                        yield return StartCoroutine(GridManager.Instance.Refresh());
                    }
                    //Generate bomb if needed
                    if (StatsManager.Instance.Score / BombSpawnScore != lastK)
                    {
                        GridManager.Instance.GenerateRandomBomb();
                        lastK = StatsManager.Instance.Score / BombSpawnScore;
                    }

                    //Save grid data
                    StatsManager.Instance.SaveGame();

                    //Player have a successful move, we can stop rotating
                    break;
                }
            }

            //End rotating to enable user input
            rotating = false;
        }

        private void SetRotation(float value)
        {
            var rot = Quaternion.Euler(0, 0, value);
            SelectionObj.localRotation = rot;

            foreach (var hexInfo in LastSelection.Hexagons)
            {
                var hex = GridManager.Instance.Get(hexInfo);
                hex.Rect.localRotation = rot;
                hex.Rect.anchoredPosition = LastSelection.MiddlePoint.ToVector3() + rot * (hex.TargetPosition.ToVector3() - LastSelection.MiddlePoint.ToVector3());
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

            //Exclude spacing from mid point
            info.MiddlePoint += new Vector2(-1, 1) * GridManager.Instance.Spacing;

            //Create & position selection shape
            if (info.LeftTriangle)
                SelectionObj = Instantiate(TriLeft, GridManager.Instance.Content).GetComponent<RectTransform>();
            else
                SelectionObj = Instantiate(TriRight, GridManager.Instance.Content).GetComponent<RectTransform>();
            SelectionObj.anchoredPosition = info.MiddlePoint;
        }
    }
}
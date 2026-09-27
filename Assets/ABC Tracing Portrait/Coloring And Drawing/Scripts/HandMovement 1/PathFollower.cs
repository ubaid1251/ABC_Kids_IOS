using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace KGF.Coloring
{
    public class PathFollower : MonoBehaviour
    {
        public PaintistaManager paintistaManager;
        private int currentShapeIndex = -1;
        private GameObject handSprite;
        private bool moveToCalled = false;

        public Transform[] waypointArray;
        public float percentsPerSecond = 2f; // %2 of the path moved per second
        //float currentPathPercent = 0.0f; //min 0, max 1

        //float pathVelocity;
        float pathLength = 0;

        void Start()
        {

            paintistaManager = GameObject.Find("PaintistaManager").GetComponent<PaintistaManager>();
            handSprite = this.transform.GetChild(0).gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (currentShapeIndex != paintistaManager.ShapeIndex)
            {
                currentShapeIndex = paintistaManager.ShapeIndex;
                paintistaManager.timeWithoutTouch = 0;
            }
            else if (currentShapeIndex >= 0)
            {
                if (paintistaManager.timeWithoutTouch > 3 &&
                    //paintistaManager.skeletonAnimation.state.GetCurrent(0) == null &&
                    paintistaManager.targetCamPosition == paintistaManager.CurrentCam.transform.position &&
                    paintistaManager.targetCamSize == paintistaManager.CurrentCam.orthographicSize)
                {
                    CallMoveToAnim();
                }
                else
                {
                    handSprite.SetActive(false);
                    moveToCalled = false;
                }
            }
        }
        public void CallMoveToAnim()
        {
            if (!moveToCalled)
            {
                if (paintistaManager.path != null)
                {
                    if (paintistaManager.path.Length > 0)
                    {
                        pathLength = iTween.PathLength(paintistaManager.path);

                        Debug.Log("Length of Path &&&" + paintistaManager.path.Length);

                        handSprite.SetActive(true);
                        gameObject.transform.position = paintistaManager.path[0];
                        if (pathLength < 5f)
                        {
                            gameObject.transform.DOPath(paintistaManager.path, pathLength * 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                            {
                                paintistaManager.timeWithoutTouch = 0;
                                handSprite.SetActive(false);
                            });

                        }
                        else
                        {
                            gameObject.transform.DOPath(paintistaManager.path, pathLength * 0.2f).SetEase(Ease.Linear).OnComplete(() =>
                            {
                                paintistaManager.timeWithoutTouch = 0;
                                handSprite.SetActive(false);
                            });
                        }
                    }
                    moveToCalled = true;
                }
            }
        }

    }
}
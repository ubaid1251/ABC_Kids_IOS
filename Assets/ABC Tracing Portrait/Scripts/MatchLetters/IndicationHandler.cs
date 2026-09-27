using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class IndicationHandler : MonoBehaviour
{
    public List<RectTransform> allIndi;
    public static IndicationHandler Instance;

    public float timeSinceLastInput = 0f;
    private float indicationDuration = 3f; // Duration to show each indication
    private float waitDuration = 3f;       // Duration to wait for user input
    private bool isIndicating = false;
    private RectTransform currentIndication = null;

    private void Start()
    {
        Instance = this;
        if (allIndi == null || allIndi.Count == 0)
        {
            // Debug.LogError("allIndi list is empty or not set.");
            return;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0)) // If the user is touching or clicking
        {
            timeSinceLastInput = 0f;
            isIndicating = false;
            if (currentIndication != null)
            {
                currentIndication.gameObject.SetActive(false);
                currentIndication = null;
            }
        }
        else
        {
            timeSinceLastInput += Time.deltaTime;

            if (timeSinceLastInput >= waitDuration && !isIndicating)
            {
                // Pick a random RectTransform from the list
                if (allIndi.Count > 0)
                {
                    int randomIndex = Random.Range(0, allIndi.Count);
                    RectTransform randomIndication = allIndi[randomIndex];

                    // Set the picked RectTransform to active
                    if (randomIndication.transform.parent
                            .GetComponent<DragAlpha>().moveTo != null)
                    {
                        randomIndication.gameObject.SetActive(true);
                        currentIndication = randomIndication;
                        randomIndication.DOMove(randomIndication.transform.parent
                                .GetComponent<DragAlpha>().moveTo.position, 1f) /*.SetLoops(3,LoopType.Restart)*/
                            .OnComplete(() =>
                            {
                                randomIndication.gameObject.SetActive(false);
                                randomIndication.DOAnchorPos(new Vector3(204, -236, 0), 0);
                            });
                    }

                    isIndicating = true;
                    timeSinceLastInput = 0f; // Reset timer
                }
            }

            // Deactivate the indication after the duration
            if (isIndicating && timeSinceLastInput >= indicationDuration)
            {
                if (currentIndication != null)
                {
                    DOTween.Kill(currentIndication, false);
                    currentIndication.DOAnchorPos(new Vector3(204, -236, 0), 0);
                    currentIndication.gameObject.SetActive(false);
                    currentIndication = null;
                }

                timeSinceLastInput = 0f;
                isIndicating = false;
            }
        }
    }
}

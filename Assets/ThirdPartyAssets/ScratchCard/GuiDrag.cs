using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class GuiDrag : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public RectTransform rectTransform;
    public Canvas Canvas;
    Vector2 initialPosition;

    public bool isGragalb;
    public GameObject Indicator;
    public Transform Destination;

    public float LeftAngle;
    public float rightAngle;
    public bool isRotateToScreen;
    public float angleOffset;
    private Vector3 screenPos;
    private void Start()
    {

        rectTransform = GetComponent<RectTransform>();
        initialPosition = Destination.localPosition;
    }

    public void ResetInditialPosition()
    {
        initialPosition = Destination.position;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isGragalb)
        {
            if (Indicator != null)
                Indicator.SetActive(false);
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isGragalb)
        {
            rectTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;

            if (isRotateToScreen) 
            {
              
                if (transform.localPosition.x<-100f)
                {
                    transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(transform.eulerAngles.z, LeftAngle, 5 * Time.deltaTime));
                }
                else if (transform.localPosition.x >100f)
                {
                    transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(transform.eulerAngles.z,rightAngle, 5 * Time.deltaTime));
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        resetPos();
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }


    public void resetPos()
    {
        if (isGragalb)
        {
            rectTransform.anchoredPosition = initialPosition;
            if (Indicator != null)
                Indicator.SetActive(true);
        }
    }


    internal void moveToolFun() 
    {
      
    }

    internal void moveToolFun2()
    {
      
    }

    void afterReacedFun() 
    {
     
    }


}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Eraser : MonoBehaviour, IDragHandler, IEndDragHandler,IPointerDownHandler,IPointerUpHandler
{
    private RectTransform rectTransform;
    public Transform p;
    public GameObject maskPrefab; // Reference to the mask prefab
    public Canvas canvas;
    Collider2D mycol;
    Vector2 pos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        pos = rectTransform.anchoredPosition;
        mycol = GetComponent<Collider2D>();
        canvas = GetComponent<Canvas>();
    }

    
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Vector3 mousePosition = rectTransform.position;

        GameObject newMask = Instantiate(maskPrefab, p);
        newMask.transform.position = mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mycol.enabled = false;
        print("da");
        for (int i = 0; i < p.transform.childCount; i++)
        {
            Destroy(p.transform.GetChild(i).gameObject);
        }
        rectTransform.DOAnchorPos(pos, 1);
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ABCManager.instance.SetEraseEnvOn();
        mycol.enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ABCManager.instance.SetEraseEnvOff();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SideItemsClick : MonoBehaviour, IPointerDownHandler
{
    RectTransform myRect;
    public bool pos, rot, scale;
    public float strength, randomness;
    public int vibrateTo;

    private void Start()
    {
        myRect = GetComponent<RectTransform>();
    }
    bool isP = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isP == false)
        {
            Vibration.Vibrate(50);
            isP = true;
            if (pos)
            {
                myRect.DOShakeAnchorPos(.5f,strength, vibrateTo,randomness).OnComplete(() =>
                {
                    isP = false;
                });
            }
            if (rot)
            {
                myRect.DOShakeRotation(.5f, strength, vibrateTo, randomness).OnComplete(() =>
                {
                    isP = false;
                });
            }
            if (scale)
            {
                myRect.DOShakeScale(.5f, strength, vibrateTo, randomness).OnComplete(() =>
                {
                    isP = false;
                });
            }
            if(!pos&&!rot&&!scale) 
            {
                isP = false;
            }
        }
    }
}

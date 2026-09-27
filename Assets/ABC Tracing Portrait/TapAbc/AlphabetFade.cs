using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AlphabetFade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.DOScale(0.7f, .5f).SetEase(Ease.Linear);
        GetComponent<Image>().DOFade(1, .1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            GetComponent<Image>().DOFade(0, .5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
       
    }
}
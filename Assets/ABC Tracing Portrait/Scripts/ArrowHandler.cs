using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHandler : MonoBehaviour
{
    public GameObject[] ArrowAppear;
    public GameObject arrow;
    int i = 0;
    public float t=.1f;
    private void Start()
    {
        ArrowAppear[i].SetActive(true);
        i++;
        Invoke(nameof(appearArrow),t);
    }
    void appearArrow()
    {
        if(i< ArrowAppear.Length)
        {
            ArrowAppear[i].SetActive(true);
            i++;
            Invoke(nameof(appearArrow), t);
        }
        else
        {
            if (LetterHandler.canShowIndiNow)
                arrow.SetActive(true);
        }
    }
}

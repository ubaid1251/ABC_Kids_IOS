using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ScratchCardAsset;
public class ColoringObj : MonoBehaviour
{
    public List<Vector3> path;
    SpriteRenderer s;
    Vector3 pos;
    public EraseProgress progreess;
    private void Start()
    {
        s = GetComponent<SpriteRenderer>();
        pos = transform.localPosition;
    }
    private void OnEnable()
    {
        Invoke(nameof(Indi), 2);
        //print("0");
    }
    public void Indi()
    {
        //print("12");
        s.enabled = true;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.DOLocalMove(path[0], .65f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.DOLocalMove(path[1], .65f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                transform.DOLocalMove(path[2], .5f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    s.enabled = false;
                    transform.GetChild(0).gameObject.SetActive(false);
                    transform.DOLocalMove(pos, 0);
                    //print("01");
                    Invoke(nameof(Indi), 2);
                });
            });
        });
    }
    private void Update()
    {
        if (progreess.isCompleted == true)
        {
            gameObject.SetActive(false);
        }
        if (Input.GetMouseButtonDown(0))
        {
            //print("1");
            DOTween.KillAll(false);
            s.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.DOLocalMove(pos, 0);
            CancelInvoke(nameof(Indi));
        }
        else if (Input.GetMouseButtonUp(0)&&!progreess.isCompleted)
        {
            //print("02");
            Invoke(nameof(Indi), 2);
        }
        else if(progreess.isCompleted&&s.enabled==true)
        {
            //print("3");
            //DOTween.KillAll(false);
            s.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.DOLocalMove(pos, 0);
            CancelInvoke(nameof(Indi));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AdditionParentalPanel : MonoBehaviour
{
    public GameObject lhs, rhs, ans;
    public Sprite[] imges;
    public Sprite[] ansImg;
    public GameObject activeObject;
    public GameObject DeativeWrong;
    int answer;
    public AudioSource right;
    public AudioSource rong;
    public Button[] buttons;
    //public bool buy = false;
    //public InAppCalling_CB buy;
    private void OnEnable()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].enabled = true;
        }
        answer = 0;
        ans.SetActive(false);
        int randvalue = Random.Range(0, 6);
        lhs.GetComponent<Image>().sprite = imges[randvalue];
        lhs.GetComponent<Image>().SetNativeSize();
        answer = randvalue + (answer + 1);

        randvalue = Random.Range(0, 3);
        rhs.GetComponent<Image>().sprite = imges[randvalue];
        rhs.GetComponent<Image>().SetNativeSize();
        answer = randvalue + (1 + answer);
        print(answer);
    }

    public void CheckAnswer(int val)
    {
        Vibration.Vibrate(50);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].enabled = false;
        }
        ans.GetComponent<Image>().sprite = ansImg[val - 1];
        ans.GetComponent<Image>().SetNativeSize();
        ans.SetActive(true);
        if (answer == val)
        {
            if (right.enabled)
                right.Play();
            ans.GetComponent<Image>().color = Color.green;
        }
        else
        {
            if (rong.enabled)
                rong.Play();
            ans.GetComponent<Image>().color = Color.red;
        }
        StartCoroutine(AnimAnswer(val));
    }

    IEnumerator AnimAnswer(int val)
    {

        yield return new WaitForSeconds(0.8f);
        if (answer == val)
        {

            if (activeObject)
            {
                activeObject.SetActive(true);

            }
            //else if (buy)
            //{
            //    buy.BuyInApp();
            //    //InappPanel.Instance.ShowRemoveAdsPanel();
            //}

        }
        else
        {

            print("No Correct");

        }

        if (DeativeWrong)
        {
            DeativeWrong.GetComponent<Animator>().Rebind();
            if (answer == val)
                DeativeWrong.SetActive(false);
            else
                PanelOut();

        }
    }

    public void DeativeObj(GameObject obj)
    {

        obj.SetActive(false);
    }
    public void PanelOut()
    {
        Vibration.Vibrate(50);
        SoundHandler.instance.PlaySource(SoundHandler.instance.mySource.clip);
        GetComponent<Animator>().Play("PanelOut");
        Invoke(nameof(Deactive), 0.9f);
    }
   
    public void Deactive()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            Intitializeabc.instance.ShowBanner();//remove later
        }
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {

        DeativeWrong.GetComponent<Animator>().Rebind();
    }
    public void PlayAudio()
    {
        AudioSource s = transform.GetChild(0).GetComponent<AudioSource>();
        if (s.enabled)
        {
            s.Play();
        }
    }
}

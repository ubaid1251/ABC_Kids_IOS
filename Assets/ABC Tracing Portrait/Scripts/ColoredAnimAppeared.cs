using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColoredAnimAppeared : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<AudioSource>())
        {
            AudioSource c = GetComponent<AudioSource>();
            if (PlayerPrefs.GetInt("sfx")==0)
            {
                c.Play();
            }
            else
            {
                c.enabled = false;
            }
            Invoke(nameof(ShowBalloon), c.clip.length);
            //ABCManager.instance.title.gameObject.SetActive(true);
        }
    }
    void ShowBalloon()
    {
        ABCManager.instance.balloonPanel.SetActive(true);
    }
    
}

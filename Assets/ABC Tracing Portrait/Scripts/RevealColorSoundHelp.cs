using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealColorSoundHelp : MonoBehaviour
{
    public AudioSource[] sourcesToPlay;
    private void Start()
    {
        if (PlayerPrefs.GetInt("sfx") == 1)
        {
            for (int i = 0; i < sourcesToPlay.Length; i++)
            {
                sourcesToPlay[i].enabled = false;
            }
        }

    }
    public void Play()
    {
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            for (int i = 0; i < sourcesToPlay.Length; i++)
            {
                sourcesToPlay[i].Play();
            }
            Invoke(nameof(StopAll), 2);
        }
    }
    void StopAll()
    {

        for (int i = 0; i < sourcesToPlay.Length; i++)
        {
            sourcesToPlay[i].Stop();
        }
    }
}

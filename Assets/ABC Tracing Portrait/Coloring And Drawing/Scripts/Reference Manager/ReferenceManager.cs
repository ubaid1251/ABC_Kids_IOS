//using DanielLochner.Assets.SimpleScrollSnap;
using KGF.Coloring;
using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    public GameObject DrawOrAnimateCanvas;
    public GameObject MainCanvas;
    public AudioSource SelectionSceneAudioSource;


    public AudioSource SelectionSceneAudio
    {
        get
        {
            if(SelectionSceneAudioSource == null)
            {
                //SelectionSceneAudioSource = FindObjectOfType<SelectedCharacter>().audioSourceBG;
            }
            return SelectionSceneAudioSource;
        }
    }
    public GameObject noAdsBtn;
    public GameObject RewardedPopUp;
    
}

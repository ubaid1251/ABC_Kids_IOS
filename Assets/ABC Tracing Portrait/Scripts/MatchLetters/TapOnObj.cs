using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TapOnObj : MonoBehaviour, IPointerDownHandler
{
    AudioSource m_AudioSource;
    private void Start()
    {
        m_AudioSource=GetComponent<AudioSource>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(m_AudioSource.clip!=null&& !m_AudioSource.isPlaying&&m_AudioSource.enabled)
            m_AudioSource.Play();
    }
}

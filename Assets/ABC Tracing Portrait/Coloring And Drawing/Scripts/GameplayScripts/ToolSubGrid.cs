using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KGF.Coloring;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ToolSubGrid : MonoBehaviour
{
    public GameObject[] child;
    int _actualTargetIndex = 0;
    public Color32 paintColor = new Color32(255, 0, 0, 255);
    private void OnEnable()
    {
        ToolGrid.FromEnable = true;
        child[0].GetComponent<Button>().onClick.Invoke();
    }

    public void OnClick(GameObject targetObj)
    {
        if (!SoundHandler.instance.mySource.isPlaying)
        {
            Debug.Log("Sounddddddddddd");
            SoundHandler.instance.PlayTap();
        }

        switch (Handler.MyHandler.Inner[0].drawMode)
        {
            case PaintistaManager.DrawMode.CustomBrush:
            case PaintistaManager.DrawMode.Default:
            case PaintistaManager.DrawMode.FloodFill:
            {
                foreach (var t in Handler.MyHandler.Inner)
                {
                    t.SelectColor(targetObj);
                }

                break;
            }
            case PaintistaManager.DrawMode.Sticker:
            {
                _actualTargetIndex = Array.IndexOf(child, targetObj);
                foreach (var t in Handler.MyHandler.Inner)
                {
                    t.SetCurrentStickerIndex(_actualTargetIndex);
                    Debug.Log(_actualTargetIndex + "Sticker index");
                }

                break;
            }
            case PaintistaManager.DrawMode.Pattern:
            case PaintistaManager.DrawMode.FloodFillPattern:
            {
                _actualTargetIndex = Array.IndexOf(child, targetObj);
                foreach (var t in Handler.MyHandler.Inner)
                {
                    StartCoroutine(t.SetCustomPatternCoroutine(_actualTargetIndex));
                }

                break;
            }
        }
    }
    public void OnClickFloodFill(GameObject targetObj)
    {
        if (!SoundHandler.instance.mySource.isPlaying)
        {
            Debug.Log("Sounddddddddddd");
            SoundHandler.instance.PlayTap();
        }

        Debug.Log("We are late");
        paintColor = targetObj.name switch
        {
            "Yellow" => new Color32(244, 235, 2, 255),
            "Red" => new Color32(255, 0, 0, 255),
            "Purple" => new Color32(171,  26, 255, 255),
            "Orange" => new Color32(255, 148, 16, 255),
            "Green" => new Color32(61, 214, 13, 255),
            "Black" =>new Color32(119, 119, 119, 255),
            "Blue" =>  new Color32(2, 138, 255, 255),
            "Cyan" => new Color32(0, 255, 255, 255),
            "Pink" => new Color32(254, 0, 136, 255),
            "Brown" => new Color32(199, 112, 20, 255),
            _ => paintColor
        };

        PlayerPrefs.SetString("FloodFillModeColor",
            paintColor.r + "," + paintColor.g + "," + paintColor.b + "," + targetObj.name);
        foreach (var t in Handler.MyHandler.Inner)
        {
            t.paintColor = paintColor;
        }

        if (Handler.MyHandler.Inner[0].drawMode == PaintistaManager.DrawMode.FloodFill) return;
        {
            foreach (var t in Handler.MyHandler.Inner)
            {
                t.SetDrawMode(2, true);
            }
        }
    }
    
    public void OnClickRainbowFloodFill(GameObject targetObj)
    {
        if (!SoundHandler.instance.mySource.isPlaying)
        {
            SoundHandler.instance.PlayTap();
        }
        foreach (var t in Handler.MyHandler.Inner)
        {
            // PlayerPrefs.SetString("DefaultModeColor", targetObj.name);
            t.SetDrawMode(6);
        }
    }
    
    
}

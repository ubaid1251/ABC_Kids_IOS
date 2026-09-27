using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using KGF.Coloring;

public class CenterGameObjectOnImages : MonoBehaviour
{
    
    public void SetImage()
    {
        Debug.Log("Name: " + ServiceManager.instance.selectedCharacter.CharacterBeenDrawn.name);
        transform.GetChild(0).GetComponent<RawImage>().texture = LoadTheCharacter(ServiceManager.instance.selectedCharacter.CharacterBeenDrawn.name );
        if(transform.GetChild(0).GetComponent<RawImage>().texture)
        {
            transform.GetChild(0).GetComponent<RawImage>().SetNativeSize();
        }
    }

    public Texture2D LoadTheCharacter(string PrefName)
    {
        {
            var localPath = Application.persistentDataPath + "/Assets/Outputs/" + PrefName + "(Clone).png";
            print(localPath);
            if (File.Exists(localPath))
            {
                Texture2D mtexture = new Texture2D(10, 10);
                 mtexture.LoadImage(File.ReadAllBytes(localPath));
                return mtexture;
            }
        }
        return null;

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KGF.Coloring;

public class LoadAllCharacters : MonoBehaviour
{
    RawImage Holder;
    public List<Texture2D> AllCharactersTexture;
    public List<CharacterSelection> AllCharacters;
    public static LoadAllCharacters instance;
   public List<string> PrefsToCheck;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    public void LoadCharacters()
    {
        // print(Loading.cameFrom);
        if(AllCharactersTexture.Count ==0)
        {
            foreach (var child in AllCharacters)
            {
                var childz = child.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<RawImage>();

                Holder = childz;

                if (PlayerPrefs.GetInt(child.PrefName) == 1)
                {
                    child.LoadTheCharacter(Holder,AllCharactersTexture,PrefsToCheck);
                }
                else
                {
                    Holder.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (var child in AllCharacters)
            {
                if (child == null)
                    print("Turja");
                print(child.name+"dadad");
                var childz = child.transform.GetChild(0).transform.GetChild(0).GetComponent<RawImage>();

                Holder = childz;
                int i = findIndex(child);
                if (PlayerPrefs.GetInt(child.PrefName) == 1&& i!=-1)
                {
                    print(i + AllCharactersTexture.Count);
                    Holder.GetComponent<RawImage>().texture = AllCharactersTexture[findIndex(child)];
                    Holder.gameObject.SetActive(true);
                }
                else if (PlayerPrefs.GetInt(child.PrefName) == 1&&i==-1)
                {
                    child.LoadTheCharacter(Holder, AllCharactersTexture,PrefsToCheck);
                }
                else
                {
                    Holder.gameObject.SetActive(false);
                }
            }
        }
    }
    int findIndex(CharacterSelection c)
    {
        for (int i = 0; i < AllCharactersTexture.Count; i++)
        {
            if (PrefsToCheck[i] == c.PrefName)
            {
                print(i+"Indewx");
                return i;
            }
        }
        print(-1 + "Indewx");
        return -1;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

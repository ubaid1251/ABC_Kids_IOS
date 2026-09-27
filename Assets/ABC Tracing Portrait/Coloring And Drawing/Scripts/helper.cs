using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KGF.Coloring;

public class helper : MonoBehaviour
{
    public List<CharacterSelection> AllCharacters;
    
    private void Start()
    {
        Debug.Log("Load Characters");
        LoadAllCharacters.instance.AllCharacters = AllCharacters;
        LoadAllCharacters.instance.LoadCharacters();
    }
}                       
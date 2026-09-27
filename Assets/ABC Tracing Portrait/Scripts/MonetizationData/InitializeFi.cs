using System;
using System.Collections;
using UnityEngine;

public class InitializeFi : MonoBehaviour
{
    public static InitializeFi _Instance;
  
    private void Start()
    {
        _Instance = this;
        DontDestroyOnLoad(gameObject);
    }
  
    public void LogFi()
    {


    }
    
}
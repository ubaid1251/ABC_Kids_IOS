using System;
using UnityEngine;
public enum ResType
{
    iphone6,
    iphonex,
    tab
}
public static class ResCheck
{
    private static bool _initialized = false;
    private static float _aspect;
    private static ResType _resType;

    public static float Aspect
    {
        get
        {
            InitIfNeeded();
            return _aspect;
        }
    }

    public static ResType ResolutionType
    {
        get
        {
            InitIfNeeded();
            return _resType;
        }
    }

    private static void InitIfNeeded()
    {
        if (_initialized) return;

        float value = (float)Screen.height / Screen.width; // portrait
        value = (float)Math.Round(value, 2);

        _aspect = value;

        if (value < 1.45f || Mathf.Approximately(value, 1.6f))
        {
            _resType = ResType.tab;
        }
        else if (Mathf.Approximately(value, 1.78f))
        {
            _resType = ResType.iphone6;
        }
        else if (value >= 2.0f)
        {
            _resType = ResType.iphonex;
        }

        Debug.Log($"[ResCheck] Aspect: {_aspect}, Type: {_resType}");

        _initialized = true;
    }
}

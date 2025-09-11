using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class RetroPostProcess : VolumeComponent
{
    [Header("Pixelization")]
    [Tooltip("Affects the pixel count")]
    public FloatParameter pixelSize = new ClampedFloatParameter(0f, 0f, 20f);
    [Tooltip("Dithering level")]
    public IntParameter bayerLevel = new ClampedIntParameter(0, -1, 2);
    [Header("Posterization")]
    public IntParameter redColourCount = new ClampedIntParameter(25, 0, 256);
    public IntParameter greenColourCount = new ClampedIntParameter(25, 0, 256);
    public IntParameter blueColourCount = new ClampedIntParameter(25, 0, 256);

    //public bool IsActive()
    //{
    //    return (pixelSize.value > 0) 
    //        && active;
    //}

    //public bool IsTileCompatible() => true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

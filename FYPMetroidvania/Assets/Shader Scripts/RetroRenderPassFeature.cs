using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class RetroRenderPassFeature : ScriptableRendererFeature
{
    private RetroRenderPass retroPass;
    [SerializeField] private Shader shader;
    private Material material;

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        retroPass = new RetroRenderPass(material);
        name = "Retro";
        retroPass.renderPassEvent = RenderPassEvent.AfterRendering;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (retroPass == null) return;
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(retroPass);
    }
    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

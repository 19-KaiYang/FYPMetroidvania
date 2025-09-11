using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class RetroRenderPass : ScriptableRenderPass
{
    private Material _material;
    private RetroPostProcess retroPostProcess;
    TextureDesc desc;

    private static readonly int redCountID = Shader.PropertyToID("_redCount");
    public RetroRenderPass(Material mat)
    {
        //if(!_material) _material = CoreUtils.CreateEngineMaterial(
        //    "Custom Post-Processing/Retro");
        _material = mat;
        //renderPassEvent = RenderPassEvent.AfterRendering;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        Debug.Log($"srcCamColor valid? {srcCamColor.IsValid()}");
        desc = renderGraph.GetTextureDesc(srcCamColor);
        desc.name = "Camera Retro Texture";
        desc.depthBufferBits = 0;
        desc.clearBuffer = false;
        var dst = renderGraph.CreateTexture(desc);

        UpdatePassSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        RenderGraphUtils.BlitMaterialParameters retroPass = new(srcCamColor, dst, _material, 0);
        renderGraph.AddBlitPass(retroPass, "Retro Pass");
        //renderGraph.AddBlitPass(srcCamColor, dst, new Vector2(1,1), new Vector2(0,0));
        resourceData.cameraColor = dst;
        //resourceData.activeColorTexture = dst;
    }

    void UpdatePassSettings()
    {
        if(_material == null) return;
        retroPostProcess = VolumeManager.instance.stack.GetComponent<RetroPostProcess>();
        if (retroPostProcess == null) return;
        int redcount = retroPostProcess.redColourCount.overrideState ?
            retroPostProcess.redColourCount.value : 200;


        _material.SetFloat("_pixelSize", (float)retroPostProcess.pixelSize);
        //_material.SetInteger(redCountID, redcount);
        //_material.SetInteger("_greenCount", retroPostProcess.greenColourCount.value);
        //_material.SetInteger("_blueCount", retroPostProcess.blueColourCount.value);
        //_material.SetInteger("_bayerLevel", retroPostProcess.bayerLevel.value);
    }
}

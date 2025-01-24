using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

/*public class CavernProjectionRendererFeature : ScriptableRendererFeature {
    public class BlitPass : ScriptableRenderPass {
        public Material blitMaterial;
        public RenderTexture sourceTexture;

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            var resourceData = frameData.Get<UniversalResourceData>();

            // Source
            RTHandle sourceTexRTHandle = RTHandles.Alloc(sourceTexture, false);
            TextureHandle sourceTexHandle = renderGraph.ImportTexture(sourceTexRTHandle);

            // Target
            var targetDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            targetDesc.name = $"CameraColor-{passName}";
            targetDesc.clearBuffer = false;
            TextureHandle targetTexHandle = renderGraph.CreateTexture(targetDesc);

            // Blit
            RenderGraphUtils.BlitMaterialParameters para = new(sourceTexHandle, targetTexHandle, blitMaterial, 0);
            renderGraph.AddBlitPass(para, passName: "Blit Pass");

            // Copy to screen.
            resourceData.cameraColor = targetTexHandle;
        }
    }

    [SerializeField] private Material blitMaterial;
    [SerializeField] private RenderTexture sourceTexture;

    private BlitPass blitPass;

    public override void Create() {
        blitPass = new BlitPass();
        blitPass.renderPassEvent = RenderPassEvent.AfterRendering;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        blitPass.blitMaterial = blitMaterial;
        blitPass.sourceTexture = sourceTexture;
        renderer.EnqueuePass(blitPass);
    }
}*/
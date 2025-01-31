using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class CavernProjectionRendererFeature : ScriptableRendererFeature {
    class ProjectionPass : ScriptableRenderPass {
        private Material material;

        public ProjectionPass(Material material) {
            this.material = material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            if (material == null) { return; }

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            if (resourceData.isActiveTargetBackBuffer) { return; } // The following line ensures that the render pass doesn't blit from the back buffer.

            TextureHandle target = resourceData.activeColorTexture;

            // This check is to avoid an error from the material preview in the scene.
            if (!target.IsValid()) return;

            TextureHandle emptySource = UniversalRenderer.CreateRenderGraphTexture(renderGraph, new RenderTextureDescriptor(32, 32, RenderTextureFormat.Default, 0), "Empty Source Texture", false);
            RenderGraphUtils.BlitMaterialParameters blitParams = new RenderGraphUtils.BlitMaterialParameters(emptySource, target, material, 0);
            renderGraph.AddBlitPass(blitParams, "Cavern Blit Pass");
        }
    }

    [SerializeField] private Material material;
    private ProjectionPass scriptablePass;

    public override void Create() {
        scriptablePass = new ProjectionPass(material);
        scriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType == CameraType.Game) {
            renderer.EnqueuePass(scriptablePass);
        }
    }
}
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Spelunx {
    public class CavernRenderer : MonoBehaviour {
        /*class ProjectionPass : ScriptableRenderPass {
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
                renderGraph.AddBlitPass(blitParams, "Cavern Projection Pass");
            }
        }*/

        public enum StereoscopicMode { Mono, Stereo, }
        
        public enum EyeResolution {
            Low = 1024,
            Mid = 2048,
            High = 4096,
            VeryHigh = 8192,
        }

        private enum CubemapIndex {
            // Monoscopic
            Mono = 0,

            // Stereoscopic
            Left = 0,
            Right,
            Front,
            Back,

            Num,
        }

        [Header("Camera Settings")]
        [SerializeField] private StereoscopicMode stereoMode = StereoscopicMode.Mono;
        [SerializeField] private EyeResolution eyeResolution = EyeResolution.Mid;
        [SerializeField, Range(0.0f, 1.0f)] private float interpupillaryDistance = 0.1f; // IPD in metres.
        [SerializeField, Min(0.1f)] private float cavernHeight = 2.0f; // Cavern physical screen height in metres.
        [SerializeField, Min(0.1f)] private float cavernRadius = 3.0f; // Cavern physical screen radius in metres.
        [SerializeField, Min(0.1f)] private float cavernAngle = 270.0f; // Cavern physical screen angle in degrees.
        [SerializeField, Range(-1.0f, 1.0f)] private float cavernElevation = 0.0f; // Cavern physical screen elevation off the floor in metres.

        [Header("References")]
        [SerializeField] private Shader shader;
        [SerializeField] private Camera captureCamera;

        private RenderTexture[] cubemaps;
        private Material material;
        // private ProjectionPass projectionPass; // This does not work.

        public float GetIPD() { return interpupillaryDistance; }
        public StereoscopicMode GetStereoscopicMode() { return stereoMode; }

        private void OnEnable() {
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable() {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void Awake() {
            // Initialise render textures.
            cubemaps = new RenderTexture[(int)CubemapIndex.Num];
            for (int i = 0; i < (int)CubemapIndex.Num; ++i) {
                cubemaps[i] = new RenderTexture((int)eyeResolution, (int)eyeResolution, 32, RenderTextureFormat.ARGB32);
                cubemaps[i].dimension = TextureDimension.Cube;
                cubemaps[i].wrapMode = TextureWrapMode.Clamp;
            }
            
            // Initialise material.
            material = new Material(shader);
            material.SetTexture("_CubemapMono", cubemaps[(int)CubemapIndex.Mono]);
            material.SetTexture("_CubemapLeft", cubemaps[(int)CubemapIndex.Left]);
            material.SetTexture("_CubemapRight", cubemaps[(int)CubemapIndex.Right]);
            material.SetTexture("_CubemapFront", cubemaps[(int)CubemapIndex.Front]);
            material.SetTexture("_CubemapBack", cubemaps[(int)CubemapIndex.Back]);

            // Initialise projection pass.
            // projectionPass = new ProjectionPass(material);
            // projectionPass.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        private void Start() {
        }

        private void Update() {
            RenderEyes();
        }

        private void RenderEyes() {
            const int leftMask = 1 << (int)CubemapFace.PositiveX;
            const int rightMask = 1 << (int)CubemapFace.NegativeX;
            const int topMask = 1 << (int)CubemapFace.PositiveY;
            const int bottomMask = 1 << (int)CubemapFace.NegativeY;
            const int frontMask = 1 << (int)CubemapFace.PositiveZ;
            const int backMask = 1 << (int)CubemapFace.NegativeZ;

            switch (stereoMode) {
                case StereoscopicMode.Mono:
                    captureCamera.stereoSeparation = 0.0f;
                    captureCamera.transform.localPosition = Vector3.zero;
                    captureCamera.transform.localRotation = Quaternion.identity;

                    // Use Camera.MonoOrStereoscopicEye.Left or Camera.MonoOrStereoscopicEye.Right to ensure that the cubemap follows the camera's rotation.
                    // Camera.MonoOrStereoscopicEye.Mono renders the cubemap to be aligned to the world's axes instead.
                    captureCamera.RenderToCubemap(cubemaps[(int)CubemapIndex.Mono], frontMask | leftMask | rightMask, Camera.MonoOrStereoscopicEye.Left);
                    break;
                case StereoscopicMode.Stereo:
                    // captureCamera.stereoSeparation = interpupillaryDistance;
                    captureCamera.stereoSeparation = 0.0f;
                    captureCamera.transform.localRotation = Quaternion.identity;
                    captureCamera.transform.localPosition = new Vector3(-interpupillaryDistance * 0.5f, 0.0f, 0.0f);
                    captureCamera.RenderToCubemap(cubemaps[(int)CubemapIndex.Left], frontMask, Camera.MonoOrStereoscopicEye.Left);
                    captureCamera.transform.localPosition = new Vector3(interpupillaryDistance * 0.5f, 0.0f, 0.0f);
                    captureCamera.RenderToCubemap(cubemaps[(int)CubemapIndex.Right], frontMask, Camera.MonoOrStereoscopicEye.Right);
                    captureCamera.transform.localPosition = new Vector3(0.0f, 0.0f, interpupillaryDistance * 0.5f);
                    captureCamera.RenderToCubemap(cubemaps[(int)CubemapIndex.Front], leftMask | rightMask, Camera.MonoOrStereoscopicEye.Left);
                    captureCamera.transform.localPosition = new Vector3(0.0f, 0.0f, -interpupillaryDistance * 0.5f);
                    captureCamera.RenderToCubemap(cubemaps[(int)CubemapIndex.Back], leftMask | rightMask, Camera.MonoOrStereoscopicEye.Right);
                    break;
            }

            material.SetInteger("_EnableStereo", stereoMode == StereoscopicMode.Stereo ? 1 : 0);
            material.SetFloat("_CavernHeight", cavernHeight);
            material.SetFloat("_CavernRadius", cavernRadius);
            material.SetFloat("_CavernAngle", cavernAngle);
            material.SetFloat("_CavernElevation", cavernElevation);
            material.SetMatrix("_CameraRotation", Matrix4x4.identity);
        }

        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
        }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == captureCamera) {
                // UniversalRenderPipelineAsset urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
                // urpAsset.scriptableRenderer.EnqueuePass(projectionPass);
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == captureCamera) {
                Graphics.Blit(null, material);
            }
        }
    }
}
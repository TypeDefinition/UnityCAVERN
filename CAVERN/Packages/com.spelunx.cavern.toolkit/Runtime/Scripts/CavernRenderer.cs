using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;

namespace Spelunx {
    [RequireComponent(typeof(Camera))]
    public class CavernRenderer : MonoBehaviour {
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
                renderGraph.AddBlitPass(blitParams, "Cavern Projection Pass");
            }
        }

        public enum StereoMode { Off, On, }
        public enum EyeResolution {
            Low = 1024,
            Mid = 2048,
            High = 4096,
        }

        [Header("Camera Settings")]
        [SerializeField] private StereoMode stereoMode = StereoMode.On;
        [SerializeField] private EyeResolution eyeResolution = EyeResolution.High;
        [SerializeField, Range(0.0f, 1.0f)] private float interpupillaryDistance = 0.064f; // IPD in metres.
        [SerializeField, Min(0.1f)] private float cavernHeight = 2.0f; // Cavern physical screen height in metres.
        [SerializeField, Min(0.1f)] private float cavernRadius = 3.0f; // Cavern physical screen radius in metres.
        [SerializeField, Min(0.1f)] private float cavernAngle = 270.0f; // Cavern physical screen angle in degrees.

        [Header("References")]
        [SerializeField] private Shader shader;
        [SerializeField] private Camera leftEye; // Have to do this for now because stereoSeperation is broken in Unity 6. Cannot use Camera.stereoSeperation.
        [SerializeField] private Camera rightEye;

        private RenderTexture cubemapMonoEye; // Mono
        private RenderTexture cubemapLeftEye; // Left Eye
        private RenderTexture cubemapRightEye; // Right Eye
        private Material material;
        private ProjectionPass projectionPass; // This does not work.

        public float GetIPD() { return interpupillaryDistance; }
        public StereoMode GetStereoMode() { return stereoMode; }

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
            cubemapMonoEye = new RenderTexture((int)eyeResolution, (int)eyeResolution, 32, RenderTextureFormat.ARGB32);
            cubemapMonoEye.dimension = TextureDimension.Cube;
            cubemapMonoEye.wrapMode = TextureWrapMode.Clamp;

            cubemapLeftEye = new RenderTexture((int)eyeResolution, (int)eyeResolution, 32, RenderTextureFormat.ARGB32);
            cubemapLeftEye.dimension = TextureDimension.Cube;
            cubemapLeftEye.wrapMode = TextureWrapMode.Clamp;

            cubemapRightEye = new RenderTexture((int)eyeResolution, (int)eyeResolution, 32, RenderTextureFormat.ARGB32);
            cubemapRightEye.dimension = TextureDimension.Cube;
            cubemapRightEye.wrapMode = TextureWrapMode.Clamp;

            // Initialise material.
            material = new Material(shader);
            material.SetTexture("_CubemapMonoEye", cubemapMonoEye);
            material.SetTexture("_CubemapLeftEye", cubemapLeftEye);
            material.SetTexture("_CubemapRightEye", cubemapRightEye);

            // Initialise projection pass.
            projectionPass = new ProjectionPass(material);
            projectionPass.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        private void Start() {
        }

        private void Update() {
            RenderEyes();
        }

        private void RenderEyes() {
            int faceMask = 0;
            faceMask |= 1 << (int)CubemapFace.PositiveX;
            faceMask |= 1 << (int)CubemapFace.NegativeX;
            faceMask |= 1 << (int)CubemapFace.PositiveY;
            faceMask |= 1 << (int)CubemapFace.NegativeY;
            faceMask |= 1 << (int)CubemapFace.PositiveZ;
            faceMask |= 1 << (int)CubemapFace.NegativeZ;

            Camera camera = GetComponent<Camera>();
            switch (stereoMode) {
                case StereoMode.Off:
                    camera.RenderToCubemap(cubemapMonoEye, faceMask, Camera.MonoOrStereoscopicEye.Mono);
                    break;
                case StereoMode.On:
                    camera.stereoSeparation = interpupillaryDistance;
                    // camera.RenderToCubemap(cubemapLeftEye, faceMask, Camera.MonoOrStereoscopicEye.Left);
                    // camera.RenderToCubemap(cubemapRightEye, faceMask, Camera.MonoOrStereoscopicEye.Right);

                    leftEye.transform.localPosition = new Vector3(-interpupillaryDistance * 0.5f, 0.0f, 0.0f);
                    leftEye.transform.localRotation = Quaternion.identity;
                    rightEye.transform.localPosition = new Vector3(interpupillaryDistance * 0.5f, 0.0f, 0.0f);
                    rightEye.transform.localRotation *= Quaternion.identity;
                    leftEye.RenderToCubemap(cubemapLeftEye, faceMask, Camera.MonoOrStereoscopicEye.Mono);
                    rightEye.RenderToCubemap(cubemapRightEye, faceMask, Camera.MonoOrStereoscopicEye.Mono);
                    break;
            }

            material.SetInteger("_EnableStereo", stereoMode == StereoMode.On ? 1 : 0);
            material.SetFloat("_CavernHeight", cavernHeight);
            material.SetFloat("_CavernRadius", cavernRadius);
            material.SetFloat("_CavernAngle", cavernAngle);
            material.SetMatrix("_CameraRotation", Matrix4x4.Rotate(transform.rotation));
        }

        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
        }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == GetComponent<Camera>()) {
                // UniversalRenderPipelineAsset urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
                // urpAsset.scriptableRenderer.EnqueuePass(projectionPass);
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == GetComponent<Camera>()) {
                Graphics.Blit(null, material);
            }
        }
    }
}
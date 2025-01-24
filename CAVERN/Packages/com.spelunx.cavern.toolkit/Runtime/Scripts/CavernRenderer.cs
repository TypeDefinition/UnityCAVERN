using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering;
using System.Collections.Generic;

namespace Spelunx {
    public enum StereoMode {
        Off,
        On,
    }

    [RequireComponent(typeof(Camera))]
    public class CavernRenderer : MonoBehaviour {
        // Todo: Move these settings into a scriptable object.
        [SerializeField] float ipd = 0.064f; // IPD in metres.
        [SerializeField] private StereoMode stereoMode = StereoMode.Off;
        [SerializeField] private Vector2Int screenResolution = new Vector2Int(5760, 1080);
        [SerializeField] private Shader shader;

        private Material material;
        private RenderTexture cubemapMonoEye; // Mono
        private RenderTexture cubemapLeftEye; // Left Eye
        private RenderTexture cubemapRightEye; // Right Eye
        private RenderTexture equirectangularProjection; // Equirectangular Projection

        public float GetIPD() { return ipd; }
        public StereoMode GetStereoMode() { return stereoMode; }

        private void OnEnable() {
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
        }

        private void OnDisable() {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
        }

        private void Awake() {
            // Initialise render textures.
            cubemapMonoEye = new RenderTexture(2048, 2048, 32, RenderTextureFormat.ARGB32);
            cubemapMonoEye.dimension = TextureDimension.Cube;
            cubemapMonoEye.wrapMode = TextureWrapMode.Clamp;

            cubemapLeftEye = new RenderTexture(2048, 2048, 32, RenderTextureFormat.ARGB32);
            cubemapLeftEye.dimension = TextureDimension.Cube;
            cubemapLeftEye.wrapMode = TextureWrapMode.Clamp;

            cubemapRightEye = new RenderTexture(2048, 2048, 32, RenderTextureFormat.ARGB32);
            cubemapRightEye.dimension = TextureDimension.Cube;
            cubemapRightEye.wrapMode = TextureWrapMode.Clamp;

            equirectangularProjection = new RenderTexture(screenResolution.x, screenResolution.y, 32, RenderTextureFormat.ARGB32);
            equirectangularProjection.dimension = TextureDimension.Tex2D;
            equirectangularProjection.wrapMode = TextureWrapMode.Clamp;

            // Initialise material.
            material = new Material(shader);
        }

        private void Start() {

        }

        private void Update() {
            RenderEyes();
        }

        private void RenderEyes() {
            Camera camera = GetComponent<Camera>();
            int faceMask = (int)(CubemapFace.PositiveX | CubemapFace.NegativeX | CubemapFace.PositiveY | CubemapFace.NegativeY | CubemapFace.PositiveZ | CubemapFace.NegativeZ);
            camera.RenderToCubemap(cubemapMonoEye, faceMask, Camera.MonoOrStereoscopicEye.Mono);
            cubemapMonoEye.ConvertToEquirect(equirectangularProjection, Camera.MonoOrStereoscopicEye.Mono);
        }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
            Graphics.Blit(equirectangularProjection, material);
        }
    }
}
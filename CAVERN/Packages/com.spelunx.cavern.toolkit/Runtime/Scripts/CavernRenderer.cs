using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering;
using System.Collections.Generic;

namespace Spelunx {
    [RequireComponent(typeof(Camera))]
    public class CavernRenderer : MonoBehaviour {
        public enum StereoMode { Off, On, }

        // Todo: Move these settings into a scriptable object.
        [SerializeField] float interpupillaryDistance = 0.064f; // IPD in metres.
        [SerializeField] private StereoMode stereoMode = StereoMode.Off;
        [SerializeField] private Shader shader;

        private Material material;
        [SerializeField] private RenderTexture cubemapMonoEye; // Mono
        [SerializeField] private RenderTexture cubemapLeftEye; // Left Eye
        [SerializeField] private RenderTexture cubemapRightEye; // Right Eye

        public float GetIPD() { return interpupillaryDistance; }
        public StereoMode GetStereoMode() { return stereoMode; }

        private void OnEnable() {
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
        }

        private void OnDisable() {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
        }

        private void Awake() {
            // Initialise render textures.
            // cubemapMonoEye = new RenderTexture(2048, 2048, 32, RenderTextureFormat.ARGB32);
            // cubemapMonoEye.dimension = TextureDimension.Cube;
            // cubemapMonoEye.wrapMode = TextureWrapMode.Clamp;

            // cubemapLeftEye = new RenderTexture(2048, 2048, 32, RenderTextureFormat.ARGB32);
            // cubemapLeftEye.dimension = TextureDimension.Cube;
            // cubemapLeftEye.wrapMode = TextureWrapMode.Clamp;

            // cubemapRightEye = new RenderTexture(2048, 2048, 32, RenderTextureFormat.ARGB32);
            // cubemapRightEye.dimension = TextureDimension.Cube;
            // cubemapRightEye.wrapMode = TextureWrapMode.Clamp;

            // equirectangularProjection = new RenderTexture(4096, 4096, 32, RenderTextureFormat.ARGB32);
            // equirectangularProjection.dimension = TextureDimension.Tex2D;
            // equirectangularProjection.wrapMode = TextureWrapMode.Clamp;

            // Initialise material.
            material = new Material(shader);
            material.SetTexture("_CubemapMonoEye", cubemapMonoEye);
            material.SetTexture("_CubemapLeftEye", cubemapLeftEye);
            material.SetTexture("_CubemapRightEye", cubemapRightEye);
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
                    camera.RenderToCubemap(cubemapLeftEye, faceMask, Camera.MonoOrStereoscopicEye.Left);
                    camera.RenderToCubemap(cubemapRightEye, faceMask, Camera.MonoOrStereoscopicEye.Right);
                    break;
            }
        }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
            material.SetInteger("_EnableStereo", stereoMode == StereoMode.On ? 1 : 0);
            Graphics.Blit(null, material);
        }
    }
}
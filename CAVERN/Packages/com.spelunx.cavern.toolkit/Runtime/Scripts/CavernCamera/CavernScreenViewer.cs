using Spelunx;
using UnityEngine;

namespace Spelunx {

    [RequireComponent(typeof(CavernRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CavernScreenViewer : MonoBehaviour {
        public enum EyeView { LeftEye, RightEye };

        [SerializeField] private EyeView eyeView = EyeView.LeftEye;

        public EyeView GetEyeView() { return eyeView; }

        private void Awake() {
        }

        private void Start() {
        }
        
        private void Update() {
        }

        private void LateUpdate() {
#if UNITY_EDITOR
            // Update render texture.
            GetComponent<MeshRenderer>().material.mainTexture = GetComponent<CavernRenderer>().GetScreenViewerTexture();
#endif
        }

        /// \*brief
        /// Generate a curved screen mesh.
        /// \*warning Ensure that the mesh's material disables back-face culling!
        private static Mesh GenerateMesh(float cavernRadius, float cavernHeight, float cavernElevation, float cavernAngle, EyeView eyeView) {
            Mesh mesh = new Mesh();

            // Have about one panel every 10 degrees. A reasonable number.
            int numPanels = Mathf.Max(1, (int)(cavernAngle / 10.0f));
            int numVertices = (numPanels + 1) * 2;

            Vector3[] positions = new Vector3[numVertices];
            Vector3[] normals = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] indices = new int[numPanels * 6];

            /********************************************** Generate inner surface. **********************************************/
            
            float cavernBottomHeight = -0.5f * cavernHeight + cavernElevation;
            float cavernTopHeight = 0.5f * cavernHeight + cavernElevation;

            float topUV = (eyeView == EyeView.LeftEye) ? 1.0f : 0.5f;
            float bottomUV = (eyeView == EyeView.LeftEye) ? 0.5f : 0.0f;

            float deltaAngle = cavernAngle / (float)numPanels;

            // Create vertices of surface.
            for (int i = 0; i <= numPanels; i++) {
                float ratio = (float)i / (float)numPanels;
                float currAngle = (ratio - 0.5f) * cavernAngle;

                // Take note that angle 0 points down the Z-axis, not the X-axis.
                float directionX = Mathf.Sin(currAngle * Mathf.Deg2Rad);
                float directionZ = Mathf.Cos(currAngle * Mathf.Deg2Rad);

                positions[i * 2] = new Vector3(cavernRadius * directionX, cavernTopHeight, cavernRadius * directionZ); // Top vertex.
                normals[i * 2] = new Vector3(cavernRadius * directionX, 0.0f, cavernRadius * directionZ);
                uvs[i * 2] = new Vector2((float)i / (float)numPanels, topUV);

                positions[i * 2 + 1] = new Vector3(cavernRadius * directionX, cavernBottomHeight, cavernRadius * directionZ); // Top vertex.
                normals[i * 2 + 1] = new Vector3(cavernRadius * directionX, 0.0f, cavernRadius * directionZ);
                uvs[i * 2 + 1] = new Vector2((float)i / (float)numPanels, bottomUV);
            }

            // Assign indices of each panel.
            // Each panel is a quad made up of 2 triangles.
            // Unity uses a CLOCKWISE WINDING ORDER for its triangles.
            for (int i = 0;  i < numPanels; ++i) {
                // Triangle 1
                indices[i * 6] = i * 2;
                indices[i * 6 + 1] = i * 2 + 2;
                indices[i * 6 + 2] = i * 2 + 1;

                // Triangle 2
                indices[i * 6 + 3] = i * 2 + 1;
                indices[i * 6 + 4] = i * 2 + 2;
                indices[i * 6 + 5] = i * 2 + 3;
            }
            
            mesh.name = "Cavern Screen Viewer Mesh";
            mesh.vertices = positions;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = indices;

            return mesh;
        }

#if UNITY_EDITOR
        public void GenerateMesh() {
            CavernRenderer cavernRenderer = GetComponent<CavernRenderer>();
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = GenerateMesh(cavernRenderer.GetCavernRadius(), cavernRenderer.GetCavernHeight(), cavernRenderer.GetCavernElevation(), cavernRenderer.GetCavernAngle(), eyeView);
        }

        public void ClearMesh() {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
        }
#endif
    }
}
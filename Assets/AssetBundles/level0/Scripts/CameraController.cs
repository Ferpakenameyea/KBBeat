using UnityEngine;

namespace AssetBundles.levels.level0
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(Skybox))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera m_Camera;
        [SerializeField] private Skybox m_Skybox;
        [Header("Change_1")]
        [SerializeField] private Material skybox;

        [Header("Change_2")]
        [SerializeField] private Material skybox2;

        [Header("Change_3")]
        [SerializeField] private Material skybox3;
        private void Start()
        {
            this.m_Camera = GetComponent<Camera>();
            this.m_Skybox = GetComponent<Skybox>();
        }

        public void ResetCamera()
        {
            this.m_Camera.clearFlags = CameraClearFlags.SolidColor;
            this.m_Camera.backgroundColor = Color.black;
        }

        public void Change_1()
        {
            this.m_Camera.clearFlags = CameraClearFlags.Skybox;
            this.m_Skybox.material = this.skybox;
        }

        public void Change_2()
        {
            this.m_Camera.clearFlags = CameraClearFlags.Skybox;
            this.m_Skybox.material = this.skybox2;
        }

        public void Change_3()
        {
            this.m_Camera.clearFlags = CameraClearFlags.Skybox;
            this.m_Skybox.material = this.skybox3;
        }

        public void ChangeWhiteBK()
        {
            this.m_Camera.clearFlags = CameraClearFlags.SolidColor;
            this.m_Camera.backgroundColor = Color.white;

            RenderSettings.ambientIntensity = 0.72f;
        }
    }
}

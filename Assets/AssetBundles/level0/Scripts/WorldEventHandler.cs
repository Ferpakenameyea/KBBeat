using KBbeat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundles.levels.level0
{
    [ExecuteInEditMode]
    public class WorldEventHandler : MonoBehaviour
    {
        [SerializeField] private bool fogControllerEnabled = false;

        [SerializeField] private Color fogColor;
        [SerializeField] private float fogDensity;

        private void Update() 
        {
            if (fogControllerEnabled)
            {
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
            }
        }

        public void End()
        {
            GameEndUI.showTarget = LevelPlayer.Instance.Export();
            SceneManager.LoadSceneAsync("End");
        }

        public void SetFog0()
        {
            RenderSettings.fogColor = new(0.5607843f, 0.8470589f, 0.9921569f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.001f;
            RenderSettings.ambientIntensity = 1.33f;
        }

        public void SetFog1()
        {
            RenderSettings.fogColor = new(0.735849f, 0.735849f, 0.735849f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.005f;
        }

        public void SetFog2()
        {
            RenderSettings.fogColor = new(0.122868f, 0, 0.1396227f, 1f);
            RenderSettings.ambientIntensity = 0.75f;
            RenderSettings.fogDensity = 0.001f;
            RenderSettings.fogMode = FogMode.Exponential;
        }
    }
}
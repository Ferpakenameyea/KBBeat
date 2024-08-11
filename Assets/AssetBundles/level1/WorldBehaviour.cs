using KBBeat;
using KBBeat.Core;
using KBBeat.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundles.levels.level1
{
    public class WorldBehaviour : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Material skybox1;
        [SerializeField] private Material skybox2;
        [SerializeField] private Material skybox3;
        [SerializeField] private MaterialChangeGroup buildingGroup;
        [SerializeField] private MaterialChangeGroup groundGroup;
        [SerializeField] private MaterialChangeGroup roadGroup;
        [SerializeField] private MaterialChangeGroup noteGroup;

        public void Awake()
        {
            StartCoroutine(WaitForSceneCoroutine());
        }

        private IEnumerator WaitForSceneCoroutine()
        {
            yield return new WaitUntil(() =>
            {
                var name = SceneManager.GetActiveScene().name;
                return name.Equals("ingame") || name.Contains("Lab");
            });
            Debug.Log("Set world fog");
            _camera.gameObject.GetComponent<Skybox>().material = skybox1;
            RenderSettings.skybox = skybox1;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.002f;
            RenderSettings.fogColor = new(0.482f, 0f, 0f, 1f);
            buildingGroup.Change(0);
            groundGroup.Change(0);
            roadGroup.Change(0);
            noteGroup.Change(0);
            yield break;
        }

        public void Change1()
        {
            RenderSettings.fogColor = new(1f, 1f, 1f, 1f);
            Debug.Log($"Changed world fog color to: {RenderSettings.fogColor}");
            _camera.gameObject.GetComponent<Skybox>().material = skybox2;
            RenderSettings.skybox = skybox2;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.001f;
            buildingGroup.Change(1);
            groundGroup.Change(1);
            roadGroup.Change(1);
            noteGroup.Change(1);
        }

        public void Change2()
        {
            _camera.gameObject.GetComponent<Skybox>().material = skybox3;
            RenderSettings.skybox = skybox3;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.001f;
            RenderSettings.fogColor = new(0.09019608f, 0.5254902f, 0.7333333f, 1f);
            buildingGroup.Change(2);
            roadGroup.Change(2);
            groundGroup.Change(0);
        }


        [Serializable]
        internal class MaterialChangeGroup
        {
            [SerializeField] internal Material target;
            [SerializeField] internal List<Material> materials;

            internal void Change(int index)
            {
                try
                {
                    target.CopyMatchingPropertiesFromMaterial(materials[index]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Out of range when changing material!");
                }
            }
        }

        public void End()
        {
            Debug.Log("End Triggered");
            GameEndUI.showTarget = LevelPlayer.Instance.Export();
            SceneManager.LoadSceneAsync("End");
        }
    }
}
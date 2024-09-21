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
        public void End()
        {
            GameEndUI.showTarget = LevelPlayer.Instance.Export();
            SceneManager.LoadSceneAsync("End");
        }
    }
}
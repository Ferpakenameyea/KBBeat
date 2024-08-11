using System.Collections;
using System.Collections.Generic;
using KBBeat;
using UnityEngine;
using UnityEngine.SceneManagement;
using KBBeat.Core;
using KBBeat.UI;

namespace KBBeat.Modding.Helper
{
    public class GameEndHelper : MonoBehaviour
    {
        public void End()
        {
            GameEndUI.showTarget = LevelPlayer.Instance.Export();
            SceneManager.LoadSceneAsync("End");
        }
    }
}


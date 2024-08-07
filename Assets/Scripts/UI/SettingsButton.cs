using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
internal class SettingsButton : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadSceneAsync("Settings", LoadSceneMode.Single);
            SoundManager.Instance.StopAllSounds();
        });
    }
}
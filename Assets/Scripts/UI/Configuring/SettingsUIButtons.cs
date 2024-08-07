using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUIButtons : MonoBehaviour
{
    [SerializeField] private Button SaveAndQuitButton;
    [SerializeField] private Button QuitButton;

    private void Start()
    {
        this.SaveAndQuitButton.onClick.AddListener(() =>
        {
            try
            {
                Configurator.Instance.Save()
                    .ContinueWith((task) =>
                    {
                        if (task.IsFaulted)
                        {
                            Debug.LogError("Configuration saving failed!");
                        }
                    });
            }
            finally
            {
                SceneManager.LoadSceneAsync("Menu");
            }
        });

        this.QuitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadSceneAsync("Menu");
        });
    }
}
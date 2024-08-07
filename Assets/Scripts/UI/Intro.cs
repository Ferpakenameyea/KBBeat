using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    [SerializeField] private AudioSource introSoundPlayer;
    public void PlayIntroSound()
    {
        introSoundPlayer.Play();
    }

    public void SwitchToMenu()
    {
        SceneManager.LoadSceneAsync("Menu");
    }
}

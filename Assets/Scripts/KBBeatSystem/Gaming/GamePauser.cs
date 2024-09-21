using KBBeat;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using KBBeat.Core;
using KBBeat.Audio;

internal class GamePauser : MonoBehaviour
{
    public KeyCode resumeKeyCode = KeyCode.Escape;
    public KeyCode pauseKeyCode = KeyCode.Escape;
    public KeyCode replayKeyCode = KeyCode.R;
    public KeyCode menuKeyCode = KeyCode.M;
    [SerializeField] private TextMeshProUGUI counter;
    [SerializeField] private TextMeshProUGUI pausedText;
    [SerializeField] private GaussianBlur cameraBlur;
    [Header("OnGamePauseBlur")]
    [Range(0, 4)]
    public int Iterations = 3;
    [Range(0f, 3.0f)]
    public float BlurSpread = 0.6f;
    [Range(1, 8)]
    public int DownSample = 2;
    private IEnumerator PauseCoroutine()
    {
        while (this.enabled)
        {
            if (Input.GetKeyDown(pauseKeyCode) && !BuiltInSettings.GamePaused)
            {
                BuiltInSettings.GamePaused = true;
                cameraBlur.enabled = true;

                cameraBlur.BlurSpread = this.BlurSpread;
                cameraBlur.Iterations = this.Iterations;
                cameraBlur.DownSample = this.DownSample;

                pausedText.gameObject.SetActive(true);
                Debug.Log("game paused!");
                Time.timeScale = 0f;
                yield return null;
            }

            if (BuiltInSettings.GamePaused)
            {
                if (Input.GetKeyDown(resumeKeyCode))
                {
                    Debug.Log("game resumed!");
                    pausedText.gameObject.SetActive(false);
                    counter.gameObject.SetActive(true);
                    var decreaseSpeed = this.cameraBlur.BlurSpread / BuiltInSettings.ResumeCountdown;

                    for (int i = BuiltInSettings.ResumeCountdown; i > 0; i--)
                    {
                        counter.text = i.ToString();
                        var p = SoundManager.Instance.PlayClip("countdown_beep", Channel.Effect);
                        for (int _ = 0; _ < 5; _++)
                        {
                            yield return new WaitForSecondsRealtime(0.2f);
                            this.cameraBlur.BlurSpread -= decreaseSpeed * 0.2f;
                        }
                    }
                    cameraBlur.enabled = false;
                    counter.gameObject.SetActive(false);
                    Time.timeScale = 1f;
                    BuiltInSettings.GamePaused = false;
                }
                else if (Input.GetKeyDown(menuKeyCode))
                {
                    this.ReturnToMenu();
                }
                else if (Input.GetKeyDown(replayKeyCode))
                {
                    this.RestartLevel();
                }

            }
            yield return null;
        }
    }

    private void Start()
    {
        counter.gameObject.SetActive(false);
        pausedText.gameObject.SetActive(false);
        StartCoroutine(this.PauseCoroutine());
    }

    private void RestartLevel()
    {
        LevelPlayer.Instance.Stop();
        BuiltInSettings.GamePaused = false;
        Time.timeScale = 1.0f;
        var action = SceneManager.LoadSceneAsync("ingame");
        action.completed += (_) => LevelPlayer.Instance.PlayLevel(LevelPlayer.Instance.PlayingLevel);
    }

    private void ReturnToMenu()
    {
        LevelPlayer.Instance.Stop();
        BuiltInSettings.GamePaused = false;
        Time.timeScale = 1.0f;
        var action = SceneManager.LoadSceneAsync("Menu");
    }
}
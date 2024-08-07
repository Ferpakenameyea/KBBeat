using KBbeat;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class GameEndUI : MonoBehaviour
{
    public static GameEndUIShowBundle showTarget = null;

    private Animator animator;
    [SerializeField] private List<RankItemLoadElem> loadList = new();
    private static Dictionary<Rank, Sprite> RankTexture = new();
    [SerializeField] private Image rankShower;
    [SerializeField] private TextMeshProUGUI acc;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private MenuBall ball;
    [SerializeField] private TextMeshProUGUI musicTitle;

    private static bool loaded = false;
    [SerializeField] private float showRank_seconds;
    [SerializeField] private float scaleOffset_showRank;

    [SerializeField] private float showAcc_seconds;
    [SerializeField] private float showScore_seconds;

    [SerializeField] private bool noShowDebug = false;

    private Coroutine showRankCoroutine = null;
    private Coroutine showAccCoroutine = null;
    private Coroutine showScoreCoroutine = null;

    [Header("Buttons")]
    [SerializeField] private MenuButton replayButton;
    [SerializeField] private MenuButton backButton;

    private Sprite GetRankTexture(Rank rank)
    {
        if (RankTexture.TryGetValue(rank, out var texture))
        {
            return texture;
        }
        return null;
    }

    [Serializable]
    private class RankItemLoadElem
    {
        public Rank rank;
        public Sprite texture;
    }

    private void Start()
    {
        this.animator = GetComponent<Animator>();

        this.replayButton.OnClick = () =>
        {
            var bundle = LevelManager.Instance.LoadedLevel.inPlaying.SceneAssetBundle;
            var req = SceneManager.LoadSceneAsync("ingame");
            req.completed += (_) =>
            {
                LevelPlayer.Instance.PlayLevel(LevelManager.Instance.LoadedLevel);
            };
        };

        this.backButton.OnClick = () =>
        {
            LevelManager.Instance.Unload();
            SceneManager.LoadSceneAsync("Menu");
        };

        if (!loaded)
        {
            RankTexture.Clear();
            foreach (var elem in this.loadList)
            {
                try
                {
                    RankTexture.Add(elem.rank, elem.texture);
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Error when loading texture of rank: {elem}");
                }
            }
            loaded = true;
        }
    }

    private IEnumerator ShowRankCoroutine()
    {
        rankShower.sprite = GetRankTexture(showTarget.rank);
        rankShower.gameObject.SetActive(true);
        rankShower.rectTransform.localScale = new(1, 1, 1);
        rankShower.SetNativeSize();
        rankShower.rectTransform.localScale *= this.scaleOffset_showRank;

        var size = rankShower.rectTransform.localScale;
        rankShower.rectTransform.localScale = size * 2;
        rankShower.color = new(1f, 1f, 1f, 0f);
        var scale_a = -2 * (size) / (showRank_seconds * showRank_seconds);
        var alpha_a = 2 * 1f / (showRank_seconds * showRank_seconds);
        var scaleSpeed = new Vector3();
        var alphaSpeed = 0f;
        var timeElapsed = 0f;
        while (timeElapsed < this.showRank_seconds)
        {
            alphaSpeed += alpha_a * Time.deltaTime;
            scaleSpeed += scale_a * Time.deltaTime;

            rankShower.rectTransform.localScale += scaleSpeed * Time.deltaTime;
            rankShower.color = rankShower.color + new Color(0, 0, 0, alphaSpeed) * Time.deltaTime;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        showRankCoroutine = null;
        yield break;
    }

    private IEnumerator ShowAccCoroutine()
    {
        var step = showTarget.acc / this.showAcc_seconds;
        acc.gameObject.SetActive(true);
        var timeElapsed = 0f;
        var cur = 0f;
        while (timeElapsed < this.showAcc_seconds)
        {
            cur += step * Time.deltaTime;
            acc.text = string.Format("{0:F2}%", cur * 100f);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        cur = showTarget.acc;
        acc.text = string.Format("{0:F2}%", cur * 100f);
        showAccCoroutine = null;
        yield break;
    }

    private IEnumerator ShowScoreCoroutine()
    {
        var step = showTarget.scores.TotalScore / this.showScore_seconds;
        var timeElapsed = 0f;
        score.gameObject.SetActive(true);
        var cur = 0f;
        while (timeElapsed < this.showScore_seconds)
        {
            timeElapsed += Time.deltaTime;
            score.text = ((int)cur).ToString();
            cur += step * Time.deltaTime;
            yield return null;
        }
        score.text = showTarget.scores.TotalScore.ToString();
        showScoreCoroutine = null;
        yield break;
    }

    public void ShowRank()
    {
        if (showRankCoroutine != null)
        {
            StopCoroutine(showRankCoroutine);
            showRankCoroutine = null;
        }
        showRankCoroutine = StartCoroutine(ShowRankCoroutine());
    }

    public void ShowAcc()
    {
        if (showAccCoroutine != null)
        {
            StopCoroutine(showAccCoroutine);
            showAccCoroutine = null;
        }
        showAccCoroutine = StartCoroutine(ShowAccCoroutine());
    }

    public void ShowScore()
    {
        if (showScoreCoroutine != null)
        {
            StopCoroutine(showScoreCoroutine);
            showScoreCoroutine = null;
        }
        showScoreCoroutine = StartCoroutine(this.ShowScoreCoroutine());
    }

    public void RunShow()
    {
        if (noShowDebug)
        {
            return;
        }
        var rec = new LevelHighScoreRecord(
            showTarget.scores.TotalScore,
            showTarget.scores.Accuracy(),
            showTarget.rank == Rank.S_plus
            );

        ScoreRecorder.Record(
            LevelManager.Instance.LoadedLevel.meta.AssetBundleName.Split('.')[1],
            rec
        );
        this.musicTitle.text = LevelManager.Instance.LoadedLevel.meta.Name;
        this.ball.SetCenterTexture(LevelManager.Instance.LoadedLevel.meta.art);
        Debug.Log("Showing result");
        this.ShowAcc();
        this.ShowScore();
        IEnumerator PlaySound()
        {
            float timer = 0f;
            while (timer < Mathf.Max(this.showAcc_seconds, this.showScore_seconds))
            {
                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
                SoundManager.Instance.PlayClip("type", Channel.Effect);
            }
        }
        StartCoroutine(PlaySound());
        Invoke(nameof(ShowRank), Mathf.Max(this.showAcc_seconds, this.showScore_seconds));
    }
}

[Serializable]
public class GameEndUIShowBundle
{
    public Scores scores;
    public float acc;
    public Rank rank;

    public GameEndUIShowBundle(Scores scores, Rank rank, float acc)
    {
        this.scores = scores;
        this.rank = rank;
        this.acc = acc;
    }
}
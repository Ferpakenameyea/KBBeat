using KBbeat;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreShower : MonoBehaviour
{
    [SerializeField] private List<RankItemLoadElem> loadList = new();
    private static Dictionary<Rank, Sprite> RankTexture = new();

    [SerializeField] private Image rankShower;
    [SerializeField] private TextMeshProUGUI highScore;
    private const string format =
                "<size=58.3><b>{0:D5}</b></size>" +
                "<size=36>{1:F4}%</size>";

    private void Start()
    {
        foreach (var elem in loadList)
        {
            if (!RankTexture.ContainsKey(elem.rank))
            {
                RankTexture.Add(elem.rank, elem.texture);
            }
        }
        rankShower.enabled = false;
        return;
    }
    public void Change(LevelHighScoreRecord rec)
    {
        if (rec == null)
        {
            rankShower.enabled = false;
            highScore.text = string.Format(format, 0, 0);
            return;
        }
        rankShower.enabled = true;
        rankShower.sprite = this.GetRankTexture(RankConverter.ToRank(rec.highAcc, rec.allCute));
        rankShower.SetNativeSize();
        highScore.text = string.Format(
            format,
            rec.highScore,
            rec.highAcc * 100);
    }

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
}

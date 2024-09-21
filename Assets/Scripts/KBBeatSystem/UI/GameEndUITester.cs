using KBBeat.UI;
using UnityEngine;

[RequireComponent(typeof(GameEndUI))]
internal class GameEndUITester : MonoBehaviour
{
    private GameEndUI target;

    [Header("Fake bundle")]
    [SerializeField] private GameEndUIShowBundle fakeBundle;

    private void Start()
    {
        this.target = GetComponent<GameEndUI>();
    }

    public void TestShowRank()
    {
        GameEndUI.showTarget = fakeBundle;
        target.ShowRank();
    }

    public void TestShowAcc()
    {
        GameEndUI.showTarget = fakeBundle;
        target.ShowAcc();
    }

    public void TestShowScore()
    {
        GameEndUI.showTarget = fakeBundle;
        target.ShowScore();
    }
}
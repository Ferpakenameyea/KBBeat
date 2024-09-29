using KBbeat;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InGameUI : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private TextMeshProUGUI combo;
    [SerializeField] private TextMeshProUGUI scoreShower;
    [SerializeField] private Color allCuteComboColor = Color.yellow;
    [SerializeField] private Color fullComboColor = Color.cyan;
    [SerializeField] private Color normalColor = Color.white;


    [SerializeField] private Animator lateAnimator;
    [SerializeField] private Animator earlyAnimator;
    [SerializeField] private Animator forceAnimator;
    [SerializeField] private TextMeshProUGUI forceTextMeshPro;


    private void Start()
    {
        this.animator = GetComponent<Animator>();
        ScoreCounter.Counter.OnScoresChanged += (score, _) =>
        {
            scoreShower.text =
                string.Format(
                    "{0:D5}\n" +
                    "{1:F4}%", score.TotalScore, score.Accuracy() * 100f);
        };
        scoreShower.text = "00000000\n0.0000%";

        ScoreCounter.Counter.OnScoresChanged += (score, _) =>
        {
            if (ScoreCounter.Counter.Combo <= 2)
            {
                this.combo.text = "";
                return;
            }
            this.combo.color =
                ScoreCounter.Counter.AllCute ? this.allCuteComboColor :
                ScoreCounter.Counter.FullCombo ? this.fullComboColor :
                                                    this.normalColor;
            this.combo.text =
                string.Format($"<line-height=30>\r\n<size=100>{ScoreCounter.Counter.Combo.ToString()}</size>\r\n<size=40>combo</size>\r\n</line-height>");
        };
        this.combo.text = "";

        ScoreCounter.OnFineEarly += EarlyWarn;
        ScoreCounter.OnFineLate += LateWarn;
        ScoreCounter.OnHeavy += HeavyWarn;
        ScoreCounter.OnWeak += WeakWarn;
    }

    private void OnDestroy() 
    {
        ScoreCounter.OnFineEarly -= EarlyWarn;
        ScoreCounter.OnFineLate -= LateWarn;
        ScoreCounter.OnHeavy -= HeavyWarn;
        ScoreCounter.OnWeak -= WeakWarn;    
    }

    private void LateWarn()
    {
        this.lateAnimator.SetTrigger("Shine");
    }

    private void EarlyWarn()
    {
        this.earlyAnimator.SetTrigger("Shine");
    }

    private void WeakWarn()
    {
        this.forceTextMeshPro.text = "TOOWEAK";
        this.forceAnimator.SetTrigger("Shine");
    }

    private void HeavyWarn()
    {
        this.forceTextMeshPro.text = "TOOHEAVY";
        this.forceAnimator.SetTrigger("Shine");
    }
}
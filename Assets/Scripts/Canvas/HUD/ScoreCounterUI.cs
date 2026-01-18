using UnityEngine;
using UnityEngine.UI;
public class ScoreCounterUI : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text scoreText;

    void Start()
    {
        if (ScoreCounter.Instance != null)
        {
            ScoreCounter.Instance.OnScoreChanged += ScoreUpdate;
            ScoreUpdate(ScoreCounter.Instance.GetScore());
        }
    }

    private void ScoreUpdate(int score)
    {
        scoreText.text = score.ToString();
    }
}

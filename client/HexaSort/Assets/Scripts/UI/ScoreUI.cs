using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HexaSort.UI
{
    public class ScoreUI : MonoBehaviour
    {
        public static ScoreUI Instance { get; private set; }

        [Header("Score")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Progress Bar")]
        [SerializeField] private RectTransform progressBarFill;
        [SerializeField] private float maxWidth = 300f;

        [Header("Star Markers")]
        [SerializeField] private Image star1Marker;
        [SerializeField] private Image star2Marker;
        [SerializeField] private Image star3Marker;
        [SerializeField] private Sprite starEmpty;
        [SerializeField] private Sprite starFilled;

        [Header("Score Thresholds")]
        [SerializeField] private int oneStarScore = 150;
        [SerializeField] private int twoStarScore = 200;
        [SerializeField] private int threeStarScore = 250;

        private void Awake()
        {
            Instance = this;
        }

        public void UpdateScore(int score)
        {
            scoreText.text = score.ToString();

            float progress = Mathf.Clamp01((float)score / threeStarScore);
            progressBarFill.sizeDelta = new Vector2(maxWidth * progress, progressBarFill.sizeDelta.y);

            star1Marker.sprite = score >= oneStarScore ? starFilled : starEmpty;
            star2Marker.sprite = score >= twoStarScore ? starFilled : starEmpty;
            star3Marker.sprite = score >= threeStarScore ? starFilled : starEmpty;
        }
    }
}
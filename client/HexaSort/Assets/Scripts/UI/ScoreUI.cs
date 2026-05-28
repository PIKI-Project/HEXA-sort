using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

        private int _displayedScore = 0;

        private void Awake()
        {
            Instance = this;
        }

        public void UpdateScore(int score)
        {
            DOTween.To(() => _displayedScore, x => 
            {
                _displayedScore = x;
                scoreText.text = _displayedScore.ToString();
            }, score, 0.5f).SetEase(Ease.OutQuad);

            float progress = Mathf.Clamp01((float)score / threeStarScore);
            float targetWidth = maxWidth * progress;

            progressBarFill
                .DOSizeDelta(new Vector2(targetWidth, progressBarFill.sizeDelta.y), 0.5f)
                .SetEase(Ease.OutQuad);

            AnimateStar(star1Marker, score >= oneStarScore);
            AnimateStar(star2Marker, score >= twoStarScore);
            AnimateStar(star3Marker, score >= threeStarScore);
        }

        private void AnimateStar(Image star, bool filled)
        {
            Sprite targetSprite = filled ? starFilled : starEmpty;
            
            if (star.sprite != targetSprite)
            {
                star.sprite = targetSprite;
                
                if (filled)
                {
                    star.transform.localScale = Vector3.zero;
                    star.transform
                        .DOScale(1f, 0.3f)
                        .SetEase(Ease.OutBack);
                }
            }
        }
    }
}
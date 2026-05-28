using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HexaSort.Services;
using HexaSort.Audio;
using DG.Tweening;

namespace HexaSort.UI
{
    public class WinPopup : MonoBehaviour
    {
        public static WinPopup Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;

        [Header("Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
        }

        private void Start()
        {
            nextButton?.onClick.AddListener(OnNextClicked);
            restartButton?.onClick.AddListener(OnRestartClicked);
            menuButton?.onClick.AddListener(OnMenuClicked);
        }

        public void Show(int stars)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].sprite = starEmpty;
                starImages[i].transform.localScale = Vector3.zero;
            }

            gameObject.SetActive(true);

            canvasGroup.alpha = 0f;
            panel.localScale = Vector3.one * 0.5f;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(canvasGroup
                .DOFade(1f, 0.3f)
                .SetEase(Ease.OutQuad));

            sequence.Join(panel
                .DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack));

            for (int i = 0; i < stars && i < starImages.Length; i++)
            {
                int index = i;
                sequence.AppendInterval(0.2f);
                sequence.AppendCallback(() =>
                {
                    starImages[index].sprite = starFilled;
                    starImages[index].transform
                        .DOScale(1f, 0.3f)
                        .SetEase(Ease.OutBack);
                });
            }
        }

        private void OnNextClicked()
        {
            int currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
            PlayerPrefs.SetInt("SelectedLevel", currentLevel + 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene("GameLevelScene");
        }

        private async void OnRestartClicked()
        {
            if (PlayerProgressService.Instance != null)
            {
                await PlayerProgressService.Instance.ClearGameStateAsync();
            }

            SceneManager.LoadScene("GameLevelScene");
        }

        private void OnMenuClicked()
        {
            MusicManager.Instance?.PlayMenuMusic();
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
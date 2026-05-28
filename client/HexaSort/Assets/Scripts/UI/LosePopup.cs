using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HexaSort.Services;
using HexaSort.Audio;
using DG.Tweening;

namespace HexaSort.UI
{
    public class LosePopup : MonoBehaviour
    {
        public static LosePopup Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panel;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
        }

        private void Start()
        {
            restartButton?.onClick.AddListener(OnRestartClicked);
            menuButton?.onClick.AddListener(OnMenuClicked);
        }

        public void Show()
        {
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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HexaSort.Services;
using HexaSort.Audio;

namespace HexaSort.UI
{
    public class WinPopup : MonoBehaviour
    {
        public static WinPopup Instance { get; private set; }

        [Header("UI Elements")]
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
                starImages[i].sprite = i < stars ? starFilled : starEmpty;
            }

            gameObject.SetActive(true);
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
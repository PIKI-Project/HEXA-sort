using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HexaSort.Services;
using HexaSort.Audio;

namespace HexaSort.UI
{
    public class LosePopup : MonoBehaviour
    {
        public static LosePopup Instance { get; private set; }

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
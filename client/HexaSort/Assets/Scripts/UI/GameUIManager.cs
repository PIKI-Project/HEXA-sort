using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HexaSort.Audio;
using HexaSort.Services;

namespace HexaSort.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button restartButton;

        private void Start()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
                
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnBackClicked()
        {
            MusicManager.Instance?.PlayMenuMusic();
            SceneManager.LoadScene("MainMenuScene");
        }

        private async void OnRestartClicked()
        {
            Debug.Log("[GameUIManager] Restart clicked");
            
            if (PlayerProgressService.Instance != null)
            {
                await PlayerProgressService.Instance.ClearGameStateAsync();
            }
            
            SceneManager.LoadScene("GameLevelScene");
        }
    }
}
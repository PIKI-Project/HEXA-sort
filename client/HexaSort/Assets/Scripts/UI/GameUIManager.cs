using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HexaSort.Audio;

namespace HexaSort.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnBackClicked()
        {
            MusicManager.Instance?.PlayMenuMusic();
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}

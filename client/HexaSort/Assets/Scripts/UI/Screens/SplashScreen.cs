using UnityEngine;
using UnityEngine.UI;
using HexaSort.Services;

namespace HexaSort.UI
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;

        private void Start()
        {
            playButton.onClick.AddListener(OnPlayClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnPlayClicked()
        {
            if (FirebaseAuthService.Instance.IsSignedIn)
            {
                UIManager.Instance.ShowLevelSelect();
            }
            else
            {
                UIManager.Instance.ShowAuth();
            }
        }

        private void OnSettingsClicked()
        {
            SettingsPopup.Instance?.Open();
        }
    }
}
using HexaSort.Services;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSort.UI
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        private void Start() => playButton.onClick.AddListener(OnPlayClicked);

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
    }
}

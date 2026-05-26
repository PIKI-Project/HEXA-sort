using UnityEngine;

namespace HexaSort.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Screens")][SerializeField] private GameObject splashScreen;
        [SerializeField] private GameObject authScreen;
        [SerializeField] private GameObject levelSelectScreen;

        private GameObject currentScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);

                return;
            }

            Instance = this;
        }

        private void Start()
        {
            HideAllScreens();
            ShowSplash();
        }

        public void ShowSplash() => SwitchTo(splashScreen);

        public void ShowAuth() => SwitchTo(authScreen);

        public void ShowLevelSelect() => SwitchTo(levelSelectScreen);

        private void SwitchTo(GameObject screen)
        {
            HideAllScreens();
            if (screen != null)
            {
                screen.SetActive(true);
                currentScreen = screen;
            }
        }

        private void HideAllScreens()
        {
            splashScreen?.SetActive(false);
            authScreen?.SetActive(false);
            levelSelectScreen?.SetActive(false);
        }
    }
}

using HexaSort.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace HexaSort.UI
{
    public class LevelSelectScreen : MonoBehaviour
    {
        [Header("Level Buttons")]
        [SerializeField]
        private LevelButton[] levelButtons;

        [Header("Logout")]
        [SerializeField] private Button logoutButton;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;

        [Header("Total Stars")]
        [SerializeField]
        private TextMeshProUGUI totalStarsText;

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = new(0.42f, 0.39f, 1f);
        [SerializeField] private Color lockedColor = new(0.29f, 0.29f, 0.29f);
        [SerializeField] private Color completedColor = new(0.31f, 0.80f, 0.64f);

        private async void OnEnable()
        {
            if (PlayerProgressService.Instance == null)
                return;

            if (!PlayerProgressService.Instance.IsLoaded)
            {
                await PlayerProgressService.Instance.LoadProgressAsync();
            }

            RefreshButtons();
        }

        private void Start()
        {
            if (levelButtons == null) return;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                if (levelButtons[i] == null) continue;

                int levelNum = i + 1;
                var button = levelButtons[i].GetButton();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnLevelClicked(levelNum));
                }
            }

            logoutButton?.onClick.AddListener(OnLogoutClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);
        }

        private void RefreshButtons()
        {
            var progress = PlayerProgressService.Instance;

            if (progress == null) return;
            if (levelButtons == null || levelButtons.Length == 0) return;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                if (levelButtons[i] == null) continue;

                int levelNum = i + 1;
                bool unlocked = progress.IsLevelUnlocked(levelNum);
                var levelResult = progress.GetLevelResult(levelNum);
                bool completed = levelResult != null && levelResult.completed;
                int stars = levelResult?.stars ?? 0;

                levelButtons[i].Setup(levelNum, stars, unlocked, completed);

                var button = levelButtons[i].GetButton();
                if (button == null) continue;

                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    if (completed)
                    {
                        image.color = completedColor;
                    }
                    else if (unlocked)
                    {
                        image.color = unlockedColor;
                    }
                    else
                    {
                        image.color = lockedColor;
                    }
                }

                button.interactable = unlocked;
            }

            if (totalStarsText != null)
            {
                totalStarsText.text = progress.TotalStars.ToString();
            }
        }

        private void OnLevelClicked(int levelNum)
        {
            PlayerPrefs.SetInt("SelectedLevel", levelNum);
            SceneManager.LoadScene("GameLevelScene");
        }

        private void OnSettingsClicked()
        {
            SettingsPopup.Instance?.Open();
        }

        private void OnLogoutClicked()
        {
            FirebaseAuthService.Instance?.SignOut();
            UIManager.Instance?.ShowSplash();
        }
    }
}
using HexaSort.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSort.UI
{
    public class LevelSelectScreen : MonoBehaviour
    {
        [Header("Level Buttons")]
        [SerializeField]
        private LevelButton[] levelButtons;

        [Header("Logout")][SerializeField] private Button logoutButton;

        [Header("Total Stars")]
        [SerializeField]
        private TextMeshProUGUI totalStarsText;

        [Header("Colors")][SerializeField] private Color unlockedColor = new(0.42f, 0.39f, 1f);
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
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelNum = i + 1;
                levelButtons[i].GetButton().onClick.AddListener(() => OnLevelClicked(levelNum));
            }

            logoutButton?.onClick.AddListener(OnLogoutClicked);
        }

        private void RefreshButtons()
        {
            PlayerProgressService progress = PlayerProgressService.Instance;

            if (progress == null) return;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelNum = i + 1;
                bool unlocked = progress.IsLevelUnlocked(levelNum);
                LevelResult levelResult = progress.GetLevelResult(levelNum);
                bool completed = levelResult != null && levelResult.completed;
                int stars = levelResult?.stars ?? 0;

                levelButtons[i].Setup(levelNum, stars, unlocked, completed);

                Button button = levelButtons[i].GetButton();
                ColorBlock colors = button.colors;

                if (completed)
                {
                    colors.normalColor = completedColor;
                    colors.highlightedColor = new Color(0.4f, 0.9f, 0.7f);
                    colors.pressedColor = new Color(0.2f, 0.6f, 0.5f);
                    colors.selectedColor = completedColor;
                }
                else if (unlocked)
                {
                    colors.normalColor = unlockedColor;
                    colors.highlightedColor = new Color(0.55f, 0.52f, 1f);
                    colors.pressedColor = new Color(0.35f, 0.33f, 0.83f);
                    colors.selectedColor = unlockedColor;
                }
                else
                {
                    colors.normalColor = lockedColor;
                    colors.highlightedColor = lockedColor;
                    colors.pressedColor = lockedColor;
                    colors.selectedColor = lockedColor;
                }

                colors.disabledColor = lockedColor;
                button.colors = colors;
            }

            if (totalStarsText != null)
            {
                totalStarsText.text = progress.TotalStars.ToString();
            }
        }

        private void OnLevelClicked(int levelNum)
        {
            // TODO: Load level
        }

        private void OnLogoutClicked()
        {
            FirebaseAuthService.Instance.SignOut();
            UIManager.Instance.ShowSplash();
        }
    }
}

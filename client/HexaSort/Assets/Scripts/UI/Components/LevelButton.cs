using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HexaSort.UI
{
    public class LevelButton : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private Image[] starImages;
        
        [Header("Star Sprites")]
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;

        private int levelNumber;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Setup(int level, int starsEarned, bool unlocked, bool completed)
        {
            levelNumber = level;
            
            if (levelNumberText != null)
                levelNumberText.text = level.ToString();

            if (starImages != null)
            {
                foreach (var star in starImages)
                {
                    if (star != null)
                        star.gameObject.SetActive(completed);
                }

                if (completed)
                {
                    for (int i = 0; i < starImages.Length; i++)
                    {
                        if (starImages[i] != null)
                            starImages[i].sprite = i < starsEarned ? starFilled : starEmpty;
                    }
                }
            }

            if (button != null)
                button.interactable = unlocked;
        }

        public Button GetButton()
        {
            return button;
        }
    }
}
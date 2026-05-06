using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HexaSort.Services;

namespace HexaSort.UI
{
    public class SettingsPopup : MonoBehaviour
    {
        public static SettingsPopup Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private Slider soundSlider;
        [SerializeField] private TextMeshProUGUI soundValueText;
        [SerializeField] private Button closeButton;

        private float lastSavedVolume;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
            soundSlider.onValueChanged.AddListener(OnVolumeChanged);
            
            LoadVolume();
            gameObject.SetActive(false);
        }

        public void Open()
        {
            LoadVolume();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            SaveVolume();
            gameObject.SetActive(false);
        }

        private void LoadVolume()
        {
            float volume = 0.5f;
            
            if (PlayerProgressService.Instance != null && PlayerProgressService.Instance.IsLoaded)
            {
                volume = PlayerProgressService.Instance.SoundVolume;
            }
            
            soundSlider.value = volume;
            lastSavedVolume = volume;
            UpdateVolumeText(volume);
            ApplyVolume(volume);
        }

        private async void SaveVolume()
        {
            float currentVolume = soundSlider.value;
            
            if (Mathf.Approximately(currentVolume, lastSavedVolume)) return;
            
            if (PlayerProgressService.Instance != null && PlayerProgressService.Instance.IsLoaded)
            {
                await PlayerProgressService.Instance.SetSoundVolumeAsync(currentVolume);
                lastSavedVolume = currentVolume;
            }
        }

        private void OnVolumeChanged(float value)
        {
            UpdateVolumeText(value);
            ApplyVolume(value);
        }

        private void UpdateVolumeText(float value)
        {
            int percent = Mathf.RoundToInt(value * 100);
            soundValueText.text = percent + "%";
        }

        private void ApplyVolume(float value)
        {
            AudioListener.volume = value;
        }
    }
}
using UnityEngine;

namespace HexaSort.Audio
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

        private AudioSource audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = musicVolume;
        }

        private void Start()
        {
            if (backgroundMusic != null)
            {
                audioSource.clip = backgroundMusic;
                audioSource.Play();
            }
        }
    }
}
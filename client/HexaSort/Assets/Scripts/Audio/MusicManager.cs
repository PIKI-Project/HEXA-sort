using UnityEngine;

namespace HexaSort.Audio
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;

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
        }

        private void Start()
        {
            PlayMenuMusic();
        }

        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        public void PlayGameMusic()
        {
            PlayMusic(gameMusic);
        }

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (audioSource.clip == clip && audioSource.isPlaying) return;

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
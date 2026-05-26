using UnityEngine;

namespace HexaSort.Audio
{
    public class MusicManager : MonoBehaviour
    {
        [Header("Music")][SerializeField] private AudioClip menuMusic;

        [SerializeField] private AudioClip gameMusic;

        private AudioSource _audioSource;
        public static MusicManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);

                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
        }

        private void Start() => PlayMenuMusic();

        public void PlayMenuMusic() => PlayMusic(menuMusic);

        public void PlayGameMusic() => PlayMusic(gameMusic);

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (_audioSource.clip == clip && _audioSource.isPlaying) return;

            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}

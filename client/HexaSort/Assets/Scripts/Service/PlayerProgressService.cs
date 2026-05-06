using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HexaSort.Services
{
    public class PlayerProgressService : MonoBehaviour
    {
        public static PlayerProgressService Instance { get; private set; }

        public event Action<PlayerProgress> OnProgressLoaded;
        public event Action<PlayerProgress> OnProgressSaved;
        public event Action<int, LevelResult> OnLevelCompleted;

        public PlayerProgress Progress { get; private set; }

        public int CurrentLevel => Progress?.currentLevel ?? 1;
        public int TotalStars => Progress?.totalStars ?? 0;
        public float SoundVolume => Progress?.soundVolume ?? 0.5f;
        public bool IsLoaded => Progress != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);

                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // TODO: remove async
        private async void Start()
        {
            if (FirebaseAuthService.Instance != null)
            {
                FirebaseAuthService.Instance.OnSignedIn += OnUserSignedIn;
                FirebaseAuthService.Instance.OnSignedOut += OnUserSignedOut;

                if (FirebaseAuthService.Instance.IsSignedIn)
                {
                    await LoadProgressAsync();
                }
            }
        }

        public async Task<bool> LoadProgressAsync()
        {
            if (!FirebaseAuthService.Instance.IsSignedIn)
                return false;

            string userId = FirebaseAuthService.Instance.UserId;

            FirestoreResult<PlayerProgress> result =
                await FirestoreService.Instance.GetDocumentAsync<PlayerProgress>("users", userId);

            if (result.Success && result.Data != null)
            {
                Progress = result.Data;
            }
            else if (result.NotFound)
            {
                Progress = CreateInitialProgress();
                await SaveProgressAsync();
            }
            else
            {
                Progress = CreateInitialProgress();
            }

            AudioListener.volume = Progress.soundVolume;
            OnProgressLoaded?.Invoke(Progress);

            return true;
        }

        public async Task<bool> SaveProgressAsync()
        {
            if (!FirebaseAuthService.Instance.IsSignedIn || Progress == null)
                return false;

            string userId = FirebaseAuthService.Instance.UserId;
            Progress.lastUpdated = DateTime.UtcNow.ToString("o");

            FirestoreResult<PlayerProgress> result =
                await FirestoreService.Instance.SetDocumentAsync("users", userId, Progress);

            if (result.Success)
            {
                OnProgressSaved?.Invoke(Progress);

                return true;
            }

            return false;
        }

        public async Task<bool> CompleteLevelAsync(int levelNumber, int score, int stars)
        {
            if (Progress == null) return false;

            stars = Mathf.Clamp(stars, 0, 3);

            LevelResult existingLevel = Progress.levels.Find(l => l.levelNumber == levelNumber);

            if (existingLevel != null)
            {
                if (score > existingLevel.bestScore)
                    existingLevel.bestScore = score;

                if (stars > existingLevel.stars)
                {
                    Progress.totalStars += stars - existingLevel.stars;
                    existingLevel.stars = stars;
                }

                existingLevel.completedAt = DateTime.UtcNow.ToString("o");
            }
            else
            {
                var levelResult = new LevelResult
                {
                    levelNumber = levelNumber,
                    stars = stars,
                    bestScore = score,
                    completed = true,
                    completedAt = DateTime.UtcNow.ToString("o")
                };

                Progress.levels.Add(levelResult);
                Progress.totalStars += stars;

                if (levelNumber >= Progress.currentLevel)
                    Progress.currentLevel = levelNumber + 1;
            }

            bool saved = await SaveProgressAsync();
            if (saved)
                OnLevelCompleted?.Invoke(levelNumber, Progress.levels.Find(l => l.levelNumber == levelNumber));

            return saved;
        }

        public async Task<bool> SetSoundVolumeAsync(float volume)
        {
            if (Progress == null) return false;

            Progress.soundVolume = volume;
            return await SaveProgressAsync();
        }

        public bool IsLevelUnlocked(int levelNumber)
        {
            if (Progress == null) return levelNumber == 1;

            return levelNumber <= Progress.currentLevel;
        }

        public LevelResult GetLevelResult(int levelNumber) =>
            Progress?.levels?.Find(l => l.levelNumber == levelNumber);

        public async Task<bool> ResetProgressAsync()
        {
            Progress = CreateInitialProgress();

            return await SaveProgressAsync();
        }

        private PlayerProgress CreateInitialProgress()
        {
            return new PlayerProgress
            {
                currentLevel = 1,
                totalStars = 0,
                soundVolume = 0.5f,
                createdAt = DateTime.UtcNow.ToString("o"),
                lastUpdated = DateTime.UtcNow.ToString("o"),
                levels = new List<LevelResult>()
            };
        }

        // TODO: remove async
        private async void OnUserSignedIn(UserData user) => await LoadProgressAsync();

        private void OnUserSignedOut() => Progress = null;

        private void OnDestroy()
        {
            if (FirebaseAuthService.Instance != null)
            {
                FirebaseAuthService.Instance.OnSignedIn -= OnUserSignedIn;
                FirebaseAuthService.Instance.OnSignedOut -= OnUserSignedOut;
            }

            if (Instance == this)
                Instance = null;
        }
    }

    [Serializable]
    public class PlayerProgress
    {
        public int currentLevel;
        public int totalStars;
        public float soundVolume = 0.5f;
        public string createdAt;
        public string lastUpdated;
        public List<LevelResult> levels;
    }

    [Serializable]
    public class LevelResult
    {
        public int levelNumber;
        public int stars;
        public int bestScore;
        public bool completed;
        public string completedAt;
    }
}
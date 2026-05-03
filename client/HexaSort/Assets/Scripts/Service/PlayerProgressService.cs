using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HexaSort.Services
{
    public class PlayerProgressService : MonoBehaviour
    {
        public static PlayerProgressService Instance { get; private set; }

        private PlayerProgress currentProgress;

        public event Action<PlayerProgress> OnProgressLoaded;
        public event Action<PlayerProgress> OnProgressSaved;
        public event Action<int, LevelResult> OnLevelCompleted;

        public PlayerProgress Progress => currentProgress;
        public int CurrentLevel => currentProgress?.currentLevel ?? 1;
        public int TotalStars => currentProgress?.totalStars ?? 0;
        public bool IsLoaded => currentProgress != null;

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

        private void Start()
        {
            if (FirebaseAuthService.Instance != null)
            {
                FirebaseAuthService.Instance.OnSignedIn += OnUserSignedIn;
                FirebaseAuthService.Instance.OnSignedOut += OnUserSignedOut;

                if (FirebaseAuthService.Instance.IsSignedIn)
                {
                    LoadProgressAsync();
                }
            }
        }

        public async Task<bool> LoadProgressAsync()
        {
            if (!FirebaseAuthService.Instance.IsSignedIn)
                return false;

            string userId = FirebaseAuthService.Instance.UserId;

            var result = await FirestoreService.Instance.GetDocumentAsync<PlayerProgress>("users", userId);

            if (result.Success && result.Data != null)
            {
                currentProgress = result.Data;
            }
            else if (result.NotFound)
            {
                currentProgress = CreateInitialProgress();
                await SaveProgressAsync();
            }
            else
            {
                currentProgress = CreateInitialProgress();
            }

            OnProgressLoaded?.Invoke(currentProgress);
            return true;
        }

        public async Task<bool> SaveProgressAsync()
        {
            if (!FirebaseAuthService.Instance.IsSignedIn || currentProgress == null)
                return false;

            string userId = FirebaseAuthService.Instance.UserId;
            currentProgress.lastUpdated = DateTime.UtcNow.ToString("o");

            var result = await FirestoreService.Instance.SetDocumentAsync("users", userId, currentProgress);

            if (result.Success)
            {
                OnProgressSaved?.Invoke(currentProgress);
                return true;
            }

            return false;
        }

        public async Task<bool> CompleteLevelAsync(int levelNumber, int score, int stars)
        {
            if (currentProgress == null) return false;

            stars = Mathf.Clamp(stars, 0, 3);

            var existingLevel = currentProgress.levels.Find(l => l.levelNumber == levelNumber);

            if (existingLevel != null)
            {
                if (score > existingLevel.bestScore)
                    existingLevel.bestScore = score;

                if (stars > existingLevel.stars)
                {
                    currentProgress.totalStars += stars - existingLevel.stars;
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

                currentProgress.levels.Add(levelResult);
                currentProgress.totalStars += stars;

                if (levelNumber >= currentProgress.currentLevel)
                    currentProgress.currentLevel = levelNumber + 1;
            }

            bool saved = await SaveProgressAsync();
            if (saved)
                OnLevelCompleted?.Invoke(levelNumber, currentProgress.levels.Find(l => l.levelNumber == levelNumber));

            return saved;
        }

        public bool IsLevelUnlocked(int levelNumber)
        {
            if (currentProgress == null) return levelNumber == 1;
            return levelNumber <= currentProgress.currentLevel;
        }

        public LevelResult GetLevelResult(int levelNumber)
        {
            return currentProgress?.levels?.Find(l => l.levelNumber == levelNumber);
        }

        public async Task<bool> ResetProgressAsync()
        {
            currentProgress = CreateInitialProgress();
            return await SaveProgressAsync();
        }

        private PlayerProgress CreateInitialProgress()
        {
            return new PlayerProgress
            {
                currentLevel = 1,
                totalStars = 0,
                createdAt = DateTime.UtcNow.ToString("o"),
                lastUpdated = DateTime.UtcNow.ToString("o"),
                levels = new List<LevelResult>()
            };
        }

        private void OnUserSignedIn(UserData user)
        {
            LoadProgressAsync();
        }

        private void OnUserSignedOut()
        {
            currentProgress = null;
        }

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
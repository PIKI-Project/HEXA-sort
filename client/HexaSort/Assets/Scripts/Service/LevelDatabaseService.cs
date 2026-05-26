using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HexaSort.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace HexaSort.Services
{
    public class LevelDatabaseService : MonoBehaviour
    {
        public static LevelDatabaseService Instance { get; private set; }

        [Header("Firebase Settings")]
        [SerializeField]
        private FirebaseConfig config;

        private const string FIRESTORE_BASE_URL =
            "https://firestore.googleapis.com/v1/projects/{0}/databases/(default)/documents/";

        private const string LEVELS_COLLECTION = "levels";

        private readonly Dictionary<int, LevelData> cachedLevels = new();

        private string BaseUrl => string.Format(FIRESTORE_BASE_URL, config.projectId);

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

        public async Task<LevelData> GetLevelAsync(int levelId)
        {
            if (cachedLevels.TryGetValue(levelId, out LevelData cached))
                return cached;

            string url = $"{BaseUrl}{LEVELS_COLLECTION}/level_{levelId}";

            try
            {
                using var request = UnityWebRequest.Get(url);
                await SendWebRequest(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    LevelData levelData = ParseLevelFromFirestore(request.downloadHandler.text);
                    if (levelData != null)
                    {
                        cachedLevels[levelId] = levelData;

                        return levelData;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        public async Task<bool> SaveLevelAsync(int levelId, LevelData levelData)
        {
            string token = FirebaseAuthService.Instance?.IdToken;

            if (string.IsNullOrEmpty(token))
                return false;

            string url = $"{BaseUrl}{LEVELS_COLLECTION}/level_{levelId}";
            string json = ConvertLevelToFirestore(levelId, levelData);

            try
            {
                using var request = new UnityWebRequest(url, "PATCH");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {token}");

                await SendWebRequest(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    cachedLevels[levelId] = levelData;

                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public async Task<List<LevelData>> GetAllLevelsAsync()
        {
            string url = $"{BaseUrl}{LEVELS_COLLECTION}";
            var levels = new List<LevelData>();

            try
            {
                using var request = UnityWebRequest.Get(url);
                await SendWebRequest(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    levels = ParseLevelsListFromFirestore(request.downloadHandler.text);

                    foreach (LevelData level in levels)
                    {
                        cachedLevels[level.Id] = level;
                    }
                }
            }
            catch
            {
            }

            return levels;
        }

        public void ClearCache() => cachedLevels.Clear();

        private string ConvertLevelToFirestore(int levelId, LevelData level)
        {
            string maskString = ConvertMaskToString(level.Mask);
            string startCellsJson = ConvertStartCellsToJson(level.StartCells);

            string json = $@"{{
                ""fields"": {{
                    ""id"": {{ ""integerValue"": ""{levelId}"" }},
                    ""name"": {{ ""stringValue"": ""{level.Name}"" }},
                    ""width"": {{ ""integerValue"": ""{level.Width}"" }},
                    ""height"": {{ ""integerValue"": ""{level.Height}"" }},
                    ""mask"": {{ ""stringValue"": ""{maskString}"" }},
                    ""startCells"": {{ ""stringValue"": ""{EscapeJson(startCellsJson)}"" }},
                    ""createdAt"": {{ ""stringValue"": ""{DateTime.UtcNow:o}"" }}
                }}
            }}";

            return json;
        }

        private LevelData ParseLevelFromFirestore(string response)
        {
            try
            {
                FirestoreLevelDocument doc = JsonUtility.FromJson<FirestoreLevelDocument>(response);

                if (doc?.fields == null) return null;

                int id = ParseInt(doc.fields.id);
                string name = doc.fields.name?.stringValue ?? $"Level {id}";
                int width = ParseInt(doc.fields.width);
                int height = ParseInt(doc.fields.height);
                string maskString = doc.fields.mask?.stringValue ?? "";
                string startCellsJson = doc.fields.startCells?.stringValue ?? "[]";

                bool[,] mask = ParseMaskFromString(maskString, width, height);
                List<StartCellData> startCells = ParseStartCellsFromJson(startCellsJson);

                return new LevelData
                {
                    Id = id,
                    Name = name,
                    Width = width,
                    Height = height,
                    Mask = mask,
                    StartCells = startCells
                };
            }
            catch
            {
                return null;
            }
        }

        private List<LevelData> ParseLevelsListFromFirestore(string response)
        {
            var levels = new List<LevelData>();

            try
            {
                FirestoreLevelListResponse listResponse = JsonUtility.FromJson<FirestoreLevelListResponse>(response);
                if (listResponse?.documents != null)
                {
                    foreach (FirestoreLevelDocumentWrapper docWrapper in listResponse.documents)
                    {
                        LevelData level = ParseLevelDocumentWrapper(docWrapper);
                        if (level != null)
                        {
                            levels.Add(level);
                        }
                    }
                }
            }
            catch
            {
            }

            levels.Sort((a, b) => a.Id.CompareTo(b.Id));

            return levels;
        }

        private LevelData ParseLevelDocumentWrapper(FirestoreLevelDocumentWrapper wrapper)
        {
            if (wrapper?.fields == null) return null;

            try
            {
                int id = ParseInt(wrapper.fields.id);
                string name = wrapper.fields.name?.stringValue ?? $"Level {id}";
                int width = ParseInt(wrapper.fields.width);
                int height = ParseInt(wrapper.fields.height);
                string maskString = wrapper.fields.mask?.stringValue ?? "";
                string startCellsJson = wrapper.fields.startCells?.stringValue ?? "[]";

                bool[,] mask = ParseMaskFromString(maskString, width, height);
                List<StartCellData> startCells = ParseStartCellsFromJson(startCellsJson);

                return new LevelData
                {
                    Id = id,
                    Name = name,
                    Width = width,
                    Height = height,
                    Mask = mask,
                    StartCells = startCells
                };
            }
            catch
            {
                return null;
            }
        }

        private string ConvertMaskToString(bool[,] mask)
        {
            if (mask == null) return "";

            int height = mask.GetLength(0);
            int width = mask.GetLength(1);
            var rows = new List<string>();

            for (int y = 0; y < height; y++)
            {
                var row = new StringBuilder();
                for (int x = 0; x < width; x++)
                {
                    row.Append(mask[y, x] ? '1' : '0');
                }

                rows.Add(row.ToString());
            }

            return string.Join(",", rows);
        }

        private bool[,] ParseMaskFromString(string maskString, int width, int height)
        {
            if (string.IsNullOrEmpty(maskString) || width <= 0 || height <= 0)
                return new bool[0, 0];

            bool[,] mask = new bool[height, width];
            string[] rows = maskString.Split(',');

            for (int y = 0; y < Mathf.Min(rows.Length, height); y++)
            {
                for (int x = 0; x < Mathf.Min(rows[y].Length, width); x++)
                {
                    mask[y, x] = rows[y][x] == '1';
                }
            }

            return mask;
        }

        private string ConvertStartCellsToJson(List<StartCellData> startCells)
        {
            if (startCells == null || startCells.Count == 0)
                return "[]";

            var items = new List<string>();
            foreach (StartCellData cell in startCells)
            {
                string stackJson = "[" + string.Join(",", cell.Stack) + "]";
                items.Add($"{{\"x\":{cell.X},\"y\":{cell.Y},\"stack\":{stackJson}}}");
            }

            return "[" + string.Join(",", items) + "]";
        }

        private List<StartCellData> ParseStartCellsFromJson(string json)
        {
            var result = new List<StartCellData>();

            if (string.IsNullOrEmpty(json) || json == "[]")
                return result;

            try
            {
                json = json.Trim('[', ']');

                if (string.IsNullOrEmpty(json)) return result;

                List<string> parts = SplitJsonObjects(json);

                foreach (string part in parts)
                {
                    StartCellData cell = ParseSingleStartCell(part);
                    if (cell != null)
                    {
                        result.Add(cell);
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        private List<string> SplitJsonObjects(string json)
        {
            var result = new List<string>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '{' || c == '[')
                {
                    depth++;
                }
                else if (c == '}' || c == ']')
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    result.Add(json.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            if (start < json.Length)
            {
                result.Add(json.Substring(start).Trim());
            }

            return result;
        }

        private StartCellData ParseSingleStartCell(string json)
        {
            try
            {
                json = json.Trim('{', '}');

                int x = 0, y = 0;
                var stack = new List<int>();

                Match xMatch = Regex.Match(json, @"""x""\s*:\s*(\d+)");
                if (xMatch.Success) x = int.Parse(xMatch.Groups[1].Value);

                Match yMatch = Regex.Match(json, @"""y""\s*:\s*(\d+)");
                if (yMatch.Success) y = int.Parse(yMatch.Groups[1].Value);

                Match stackMatch = Regex.Match(json, @"""stack""\s*:\s*\[([^\]]*)\]");
                if (stackMatch.Success)
                {
                    string stackStr = stackMatch.Groups[1].Value;
                    string[] nums = stackStr.Split(',');
                    foreach (string num in nums)
                    {
                        if (int.TryParse(num.Trim(), out int val))
                        {
                            stack.Add(val);
                        }
                    }
                }

                return new StartCellData { X = x, Y = y, Stack = stack };
            }
            catch
            {
                return null;
            }
        }

        private int ParseInt(FirestoreIntValue value)
        {
            if (value == null) return 0;
            if (!string.IsNullOrEmpty(value.integerValue))
                return int.Parse(value.integerValue);

            return 0;
        }

        private string EscapeJson(string json) => json.Replace("\"", "\\\"");

        private Task SendWebRequest(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ => tcs.SetResult(true);

            return tcs.Task;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    [Serializable]
    public class LevelData
    {
        public int Id;
        public string Name;
        public int Width;
        public int Height;
        public bool[,] Mask;
        public List<StartCellData> StartCells;

        public bool IsCellPlayable(int x, int y)
        {
            if (Mask == null) return false;
            if (y < 0 || y >= Height || x < 0 || x >= Width) return false;

            return Mask[y, x];
        }

        public StartCellData GetStartCell(int x, int y) => StartCells?.Find(c => c.X == x && c.Y == y);
    }

    [Serializable]
    public class StartCellData
    {
        public int X;
        public int Y;
        public List<int> Stack;
    }

    [Serializable]
    internal class FirestoreLevelDocument
    {
        public FirestoreLevelFields fields;
    }

    [Serializable]
    internal class FirestoreLevelListResponse
    {
        public FirestoreLevelDocumentWrapper[] documents;
    }

    [Serializable]
    internal class FirestoreLevelDocumentWrapper
    {
        public string name;
        public FirestoreLevelFields fields;
    }

    [Serializable]
    internal class FirestoreLevelFields
    {
        public FirestoreIntValue id;
        public FirestoreStringValue name;
        public FirestoreIntValue width;
        public FirestoreIntValue height;
        public FirestoreStringValue mask;
        public FirestoreStringValue startCells;
        public FirestoreStringValue createdAt;
    }

    [Serializable]
    internal class FirestoreIntValue
    {
        public string integerValue;
    }

    [Serializable]
    internal class FirestoreStringValue
    {
        public string stringValue;
    }
}

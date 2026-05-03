using System;
using System.Text;
using System.Threading.Tasks;
using HexaSort.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace HexaSort.Services
{
    public class FirestoreService : MonoBehaviour
    {
        public static FirestoreService Instance { get; private set; }

        [Header("Firebase Settings")]
        [SerializeField]
        private FirebaseConfig config;

        private const string FIRESTORE_BASE_URL =
            "https://firestore.googleapis.com/v1/projects/{0}/databases/(default)/documents/";

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

        public async Task<FirestoreResult<T>> GetDocumentAsync<T>(string collection, string documentId) where T : class
        {
            string token = FirebaseAuthService.Instance?.IdToken;

            if (string.IsNullOrEmpty(token))
                return new FirestoreResult<T> { Success = false, Error = "Не авторизован" };

            string url = $"{BaseUrl}{collection}/{documentId}";

            try
            {
                using UnityWebRequest request = CreateGetRequest(url, token);
                await SendWebRequest(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;
                    T document = ParseDocument<T>(response);

                    return new FirestoreResult<T> { Success = true, Data = document };
                }

                if (request.responseCode == 404)
                {
                    return new FirestoreResult<T> { Success = false, Error = "Документ не найден", NotFound = true };
                }

                string error = ParseError(request.downloadHandler.text);

                return new FirestoreResult<T> { Success = false, Error = error };
            }
            catch (Exception ex)
            {
                return new FirestoreResult<T> { Success = false, Error = ex.Message };
            }
        }

        public async Task<FirestoreResult<T>> SetDocumentAsync<T>(string collection, string documentId, T data)
            where T : class
        {
            string token = FirebaseAuthService.Instance?.IdToken;

            if (string.IsNullOrEmpty(token))
                return new FirestoreResult<T> { Success = false, Error = "Не авторизован" };

            string url = $"{BaseUrl}{collection}/{documentId}";
            string json = ConvertToFirestoreFormat(data);

            try
            {
                using UnityWebRequest request = CreatePatchRequest(url, json, token);
                await SendWebRequest(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return new FirestoreResult<T> { Success = true, Data = data };
                }

                string error = ParseError(request.downloadHandler.text);

                return new FirestoreResult<T> { Success = false, Error = error };
            }
            catch (Exception ex)
            {
                return new FirestoreResult<T> { Success = false, Error = ex.Message };
            }
        }

        public async Task<FirestoreResult<bool>> DeleteDocumentAsync(string collection, string documentId)
        {
            string token = FirebaseAuthService.Instance?.IdToken;

            if (string.IsNullOrEmpty(token))
                return new FirestoreResult<bool> { Success = false, Error = "Не авторизован" };

            string url = $"{BaseUrl}{collection}/{documentId}";

            try
            {
                using UnityWebRequest request = CreateDeleteRequest(url, token);
                await SendWebRequest(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return new FirestoreResult<bool> { Success = true, Data = true };
                }

                string error = ParseError(request.downloadHandler.text);

                return new FirestoreResult<bool> { Success = false, Error = error };
            }
            catch (Exception ex)
            {
                return new FirestoreResult<bool> { Success = false, Error = ex.Message };
            }
        }

        private UnityWebRequest CreateGetRequest(string url, string token)
        {
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            return request;
        }

        private UnityWebRequest CreatePatchRequest(string url, string json, string token)
        {
            var request = new UnityWebRequest(url, "PATCH");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            return request;
        }

        private UnityWebRequest CreateDeleteRequest(string url, string token)
        {
            var request = UnityWebRequest.Delete(url);
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            return request;
        }

        private Task SendWebRequest(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ => tcs.SetResult(true);

            return tcs.Task;
        }

        private string ConvertToFirestoreFormat<T>(T data)
        {
            var wrapper = new FirestoreDocument();
            wrapper.fields = new FirestoreFields();
            wrapper.fields.data = new FirestoreValue { stringValue = JsonUtility.ToJson(data) };

            return JsonUtility.ToJson(wrapper);
        }

        private T ParseDocument<T>(string response) where T : class
        {
            try
            {
                FirestoreDocumentResponse doc = JsonUtility.FromJson<FirestoreDocumentResponse>(response);
                if (doc.fields?.data?.stringValue != null)
                {
                    return JsonUtility.FromJson<T>(doc.fields.data.stringValue);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private string ParseError(string response)
        {
            try
            {
                FirestoreErrorResponse error = JsonUtility.FromJson<FirestoreErrorResponse>(response);

                return error.error?.message ?? "Неизвестная ошибка";
            }
            catch
            {
                return "Неизвестная ошибка";
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    [Serializable]
    public class FirestoreResult<T>
    {
        public bool Success;
        public string Error;
        public T Data;
        public bool NotFound;
    }

    [Serializable]
    internal class FirestoreDocument
    {
        public FirestoreFields fields;
    }

    [Serializable]
    internal class FirestoreFields
    {
        public FirestoreValue data;
    }

    [Serializable]
    internal class FirestoreValue
    {
        public string stringValue;
    }

    [Serializable]
    internal class FirestoreDocumentResponse
    {
        public string name;
        public FirestoreFields fields;
    }

    [Serializable]
    internal class FirestoreErrorResponse
    {
        public FirestoreError error;
    }

    [Serializable]
    internal class FirestoreError
    {
        public int code;
        public string message;
    }
}

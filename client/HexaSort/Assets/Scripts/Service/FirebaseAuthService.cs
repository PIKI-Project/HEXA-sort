using System;
using System.Text;
using System.Threading.Tasks;
using HexaSort.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace HexaSort.Services
{
    public class FirebaseAuthService : MonoBehaviour
    {
        public static FirebaseAuthService Instance { get; private set; }

        [Header("Firebase Settings")]
        [SerializeField]
        private FirebaseConfig config;

        private const string SIGN_UP_URL = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";
        private const string SIGN_IN_URL = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
        private const string PASSWORD_RESET_URL = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=";

        private const string TOKEN_KEY = "firebase_id_token";
        private const string USER_ID_KEY = "firebase_user_id";
        private const string EMAIL_KEY = "firebase_email";
        private const string REFRESH_TOKEN_KEY = "firebase_refresh_token";

        public event Action<UserData> OnSignedIn;
        public event Action OnSignedOut;
        public event Action<string> OnError;

        public bool IsSignedIn => CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.IdToken);
        public UserData CurrentUser { get; private set; }

        public string UserId => CurrentUser?.UserId ?? "";
        public string Email => CurrentUser?.Email ?? "";
        public string IdToken => CurrentUser?.IdToken ?? "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);

                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            TryRestoreSession();
        }

        public async Task<AuthResult> RegisterAsync(string email, string password)
        {
            AuthResult validation = ValidateInput(email, password);

            if (!validation.Success)
                return validation;

            var request = new EmailPasswordRequest
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            AuthResult result = await SendRequest(SIGN_UP_URL + config.apiKey, request);

            if (result.Success)
            {
                SaveSession();
                OnSignedIn?.Invoke(CurrentUser);
            }
            else
            {
                OnError?.Invoke(result.Error);
            }

            return result;
        }

        public async Task<AuthResult> SignInAsync(string email, string password)
        {
            AuthResult validation = ValidateInput(email, password);

            if (!validation.Success)
                return validation;

            var request = new EmailPasswordRequest
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            AuthResult result = await SendRequest(SIGN_IN_URL + config.apiKey, request);

            if (result.Success)
            {
                SaveSession();
                OnSignedIn?.Invoke(CurrentUser);
            }
            else
            {
                OnError?.Invoke(result.Error);
            }

            return result;
        }

        public async Task<AuthResult> SignInOrRegisterAsync(string email, string password)
        {
            AuthResult signInResult = await SignInAsync(email, password);

            if (signInResult.Success)
                return signInResult;

            if (signInResult.Error == "Пользователь не найден" ||
                signInResult.Error == "Неверный email или пароль")
            {
                return await RegisterAsync(email, password);
            }

            return signInResult;
        }

        public async Task<AuthResult> ResetPasswordAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return new AuthResult { Success = false, Error = "Email обязателен" };

            var request = new PasswordResetRequest
            {
                email = email,
                requestType = "PASSWORD_RESET"
            };

            string json = JsonUtility.ToJson(request);

            try
            {
                using UnityWebRequest webRequest = CreatePostRequest(PASSWORD_RESET_URL + config.apiKey, json);
                await SendWebRequest(webRequest);

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    return new AuthResult { Success = true };
                }

                string error = ParseError(webRequest.downloadHandler.text);

                return new AuthResult { Success = false, Error = error };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Error = ex.Message };
            }
        }

        public void SignOut()
        {
            CurrentUser = null;
            ClearSession();
            OnSignedOut?.Invoke();
        }

        private AuthResult ValidateInput(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
                return new AuthResult { Success = false, Error = "Email обязателен" };

            if (!email.Contains("@"))
                return new AuthResult { Success = false, Error = "Неверный формат email" };

            if (string.IsNullOrEmpty(password))
                return new AuthResult { Success = false, Error = "Пароль обязателен" };

            if (password.Length < 6)
                return new AuthResult { Success = false, Error = "Пароль должен быть минимум 6 символов" };

            return new AuthResult { Success = true };
        }

        private async Task<AuthResult> SendRequest(string url, EmailPasswordRequest requestData)
        {
            string json = JsonUtility.ToJson(requestData);

            try
            {
                using UnityWebRequest request = CreatePostRequest(url, json);
                await SendWebRequest(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);

                    CurrentUser = new UserData
                    {
                        UserId = response.localId,
                        Email = response.email,
                        IdToken = response.idToken,
                        RefreshToken = response.refreshToken
                    };

                    return new AuthResult { Success = true, User = CurrentUser };
                }

                string error = ParseError(request.downloadHandler.text);

                return new AuthResult { Success = false, Error = error };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Error = ex.Message };
            }
        }

        private UnityWebRequest CreatePostRequest(string url, string json)
        {
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            return request;
        }

        private Task SendWebRequest(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ => tcs.SetResult(true);

            return tcs.Task;
        }

        private string ParseError(string responseBody)
        {
            try
            {
                FirebaseErrorResponse error = JsonUtility.FromJson<FirebaseErrorResponse>(responseBody);
                string code = error.error.message;

                return code switch
                {
                    "EMAIL_NOT_FOUND" => "Пользователь не найден",
                    "INVALID_PASSWORD" => "Неверный пароль",
                    "INVALID_LOGIN_CREDENTIALS" => "Неверный email или пароль",
                    "USER_DISABLED" => "Аккаунт заблокирован",
                    "EMAIL_EXISTS" => "Email уже зарегистрирован",
                    "WEAK_PASSWORD" => "Пароль слишком простой (минимум 6 символов)",
                    "TOO_MANY_ATTEMPTS_TRY_LATER" => "Слишком много попыток, попробуйте позже",
                    "INVALID_EMAIL" => "Неверный формат email",
                    _ => code
                };
            }
            catch
            {
                return "Неизвестная ошибка";
            }
        }

        private void TryRestoreSession()
        {
            string token = PlayerPrefs.GetString(TOKEN_KEY, "");
            string userId = PlayerPrefs.GetString(USER_ID_KEY, "");
            string email = PlayerPrefs.GetString(EMAIL_KEY, "");
            string refreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_KEY, "");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
            {
                CurrentUser = new UserData
                {
                    UserId = userId,
                    Email = email,
                    IdToken = token,
                    RefreshToken = refreshToken
                };
                OnSignedIn?.Invoke(CurrentUser);
            }
        }

        private void SaveSession()
        {
            if (CurrentUser == null) return;

            PlayerPrefs.SetString(TOKEN_KEY, CurrentUser.IdToken);
            PlayerPrefs.SetString(USER_ID_KEY, CurrentUser.UserId);
            PlayerPrefs.SetString(EMAIL_KEY, CurrentUser.Email);
            PlayerPrefs.SetString(REFRESH_TOKEN_KEY, CurrentUser.RefreshToken);
            PlayerPrefs.Save();
        }

        private void ClearSession()
        {
            PlayerPrefs.DeleteKey(TOKEN_KEY);
            PlayerPrefs.DeleteKey(USER_ID_KEY);
            PlayerPrefs.DeleteKey(EMAIL_KEY);
            PlayerPrefs.DeleteKey(REFRESH_TOKEN_KEY);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    [Serializable]
    public class UserData
    {
        public string UserId;
        public string Email;
        public string IdToken;
        public string RefreshToken;
    }

    [Serializable]
    public class AuthResult
    {
        public bool Success;
        public string Error;
        public UserData User;
    }

    [Serializable]
    internal class EmailPasswordRequest
    {
        public string email;
        public string password;
        public bool returnSecureToken;
    }

    [Serializable]
    internal class PasswordResetRequest
    {
        public string email;
        public string requestType;
    }

    [Serializable]
    internal class AuthResponse
    {
        public string idToken;
        public string localId;
        public string email;
        public string refreshToken;
    }

    [Serializable]
    internal class FirebaseErrorResponse
    {
        public FirebaseError error;
    }

    [Serializable]
    internal class FirebaseError
    {
        public int code;
        public string message;
    }
}

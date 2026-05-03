using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HexaSort.Services;

namespace HexaSort.UI
{
    public class AuthScreen : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;

        [Header("Buttons")]
        [SerializeField] private Button signInButton;
        [SerializeField] private Button logInButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI errorText;

        private void Start()
        {
            signInButton.onClick.AddListener(OnSignInClicked);
            logInButton.onClick.AddListener(OnLogInClicked);
            ClearError();
        }

        private async void OnSignInClicked()
        {
            ClearError();

            string email = emailInput.text;
            string password = passwordInput.text;

            if (!ValidateInput(email, password)) return;

            SetButtonsInteractable(false);

            var result = await FirebaseAuthService.Instance.SignInAsync(email, password);

            if (result.Success)
            {
                UIManager.Instance.ShowLevelSelect();
            }
            else
            {
                ShowError(result.Error);
            }

            SetButtonsInteractable(true);
        }

        private async void OnLogInClicked()
        {
            ClearError();

            string email = emailInput.text;
            string password = passwordInput.text;

            if (!ValidateInput(email, password)) return;

            SetButtonsInteractable(false);

            var result = await FirebaseAuthService.Instance.RegisterAsync(email, password);

            if (result.Success)
            {
                UIManager.Instance.ShowLevelSelect();
            }
            else
            {
                ShowError(result.Error);
            }

            SetButtonsInteractable(true);
        }

        private bool ValidateInput(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                ShowError("Enter email");
                return false;
            }

            if (!email.Contains("@"))
            {
                ShowError("Invalid email format");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Enter password");
                return false;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            if (errorText != null)
                errorText.text = message;
        }

        private void ClearError()
        {
            if (errorText != null)
                errorText.text = "";
        }

        private void SetButtonsInteractable(bool interactable)
        {
            signInButton.interactable = interactable;
            logInButton.interactable = interactable;
        }
    }
}
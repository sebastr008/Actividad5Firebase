using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseAuthController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelAuth;
    [SerializeField] private GameObject panelLogin;
    [SerializeField] private GameObject panelRegister;
    [SerializeField] private GameObject panelHome;
    [SerializeField] private GameObject panelGame;
    [SerializeField] private GameObject panelLeaderboard;

    [Header("Register Inputs")]
    [SerializeField] private TMP_InputField inputRegisterUsername;
    [SerializeField] private TMP_InputField inputRegisterEmail;
    [SerializeField] private TMP_InputField inputRegisterPassword;

    [Header("Login Inputs")]
    [SerializeField] private TMP_InputField inputLoginEmail;
    [SerializeField] private TMP_InputField inputLoginPassword;

    [Header("Auth Buttons")]
    [SerializeField] private Button buttonRegister;
    [SerializeField] private Button buttonLogin;
    [SerializeField] private Button buttonRecoverPassword;
    [SerializeField] private Button buttonGoToRegister;
    [SerializeField] private Button buttonGoToLogin;
    [SerializeField] private Button buttonLogout;

    [Header("Labels")]
    [SerializeField] private TMP_Text textFeedback;
    [SerializeField] private TMP_Text textUsername;
    [SerializeField] private TMP_Text textBestScore;

    private FirebaseAuth auth;
    private DatabaseReference database;
    private bool firebaseReady;

    private void Start()
    {
        buttonRegister.onClick.AddListener(Register);
        buttonLogin.onClick.AddListener(Login);
        buttonRecoverPassword.onClick.AddListener(RecoverPassword);
        buttonGoToRegister.onClick.AddListener(ShowRegister);
        buttonGoToLogin.onClick.AddListener(ShowLogin);
        buttonLogout.onClick.AddListener(Logout);

        panelAuth.SetActive(true);
        panelHome.SetActive(false);
        panelGame.SetActive(false);
        panelLeaderboard.SetActive(false);

        ShowLogin();
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        SetFeedback("Conectando con Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                database = FirebaseDatabase.DefaultInstance.RootReference;
                firebaseReady = true;

                auth.StateChanged += OnAuthStateChanged;

                SetFeedback("Firebase conectado.");
                OnAuthStateChanged(this, EventArgs.Empty);
            }
            else
            {
                SetFeedback("Error con Firebase: " + task.Result);
            }
        });
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (!firebaseReady) return;

        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            panelAuth.SetActive(false);
            panelHome.SetActive(true);
            panelGame.SetActive(false);
            panelLeaderboard.SetActive(false);

            LoadUserData(user.UserId);
        }
        else
        {
            panelAuth.SetActive(true);
            panelHome.SetActive(false);
            panelGame.SetActive(false);
            panelLeaderboard.SetActive(false);

            ShowLogin();

            if (textUsername != null)
            {
                textUsername.text = "Jugador:";
            }

            if (textBestScore != null)
            {
                textBestScore.text = "Mejor puntaje: 0";
            }
        }
    }

    private void ShowLogin()
    {
        panelLogin.SetActive(true);
        panelRegister.SetActive(false);

        SetFeedback("Ingresa con tu correo y contraseña.");
    }

    private void ShowRegister()
    {
        panelLogin.SetActive(false);
        panelRegister.SetActive(true);

        SetFeedback("Crea una cuenta nueva.");
    }

    private void Register()
    {
        if (!firebaseReady)
        {
            SetFeedback("Firebase todavía no está listo.");
            return;
        }

        string username = inputRegisterUsername.text.Trim();
        string email = inputRegisterEmail.text.Trim();
        string password = inputRegisterPassword.text;

        if (string.IsNullOrEmpty(username))
        {
            SetFeedback("Escribe un nombre de usuario.");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            SetFeedback("Escribe un correo.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetFeedback("Escribe una contraseña.");
            return;
        }

        SetFeedback("Registrando usuario...");

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                SetFeedback("Error al registrar: " + task.Exception.GetBaseException().Message);
                return;
            }

            AuthResult result = task.Result;
            FirebaseUser user = result.User;

            SaveUserData(user.UserId, username, email);
        });
    }

    private void SaveUserData(string userId, string username, string email)
    {
        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "username", username },
            { "email", email },
            { "score", 0 },
            { "createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        database.Child("users").Child(userId).UpdateChildrenAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                SetFeedback("Usuario creado, pero no se guardaron los datos extra.");
                return;
            }

            SetFeedback("Registro exitoso.");
            LoadUserData(userId);
        });
    }

    private void Login()
    {
        if (!firebaseReady)
        {
            SetFeedback("Firebase todavía no está listo.");
            return;
        }

        string email = inputLoginEmail.text.Trim();
        string password = inputLoginPassword.text;

        if (string.IsNullOrEmpty(email))
        {
            SetFeedback("Escribe tu correo.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetFeedback("Escribe tu contraseña.");
            return;
        }

        SetFeedback("Iniciando sesión...");

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                SetFeedback("Error al iniciar sesión: " + task.Exception.GetBaseException().Message);
                return;
            }

            SetFeedback("Login exitoso.");
        });
    }

    private void RecoverPassword()
    {
        if (!firebaseReady)
        {
            SetFeedback("Firebase todavía no está listo.");
            return;
        }

        string email = inputLoginEmail.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            SetFeedback("Escribe tu correo para recuperar contraseña.");
            return;
        }

        SetFeedback("Enviando correo de recuperación...");

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                SetFeedback("No se pudo enviar el correo de recuperación.");
                return;
            }

            SetFeedback("Correo de recuperación enviado.");
        });
    }

    private void Logout()
    {
        if (!firebaseReady) return;

        auth.SignOut();
        SetFeedback("Sesión cerrada.");
    }

    private void LoadUserData(string userId)
    {
        database.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                SetFeedback("No se pudieron cargar los datos del usuario.");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (!snapshot.Exists)
            {
                textUsername.text = "Jugador: Sin datos";
                textBestScore.text = "Mejor puntaje: 0";
                return;
            }

            string username = "Sin nombre";
            string score = "0";

            if (snapshot.Child("username").Value != null)
            {
                username = snapshot.Child("username").Value.ToString();
            }

            if (snapshot.Child("score").Value != null)
            {
                score = snapshot.Child("score").Value.ToString();
            }

            textUsername.text = "Jugador: " + username;
            textBestScore.text = "Mejor puntaje: " + score;
        });
    }

    private void SetFeedback(string message)
    {
        Debug.Log(message);

        if (textFeedback != null)
        {
            textFeedback.text = message;
        }
    }
}
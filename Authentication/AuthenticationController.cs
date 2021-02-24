using Planner.Authentication.Models;
using Planner.Authentication.Requests;
using Planner.Launcher;
using Planner.Launcher.Alert;
using Planner.Launcher.Notification;
using Planner.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Planner.Authentication
{
    /*! \defgroup authentication Аутентификация
        \brief Данный модуль предназначен для контроля входа в приложение
     
        Модуль авторизации пользователей. Обрабатывает логин, авторизацию
        по токену, проводит данные пользователя.
     */
    
    /*!
     \ingroup authentication
     */
    
    /// <summary>
    /// Авторизация
    /// </summary>
    public class AuthenticationController: IInitializable
    {
        [Inject]
        private AuthenticationView _authenticationView;

        [Inject]
        private RestorePasswordView _restorePasswordView;


        [Inject]
        private UserProvider _userProvider;

        [Inject]
        private LogInRequest.Pool _loginRequester;

        [Inject]
        private AuthenticationRequest.Pool _authenticationRequester;

        [Inject]
        private RestorePasswordRequest.Pool _restorePasswordRequester;

        [Inject]
        private Launcher _launcher;

        [Inject]
        private LoadProgress _loadProgress;

        [Inject]
        private MonoCompositeActivatable _monoCompositeActivatable;

        [Inject]
        private AlertController _alertController;

        private CompositeDisposable _disposable;

        private AuthenticationData _authenticationData;

        private const string TokenPrefsKey = "auth_token";

        private string email;

        private bool isAuthenticated = false;

        public void Initialize()
        {
            if (PlayerPrefs.HasKey(TokenPrefsKey))
            {
                var token = new Models.Token(PlayerPrefs.GetString(TokenPrefsKey));
                Auth(token);
            }

            _disposable = new CompositeDisposable();
            
            _authenticationData = new AuthenticationData();

            _authenticationView
                .OnLogInButtonClick
                .Subscribe(_ => LogIn())
                .AddTo(_disposable);

            _authenticationView
                .OnLoginInputFieldChange
                .Subscribe(login => _authenticationData.SetLogIn(login))
                .AddTo(_disposable);

            _authenticationView
                .OnPasswordInputFieldChange
                .Subscribe(password => _authenticationData.SetPassword(password))
                .AddTo(_disposable);

            _authenticationView
                .OnForgotMeToggleChange
                .Subscribe(forgotMe => _authenticationData.ForgotMe = forgotMe)
                .AddTo(_disposable);

            _authenticationView
                .OnForgotPasswordTextClick
                .Subscribe(_ => ShowView(true))
                .AddTo(_disposable);

            _restorePasswordView
                .OnResroteButtonClick
                .Subscribe(_ => SendRestorePasswordReqest())
                .AddTo(_disposable);

            _restorePasswordView
                .OnBackButtonClick
                .Subscribe(_ => ShowView(false))
                .AddTo(_disposable);

            _restorePasswordView
                .OnEmailInputFieldChange
                .Subscribe(email => this.email = email)
                .AddTo(_disposable);
        }

        /// <summary>
        /// Вход
        /// После успешного входа получаем токен и сохраняем в PlayerPrefs
        /// </summary>
        private void LogIn()
        {
            var memento = _authenticationData.GetMemento();
            _loginRequester
                .Spawn(
                    new LogInRequest.RequestData(memento.Login, memento.Password),
                    tokenData =>
                    {
                        if (_authenticationData.ForgotMe)
                            PlayerPrefs.SetString(TokenPrefsKey, tokenData.Token.Identifier);
                        
                        Auth(tokenData.Token);
                    },
                    _ =>
                    {
                        OnErrLogin();
                    });
        }

        private void Auth(Models.Token token)
        {
            _authenticationRequester.Spawn(
                new AuthenticationRequest.RequestData(token),
                userData => 
                {
                    OnSuccessAuth(userData.User);
                },
                _ =>
                {
                    OnErrAuth();
                });
        }

        /// <summary>
        /// После успешной авторизации
        /// </summary>
        /// <param name="user"></param>
        private void OnSuccessAuth(Models.User user)
        {
            if (user == default || isAuthenticated) return;
            isAuthenticated = true;
            _userProvider.User = user;
            _launcher.Launch(user, () =>
            {
                //Debug.Log($"Welcome {user.GetFullName()}.");
                _monoCompositeActivatable.Deactivate();
            });
        }

        private void OnErrAuth()
        {
            PlayerPrefs.DeleteKey(TokenPrefsKey);
        }

        private void OnErrLogin()
        {
            _alertController
                .ShowAlert(AlertType.InvalidLoginOrPassword);
        }

        public void LogOut()
        {
            PlayerPrefs.DeleteKey(TokenPrefsKey);
            _monoCompositeActivatable.Activate();
        }

        public void ShowView(bool isRestoreView)
        {
            _authenticationView.SetActive(!isRestoreView);
            _restorePasswordView.SetActive(isRestoreView);
        }

        /// <summary>
        /// Отправка запроса на восстановление
        /// </summary>
        private void SendRestorePasswordReqest()
        {
            _restorePasswordRequester
                .Spawn(
                    new RestorePasswordRequest.RequestData(email),
                    IsEmailExist =>
                    {
                        if (IsEmailExist)
                            OnExistingEmail();
                        else
                            OnNonexistentEmail();
                    },
                    _ =>
                    {
                        OnNonexistentEmail();
                    });
        }

        /// <summary>
        /// Если указаная почта существует
        /// </summary>
        /// <param name="user"></param>
        private void OnExistingEmail()
        {
            _alertController
                .ShowAlert(AlertType.RestorePasswordEmailExist);
        }

        /// <summary>
        /// Если указаная почта НЕ существует
        /// </summary>
        /// <param name="user"></param>
        private void OnNonexistentEmail()
        {
            _alertController
                .ShowAlert(AlertType.RestorePasswordEmailNotExist);
        }
    }
}
using System;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Planner.Authentication
{
    /*!
     \ingroup authentication
     \brief Класс вьюхи авторизации
     */
    public class AuthenticationView: UIBehaviour
    {
        [SerializeField]
        private TMP_InputField _loginInputField;

        [SerializeField]
        private TMP_InputField _passwordInputField;

        [SerializeField]
        private Button _loginButton;

        [SerializeField]
        private Toggle _forgotMeToggle;

        [SerializeField]
        private TMP_Text _forgotPassword;

        public IObservable<string> OnLoginInputFieldChange { get; private set; }
        
        public IObservable<string> OnPasswordInputFieldChange { get; private set; }

        public IObservable<Unit> OnLogInButtonClick { get; private set; }
        
        public IObservable<bool> OnForgotMeToggleChange { get; private set; }

        public IObservable<PointerEventData> OnForgotPasswordTextClick { get; private set; }

        protected override void Awake()
        {
            OnLoginInputFieldChange = _loginInputField
                .onValueChanged
                .AsObservable()
                .TakeUntilDestroy(this);

            OnPasswordInputFieldChange = _passwordInputField
                .onValueChanged
                .AsObservable()
                .TakeUntilDestroy(this);

            OnLogInButtonClick = _loginButton
                .OnClickAsObservable()
                .TakeUntilDestroy(this);

            OnForgotMeToggleChange = _forgotMeToggle
                .OnValueChangedAsObservable()
                .TakeUntilDestroy(this);

            OnForgotPasswordTextClick = _forgotPassword
                .OnPointerClickAsObservable()
                .TakeUntilDestroy(this);
        }
    }
}
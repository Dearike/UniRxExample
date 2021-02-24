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
     \brief Класс вьюхи восстановления пароля
     */
    public class RestorePasswordView : UIBehaviour
    {
        [SerializeField]
        private TMP_InputField _emailInputField;

        [SerializeField]
        private Button _restoreButton;

        [SerializeField]
        private Button _backButton;

        public IObservable<string> OnEmailInputFieldChange { get; private set; }

        public IObservable<Unit> OnResroteButtonClick { get; private set; }

        public IObservable<Unit> OnBackButtonClick { get; private set; }

        protected override void Awake()
        {
            gameObject.SetActive(false);

            OnEmailInputFieldChange = _emailInputField
                .onValueChanged
                .AsObservable()
                .TakeUntilDestroy(this);

            OnResroteButtonClick = _restoreButton
                .OnClickAsObservable()
                .TakeUntilDestroy(this);

            OnBackButtonClick = _backButton
                .OnClickAsObservable()
                .TakeUntilDestroy(this);
        }
    }
}
using System.Collections.Generic;
using Planner.Authentication.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Planner.Authentication
{
    public class UserRightViewDisabler: UIBehaviour
    {
        [SerializeField]
        private UserRight _userRight;

        [Inject]
        private UserProvider _userProvider;

        protected override void OnEnable()
        {
            if (!_userProvider.User.HasRights(_userRight))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
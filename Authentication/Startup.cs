using UnityEngine.EventSystems;
using Zenject;

namespace Planner.Authentication
{
    public class Startup: UIBehaviour
    {
        [Inject] 
        private AuthenticationController _authenticationController;

        protected override void Start()
        {
            _authenticationController.Initialize();
        }
    }
}
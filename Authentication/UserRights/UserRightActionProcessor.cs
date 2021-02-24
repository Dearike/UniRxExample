using System.Collections.Generic;
using Planner.Authentication.Models;
using Zenject;

namespace Planner.Authentication.UserRights
{
    public class UserRightActionProcessor
    {
        [Inject]
        private IUserProvider _userProvider;
        
        private readonly Dictionary<UserRight, AbstractAction> _cashedActions 
            = new Dictionary<UserRight, AbstractAction>();
        
        public void MakeAction(UserRight userRight)
        {
            if (!_userProvider.User.HasRights(userRight)) return;

            var actionIsCashed = _cashedActions.TryGetValue(userRight, out var action);
            
            if (!actionIsCashed)
            {
                switch (userRight)
                {
                    case UserRight.Render:
                        action = new RenderAction();
                        break;
                    default:
                        return;
                }
                
                _cashedActions.Add(userRight, action);
            }
            
            action.Make();
        }
    }
}
using UnityEngine;

namespace Planner.Authentication.UserRights
{
    public class RenderAction: AbstractAction
    {
        public override void Make()
        {
            Debug.LogError("render");
        }
    }
}
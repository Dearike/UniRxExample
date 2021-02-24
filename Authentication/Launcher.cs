using System;
using Planner.Authentication.Models;
using Planner.Launcher;

namespace Planner.Authentication
{
    public class Launcher: AbstractSceneLauncher
    {
        protected override string GetSceneName()
        {
            return "Launcher";
        }
        
        public void Launch (Models.User user, Action completionAction)
        {
            LoadScene (
                container =>
                {
                    container
                        .Bind<IUserProvider>()
                        .FromInstance(new UserProvider { User = user });
                },
                completionAction);

        }
    }
}
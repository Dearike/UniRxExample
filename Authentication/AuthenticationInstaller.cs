using Planner.Authentication.Models;
using Planner.Authentication.UserRights;
using Planner.CommonEditor.Installers;
using Planner.Network;
using Planner.Utils;
using UnityEngine;
using Zenject;

namespace Planner.Authentication
{
    public class AuthenticationInstaller: MonoInstaller
    {
        [SerializeField]
        private AuthenticationView _authenticationView;

        [SerializeField]
        private RestorePasswordView _restorePasswordView;

        [SerializeField]
        private MonoCompositeActivatable _compositeActivable;
        
        public override void InstallBindings()
        {            
            NetworkingInstaller.Install(Container);
            ConfigInstaller.Install(Container);

            Container
                .Bind<UserRightActionProcessor>()
                .AsSingle();

            Container
                .Bind<UserProvider>()
                .AsSingle();
            
            Container
                .Bind<AuthenticationController>()
                .FromSubContainerResolve()
                .ByMethod(Install)
                .AsSingle();
        }

        private void Install(DiContainer subContainer)
        {
            subContainer
                .BindInterfacesAndSelfTo<AuthenticationController>()
                .AsSingle();

            subContainer
                .BindInstance(_authenticationView);

            subContainer
                .BindInstance(_restorePasswordView);

            subContainer
                .BindInstance(_compositeActivable);
            
            subContainer
                .Bind<Launcher>()
                .AsSingle();
        }
    }
}
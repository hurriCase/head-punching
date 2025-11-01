using Source.Scripts.Gameplay.Gloves;
using Source.Scripts.Gameplay.Head;
using Source.Scripts.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Source.Scripts.Bootstrap
{
    internal sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private HeadController _headController;
        [SerializeField] private GlovesHandler _glovesHandler;
        [SerializeField] private PunchService _punchService;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_camera);

            builder.RegisterInstance(_headController).As<IHeadController>();
            builder.RegisterInstance(_glovesHandler).As<IGlovesHandler>();
            builder.RegisterInstance(_punchService).As<IPunchService>();

            builder.Register<IA_Gameplay>(Lifetime.Singleton);
            builder.Register<InputService>(Lifetime.Singleton).As<IInputService>();

            builder.RegisterEntryPoint<EntryPoint>();
        }
    }
}
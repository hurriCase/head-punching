using Source.Scripts.Gameplay.CameraEffects;
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
        [SerializeField] private CameraShake _cameraShake;
        [SerializeField] private HeadController _headController;
        [SerializeField] private GlovesHandler _glovesHandler;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_camera);
            builder.RegisterInstance(_cameraShake).As<ICameraShake>();

            builder.RegisterInstance(_headController).As<IHeadController>();
            builder.RegisterInstance(_glovesHandler).As<IGlovesHandler>();

            builder.Register<IA_Gameplay>(Lifetime.Singleton);
            builder.Register<InputService>(Lifetime.Singleton).As<IInputService>();

            builder.RegisterEntryPoint<EntryPoint>();
        }
    }
}
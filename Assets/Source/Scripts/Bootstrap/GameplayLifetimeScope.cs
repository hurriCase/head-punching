using Source.Scripts.Gameplay;
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

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_camera);
            builder.RegisterInstance(_headController);
            builder.Register<IA_Gameplay>(Lifetime.Singleton);
            builder.Register<InputService>(Lifetime.Singleton).As<IInputService>();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
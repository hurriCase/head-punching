using PrimeTween;
using Source.Scripts.Gameplay.Gloves;
using Source.Scripts.Gameplay.Head;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Source.Scripts.Bootstrap
{
    internal sealed class EntryPoint : IStartable
    {
        [Inject] private IHeadController _headController;
        [Inject] private IGlovesHandler _glovesHandler;

        public void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;

            Application.targetFrameRate = 60;

            PrimeTweenConfig.warnTweenOnDisabledTarget = false;

            _headController.Init();
            _glovesHandler.Init();
        }
    }
}
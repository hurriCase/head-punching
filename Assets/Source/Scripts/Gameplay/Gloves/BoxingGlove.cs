using Cysharp.Threading.Tasks;
using R3;
using Source.Scripts.Gameplay.Head;
using UnityEngine;
using VContainer;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class BoxingGlove : MonoBehaviour
    {
        [SerializeField] private GloveMovementHandler _gloveMovementHandler;
        [SerializeField] private PunchChargeHandler _punchChargeHandler;

        [SerializeField] private Transform _punchTransform;

        [Inject] private IHeadController _headController;
        [Inject] private IPunchService _punchService;

        private bool _isPunching;
        private Vector3 _basePunchLocalPosition;
        private Quaternion _basePunchLocalRotation;

        internal void Init(Observable<Unit> onMousePressed, Observable<Unit> onMouseReleased)
        {
            _gloveMovementHandler.Init();

            _basePunchLocalPosition = _punchTransform.localPosition;
            _basePunchLocalRotation = _punchTransform.localRotation;

            onMousePressed
                .Where(this, static (_, self) => self._isPunching is false)
                .Subscribe(this, static (_, self) => self._punchChargeHandler.StartCharge())
                .RegisterTo(destroyCancellationToken);

            onMouseReleased
                .Where(this, static (_, self) => self._isPunching is false)
                .Do(this, static (_, self) => self.ExecutePunch().Forget())
                .Subscribe(this, static (_, self) => self._punchChargeHandler.ReleaseCharge())
                .RegisterTo(destroyCancellationToken);
        }

        private async UniTask ExecutePunch()
        {
            _isPunching = true;

            var startPoint = _punchTransform.position;
            var punchTarget = _headController.GetPunchTarget(startPoint);
            await _punchService.ExecutePunch(_punchTransform, startPoint, punchTarget);

            CompletePunch(startPoint, punchTarget);
            ResetPunchTransform();
        }

        private void CompletePunch(Vector3 startPoint, Vector3 punchTarget)
        {
            var punchDirection = (punchTarget - startPoint).normalized;
            var forceMultiplier = _punchChargeHandler.ReleaseCharge();

            _headController.ApplyPunchImpact(punchTarget, punchDirection, forceMultiplier);

            _isPunching = false;
        }

        private void ResetPunchTransform()
        {
            _punchTransform.localPosition = _basePunchLocalPosition;
            _punchTransform.localRotation = _basePunchLocalRotation;
        }
    }
}
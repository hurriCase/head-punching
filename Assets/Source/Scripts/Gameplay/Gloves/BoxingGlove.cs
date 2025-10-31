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

        private bool _isPunching;

        internal void Init(Observable<Unit> onMousePressed, Observable<Unit> onMouseReleased)
        {
            _gloveMovementHandler.Init();

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

            var punchData = new PunchData(_punchTransform, _headController);

            await _gloveMovementHandler.ExecutePunch(punchData.LocalPunchTarget, punchData.LocalTargetRotation);

            CompletePunch(punchData.PunchStartPosition, punchData.CurrentPunchTarget);
        }

        private void CompletePunch(Vector3 punchStartPosition, Vector3 currentPunchTarget)
        {
            var punchDirection = (currentPunchTarget - punchStartPosition).normalized;
            var forceMultiplier = _punchChargeHandler.ReleaseCharge();

            _headController.ApplyPunchImpact(currentPunchTarget, punchDirection, forceMultiplier);

            _isPunching = false;
        }
    }
}
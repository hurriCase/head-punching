using Cysharp.Threading.Tasks;
using R3;
using Source.Scripts.Gameplay.Gloves.PunchAnimation;
using Source.Scripts.Gameplay.Gloves.PunchAnimation.Animation;
using Source.Scripts.Gameplay.Head;
using UnityEngine;
using VContainer;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class BoxingGlove : MonoBehaviour
    {
        [SerializeField] private PunchChargeHandler _punchChargeHandler;

        [SerializeField] private Transform _punchTransform;
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private bool _invertRotation;

        [Inject] private IHeadController _headController;

        internal SplineAnimation PunchAnimation { get; set; }
        private bool _isPunching;

        internal void Init(Observable<Unit> onMousePressed, Observable<Unit> onMouseReleased)
        {
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

            var animationConfig = new SplineAnimationConfig(
                _punchTransform,
                _visualTransform,
                startPoint,
                punchTarget,
                _invertRotation,
                this,
                static (self, startPoint, endPoint) => self.ProvideImpact(startPoint, endPoint));

            await PunchAnimation.Animate(animationConfig);

            _isPunching = false;
        }

        private void ProvideImpact(Vector3 startPoint, Vector3 punchTarget)
        {
            var punchDirection = (punchTarget - startPoint).normalized;
            var forceMultiplier = _punchChargeHandler.ReleaseCharge();

            _headController.ApplyPunchImpact(punchTarget, punchDirection, forceMultiplier);
        }
    }
}
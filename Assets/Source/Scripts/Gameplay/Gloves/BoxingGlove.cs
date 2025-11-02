using AYellowpaper.SerializedCollections;
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
        [SerializeField] private SerializedDictionary<PunchType, SplineAnimation> _punches;
        [SerializeField] private PunchChargeHandler _punchChargeHandler;
        [SerializeField] private Transform _punchTransform;
        [SerializeField] private Transform _visualTransform;

        [Header("Punch Detection")]
        [SerializeField] private float _uppercutAngleThreshold = 45f;
        [SerializeField] private float _hookAngleThreshold = 60f;

        [Inject] private IHeadController _headController;

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

            var punchType = DeterminePunchType(startPoint, punchTarget);
            var punchAnimation = _punches[punchType];

            var animationConfig = new SplineAnimationConfig(
                _punchTransform,
                _visualTransform,
                startPoint,
                punchTarget,
                this,
                static (self, startPoint, endPoint) => self.ProvideImpact(startPoint, endPoint));

            await punchAnimation.Animate(animationConfig);

            _isPunching = false;
        }

        private PunchType DeterminePunchType(Vector3 startPoint, Vector3 targetPoint)
        {
            var direction = (targetPoint - startPoint).normalized;
            var verticalAngle = Vector3.Angle(Vector3.up, direction);

            if (verticalAngle < _uppercutAngleThreshold)
                return PunchType.Uppercut;

            var horizontalDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            var forwardDirection = Vector3.forward;
            var horizontalAngle = Vector3.Angle(forwardDirection, horizontalDirection);

            return horizontalAngle > _hookAngleThreshold ? PunchType.Hook : PunchType.Jab;
        }

        private void ProvideImpact(Vector3 startPoint, Vector3 punchTarget)
        {
            var punchDirection = (punchTarget - startPoint).normalized;
            var forceMultiplier = _punchChargeHandler.ReleaseCharge();

            _headController.ApplyPunchImpact(punchTarget, punchDirection, forceMultiplier);
        }
    }
}
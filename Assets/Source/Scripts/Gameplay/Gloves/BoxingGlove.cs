using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using R3;
using Source.Scripts.Gameplay.Gloves.PunchAnimation;
using Source.Scripts.Gameplay.Gloves.PunchAnimation.Animation;
using Source.Scripts.Gameplay.Head;
using Source.Scripts.Input;
using UnityEngine;
using VContainer;

namespace Source.Scripts.Gameplay.Gloves
{
    internal sealed class BoxingGlove : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<HeadSide, SplineAnimation> _punches;
        [SerializeField] private PunchChargeHandler _punchChargeHandler;
        [SerializeField] private Transform _punchTransform;
        [SerializeField] private Transform _visualTransform;

        [SerializeField] private float _uppercutAngleThreshold;
        [SerializeField] private float _hookAngleThreshold;

        [Inject] private IHeadController _headController;
        [Inject] private IInputService _inputService;
        [Inject] private Camera _camera;

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
            var headSide = DetermineHeadSide();
            var punchAnimation = _punches[headSide];
            var punchTargetOffset = punchAnimation.PunchTargetOffset;
            var punchTarget = _headController.GetPunchTarget(startPoint, headSide, punchTargetOffset);

            var animationConfig = new SplineAnimationConfig(
                _punchTransform,
                _visualTransform,
                startPoint,
                punchTarget,
                this,
                headSide,
                static (self, startPoint, headSide) => self.ProvideImpact(startPoint, headSide));

            await punchAnimation.Animate(animationConfig);

            _isPunching = false;
        }

        private HeadSide DetermineHeadSide()
        {
            var meshCollider = _headController.MeshCollider;
            var mousePosition = _inputService.CurrentMousePosition.CurrentValue;
            var mouseRay = _camera.ScreenPointToRay(mousePosition);
            var maxDistance = (meshCollider.bounds.center - _punchTransform.position).magnitude;

            return meshCollider.Raycast(mouseRay, out _, float.PositiveInfinity)
                ? HeadSide.Center
                : GetRelativeSide(mouseRay);
        }

        private HeadSide GetRelativeSide(Ray mouseRay)
        {
            var meshCollider = _headController.MeshCollider;
            var plane = new Plane(Vector3.forward, meshCollider.bounds.center);

            var clickPosition = plane.Raycast(mouseRay, out var distance)
                ? mouseRay.GetPoint(distance)
                : meshCollider.bounds.center;

            var colliderCenter = meshCollider.bounds.center;
            var direction = clickPosition - colliderCenter;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x > 0 ? HeadSide.Right : HeadSide.Left;

            return direction.y > 0 ? HeadSide.Top : HeadSide.Bottom;
        }

        private void ProvideImpact(Vector3 punchTarget, HeadSide headSide)
        {
            var punchDirection = headSide switch
            {
                HeadSide.Center => Vector3.forward,
                HeadSide.Left => Vector3.right,
                HeadSide.Right => Vector3.left,
                HeadSide.Top => Vector3.down,
                HeadSide.Bottom => Vector3.up,
                _ => Vector3.forward
            };
            var forceMultiplier = _punchChargeHandler.ReleaseCharge();

            _headController.ApplyPunchImpact(punchTarget, punchDirection, forceMultiplier);
        }
    }
}
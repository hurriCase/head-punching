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
                .Subscribe(this, static (_, self) => self.ExecutePunchAsync().Forget())
                .RegisterTo(destroyCancellationToken);
        }

        private async UniTask ExecutePunchAsync()
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
                punchTarget);

            var punchTask = punchAnimation.Animate(animationConfig);

            await punchAnimation.OnPunchFinished.FirstAsync(destroyCancellationToken);

            ApplyPunchImpact(punchTarget, headSide);

            await punchTask;

            _isPunching = false;
        }

        private HeadSide DetermineHeadSide()
        {
            var meshCollider = _headController.MeshCollider;
            var mousePosition = _inputService.CurrentMousePosition.CurrentValue;
            var mouseRay = _camera.ScreenPointToRay(mousePosition);
            var maxDistance = Vector3.Distance(meshCollider.bounds.center, _punchTransform.position)
                              + meshCollider.bounds.extents.magnitude;

            return meshCollider.Raycast(mouseRay, out _, maxDistance)
                ? HeadSide.Center
                : GetRelativeSide(mouseRay);
        }

        private HeadSide GetRelativeSide(Ray mouseRay)
        {
            var meshCollider = _headController.MeshCollider;
            var colliderCenter = meshCollider.bounds.center;
            var plane = new Plane(Vector3.forward, colliderCenter);

            var clickPosition = plane.Raycast(mouseRay, out var distance)
                ? mouseRay.GetPoint(distance)
                : colliderCenter;

            var direction = clickPosition - colliderCenter;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x > 0 ? HeadSide.Right : HeadSide.Left;

            return direction.y > 0 ? HeadSide.Top : HeadSide.Bottom;
        }

        private void ApplyPunchImpact(Vector3 punchTarget, HeadSide headSide)
        {
            var punchDirection = GetPunchDirection(headSide);
            var forceMultiplier = _punchChargeHandler.ReleaseCharge();

            _headController.ApplyPunchImpact(punchTarget, punchDirection, forceMultiplier);
        }

        private Vector3 GetPunchDirection(HeadSide headSide) => headSide switch
        {
            HeadSide.Center => Vector3.forward,
            HeadSide.Left => Vector3.right,
            HeadSide.Right => Vector3.left,
            HeadSide.Top => Vector3.down,
            HeadSide.Bottom => Vector3.up,
            _ => Vector3.forward
        };
    }
}
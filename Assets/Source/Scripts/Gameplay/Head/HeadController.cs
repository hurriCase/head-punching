using System;
using Source.Scripts.Gameplay.Gloves.PunchAnimation;
using UnityEngine;

namespace Source.Scripts.Gameplay.Head
{
    internal sealed class HeadController : MonoBehaviour, IHeadController
    {
        [field: SerializeField] public MeshCollider MeshCollider { get; private set; }

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private LocalizedMeshDeformation _meshDeformation;

        [SerializeField] private float _basePunchForce;

        public void Init()
        {
            _meshDeformation.Init();
        }

        public void ApplyPunchImpact(Vector3 impactPoint, Vector3 punchDirection, float forceMultiplier)
        {
            var force = punchDirection * (_basePunchForce * forceMultiplier);
            _rigidbody.AddForce(force, ForceMode.Impulse);

            _meshDeformation.ApplyDeformationAtPoint(impactPoint, force, forceMultiplier);
        }

        public Vector3 GetPunchTarget(Vector3 position, HeadSide headSide, Vector3 punchTargetOffset)
        {
            if (headSide == HeadSide.Center)
                return MeshCollider.ClosestPoint(position + punchTargetOffset);

            var headCenter = MeshCollider.bounds.center;
            var distance = position.z - headCenter.z;
            var shiftedPosition = headSide switch
            {
                HeadSide.Left => new Vector3(distance, headCenter.y, headCenter.z),
                HeadSide.Right => new Vector3(-distance, headCenter.y, headCenter.z),
                HeadSide.Top => new Vector3(headCenter.x, -distance, headCenter.z),
                HeadSide.Bottom => new Vector3(headCenter.x, distance, headCenter.z),
                _ => throw new ArgumentOutOfRangeException(nameof(headSide), headSide, null)
            };
            return MeshCollider.ClosestPoint(shiftedPosition + punchTargetOffset);
        }
    }
}
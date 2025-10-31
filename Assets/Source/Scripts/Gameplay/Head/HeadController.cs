using UnityEngine;

namespace Source.Scripts.Gameplay.Head
{
    internal sealed class HeadController : MonoBehaviour, IHeadController
    {
        [SerializeField] private MeshCollider _meshCollider;
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

        public Vector3 GetPunchTarget(Vector3 position) => _meshCollider.ClosestPoint(position);
    }
}
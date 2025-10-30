using UnityEngine;

namespace Source.Scripts.Gameplay
{
    internal sealed class HeadController : MonoBehaviour
    {
        [field: SerializeField] internal MeshCollider MeshCollider { get; private set; }

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private ConfigurableJoint _joint;

        [SerializeField] private float _punchForceMultiplier;

        internal void ApplyPunchImpact(Vector3 punchDirection)
        {
            var force = punchDirection * _punchForceMultiplier;
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
}
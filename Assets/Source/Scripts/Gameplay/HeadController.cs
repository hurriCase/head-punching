using PrimeTween;
using UnityEngine;

namespace Source.Scripts.Gameplay
{
    internal sealed class HeadController : MonoBehaviour
    {
        [field: SerializeField] internal MeshCollider MeshCollider { get; private set; }
        [field: SerializeField] internal Transform Transform { get; private set; }
    }
}
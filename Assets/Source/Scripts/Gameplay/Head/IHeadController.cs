using UnityEngine;

namespace Source.Scripts.Gameplay.Head
{
    internal interface IHeadController
    {
        void Init();
        void ApplyPunchImpact(Vector3 impactPoint, Vector3 punchDirection);
        Vector3 GetPunchTarget(Vector3 position);
    }
}
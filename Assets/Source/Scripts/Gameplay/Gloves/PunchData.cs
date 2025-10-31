using Source.Scripts.Gameplay.Head;
using UnityEngine;

namespace Source.Scripts.Gameplay.Gloves
{
    internal readonly struct PunchData
    {
        internal Vector3 LocalPunchTarget { get; }
        internal Quaternion LocalTargetRotation { get; }
        internal Vector3 CurrentPunchTarget { get; }
        internal Vector3 PunchStartPosition { get; }

        internal PunchData(Transform punchTransform, IHeadController headController)
        {
            PunchStartPosition = punchTransform.position;
            CurrentPunchTarget = headController.GetPunchTarget(PunchStartPosition);

            var direction = CurrentPunchTarget - PunchStartPosition;
            var targetRotation = Quaternion.LookRotation(direction);

            var parent = punchTransform.parent;
            LocalPunchTarget = parent.InverseTransformPoint(CurrentPunchTarget);
            LocalTargetRotation = Quaternion.Inverse(parent.rotation) * targetRotation;
        }
    }
}
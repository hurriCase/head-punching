using Unity.Mathematics;
using UnityEngine.Splines;

namespace Source.Scripts.Gameplay.Gloves.PunchAnimation.Animation
{
    internal readonly struct InterpolatedQuaternion : IInterpolator<quaternion>
    {
        public quaternion Interpolate(quaternion from, quaternion to, float t) => math.slerp(from, to, t);
    }
}
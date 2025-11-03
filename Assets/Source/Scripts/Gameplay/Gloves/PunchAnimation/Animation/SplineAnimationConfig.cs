using System;
using UnityEngine;

namespace Source.Scripts.Gameplay.Gloves.PunchAnimation.Animation
{
    [Serializable]
    internal struct SplineAnimationConfig
    {
        internal Transform PositionTarget { get; }
        internal Transform RotationTarget { get; }
        internal Vector3 StartPoint { get; }
        internal Vector3 EndPoint { get; }
        internal BoxingGlove BoxingGlove { get; }
        internal HeadSide HeadSide { get; }
        internal Action<BoxingGlove, Vector3, HeadSide> AnimationFinished { get; }

        internal SplineAnimationConfig(
            Transform positionTarget,
            Transform rotationTarget,
            Vector3 startPoint,
            Vector3 endPoint,
            BoxingGlove boxingGlove = null,
            HeadSide headSide = default,
            Action<BoxingGlove, Vector3, HeadSide> animationFinished = null)
        {
            PositionTarget = positionTarget;
            RotationTarget = rotationTarget;
            StartPoint = startPoint;
            EndPoint = endPoint;
            BoxingGlove = boxingGlove;
            HeadSide = headSide;
            AnimationFinished = animationFinished;
        }
    }
}
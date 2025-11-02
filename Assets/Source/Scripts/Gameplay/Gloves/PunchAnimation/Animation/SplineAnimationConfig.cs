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
        internal bool InvertRotation { get; }

        internal SplineAnimationConfig(
            Transform positionTarget,
            Transform rotationTarget,
            Vector3 startPoint,
            Vector3 endPoint,
            bool invertRotation)
        {
            PositionTarget = positionTarget;
            RotationTarget = rotationTarget;
            StartPoint = startPoint;
            EndPoint = endPoint;
            InvertRotation = invertRotation;
        }
    }
}
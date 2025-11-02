using UnityEngine;

namespace Source.Scripts.Gameplay.Gloves
{
    internal readonly struct SplineTransformation
    {
        internal Vector3 SplineStartPoint { get; }
        internal Quaternion Rotation { get; }
        internal float Scale { get; }

        internal SplineTransformation(Vector3 splineStartPoint, Quaternion rotation, float scale)
        {
            SplineStartPoint = splineStartPoint;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
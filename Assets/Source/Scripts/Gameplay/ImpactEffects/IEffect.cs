using UnityEngine;

namespace Source.Scripts.Gameplay.ImpactEffects
{
    internal abstract class ImpactEffectBase : MonoBehaviour
    {
        internal abstract void Play(float intensity);
    }
}
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Source.Scripts.Gameplay.CameraEffects
{
    internal sealed class PunchSoundEffect : MonoBehaviour, IPunchSoundEffect
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private SerializedDictionary<float, List<AudioClip>> _punchSoundEffect;

        public void Play(float intensity)
        {
            foreach (var (clipIntensity, audioClips) in _punchSoundEffect)
            {
                if (intensity < clipIntensity)
                    continue;

                var randomClip = audioClips[Random.Range(0, audioClips.Count)];
                _audioSource.PlayOneShot(randomClip);
            }
        }
    }
}
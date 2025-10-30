using System;
using PrimeTween;
using UnityEngine;

namespace Source.Scripts.Gameplay.Head
{
    internal sealed class LocalizedMeshDeformation : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private float _deformationRadius;
        [SerializeField] private float _deformationStrength;
        [SerializeField] private float _recoveryDuration;

        private Mesh _mesh;
        private Vector3[] _originalVertices;
        private Vector3[] _displacedVertices;
        private Vector3[] _recoveryStartVertices;

        private Tween _recoveryTween;

        internal void Init()
        {
            _mesh = _meshFilter.mesh;
            _originalVertices = _mesh.vertices;
            _displacedVertices = new Vector3[_originalVertices.Length];
            _recoveryStartVertices = new Vector3[_originalVertices.Length];

            Array.Copy(_originalVertices, _displacedVertices, _originalVertices.Length);
        }

        internal void ApplyDeformationAtPoint(Vector3 worldPoint, Vector3 force)
        {
            var meshTransform = _meshFilter.transform;
            var localPoint = meshTransform.InverseTransformPoint(worldPoint);
            var forceDirection = meshTransform.InverseTransformDirection(force.normalized);

            var hasDeformation = false;

            for (var i = 0; i < _originalVertices.Length; i++)
            {
                var distance = Vector3.Distance(_originalVertices[i], localPoint);

                if (distance > _deformationRadius)
                    continue;

                var falloff = 1f - distance / _deformationRadius;
                var displacement = forceDirection * (_deformationStrength * falloff);

                _displacedVertices[i] = _originalVertices[i] + displacement;
                hasDeformation = true;
            }

            if (hasDeformation is false)
                return;

            UpdateMesh();
            StartRecovery();
        }

        private void StartRecovery()
        {
            _recoveryTween.Stop();

            Array.Copy(_displacedVertices, _recoveryStartVertices, _displacedVertices.Length);

            _recoveryTween = Tween.Custom(this, 0f, 1f, _recoveryDuration,
                onValueChange: static (self, progress) => self.RecoverMesh(progress));
        }

        private void RecoverMesh(float progress)
        {
            for (var i = 0; i < _displacedVertices.Length; i++)
                _displacedVertices[i] = Vector3.Lerp(_recoveryStartVertices[i], _originalVertices[i], progress);

            UpdateMesh();
        }

        private void UpdateMesh()
        {
            _mesh.vertices = _displacedVertices;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
        }
    }
}
using System.Linq;
using Cysharp.Threading.Tasks;
using Source.Scripts.Gameplay.Gloves.PunchAnimation.Animation;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Source.Editor
{
    [CustomEditor(typeof(SplineAnimation))]
    internal sealed class SplineRotationDataEditor : UnityEditor.Editor
    {
        private SplineAnimation _splineAnimation;
        private SerializedProperty _timeCurveProperty;

        private float _previewProgress;
        private int _knotIndex;

        private const string PreviewProgressKey = KeyPrefix + "PreviewProgress";
        private const string KnotIndexKey = KeyPrefix + "KnotIndex";
        private const string KeyPrefix = "splineAnimation.";

        private void OnEnable()
        {
            _previewProgress = EditorPrefs.GetFloat(PreviewProgressKey, 0f);
            _knotIndex = EditorPrefs.GetInt(KnotIndexKey, 0);

            _splineAnimation = (SplineAnimation)target;
            _timeCurveProperty = FindField(nameof(_splineAnimation.PunchSettings));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var timeCurvePropertyName = GetBackingField(nameof(_splineAnimation.PunchSettings));
            DrawPropertiesExcluding(serializedObject, timeCurvePropertyName);

            if (!_splineAnimation.SplineContainer
                || !_splineAnimation.PreviewPositionTarget
                || !_splineAnimation.PreviewRotationTarget)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawKnotEditing();
            DrawPreview();

            if (GUILayout.Button("Sync With Knots"))
                SyncRotationDataWithKnots();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPreview()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_timeCurveProperty);

            _previewProgress = EditorGUILayout.Slider("Preview Progress", _previewProgress, 0f, 1f);

            if (GUILayout.Button("Start animation"))
                AnimatePreview();

            if (EditorGUI.EndChangeCheck() is false)
                return;

            PreviewAtProgress(_previewProgress);
            EditorPrefs.SetFloat(PreviewProgressKey, _previewProgress);
        }

        private void DrawKnotEditing()
        {
            var knotCount = _splineAnimation.SplineContainer.Spline.Count;

            EditorGUI.BeginChangeCheck();
            _knotIndex = EditorGUILayout.IntSlider("Knot Index", _knotIndex, 0, knotCount - 1);
            if (EditorGUI.EndChangeCheck())
            {
                var normalizedProgress = _splineAnimation.SplineContainer.Spline.ConvertIndexUnit(
                    _knotIndex,
                    PathIndexUnit.Knot,
                    PathIndexUnit.Normalized);

                PreviewAtProgress(normalizedProgress);
                EditorPrefs.SetInt(KnotIndexKey, _knotIndex);
            }

            var rotationTarget = _splineAnimation.PreviewRotationTarget;

            if (GUILayout.Button("Capture Current Rotation"))
                CaptureRotationForCurrentKnot(rotationTarget.rotation);
        }

        private void CaptureRotationForCurrentKnot(Quaternion rotation)
        {
            using var undoScope = new UndoScope(_splineAnimation, serializedObject, "Capture Rotation");

            while (_splineAnimation.RotationData.Count <= _knotIndex)
            {
                var index = _splineAnimation.RotationData.Count;
                var defaultRotation = _splineAnimation.SplineContainer.Spline[index].Rotation;
                _splineAnimation.RotationData.Add(new DataPoint<quaternion>(index, defaultRotation));
            }

            _splineAnimation.RotationData[_knotIndex] = new DataPoint<quaternion>(_knotIndex, rotation);
        }

        private void SyncRotationDataWithKnots()
        {
            var spline = _splineAnimation.SplineContainer.Spline;
            var knotCount = spline.Count;

            using var undoScope = new UndoScope(_splineAnimation, serializedObject, "Sync Rotations");

            _splineAnimation.RotationData.Clear();

            for (var i = 0; i < knotCount; i++)
            {
                var knotRotation = spline[i].Rotation;
                var rotationData = new DataPoint<quaternion>(i, knotRotation);
                _splineAnimation.RotationData.Add(rotationData);
            }
        }

        private void AnimatePreview()
        {
            var splineTransform = _splineAnimation.SplineContainer.transform;
            var knots = _splineAnimation.SplineContainer.Spline.Knots.ToArray();

            var startPosition = splineTransform.TransformPoint(knots[0].Position);
            var endPosition = splineTransform.TransformPoint(knots[^1].Position);

            var config = new SplineAnimationConfig(
                _splineAnimation.PreviewPositionTarget,
                _splineAnimation.PreviewRotationTarget,
                startPosition,
                endPosition,
                false);

            _splineAnimation.Animate(config).Forget();
        }

        private void PreviewAtProgress(float normalizedProgress)
        {
            var (position, rotation) = _splineAnimation.EvaluateSplineTransform(normalizedProgress);
            _splineAnimation.PreviewPositionTarget.position = position;
            _splineAnimation.PreviewRotationTarget.rotation = rotation;
        }

        private SerializedProperty FindField(string name) => serializedObject.FindProperty(GetBackingField(name));
        private string GetBackingField(string name) => $"<{name}>k__BackingField";
    }
}
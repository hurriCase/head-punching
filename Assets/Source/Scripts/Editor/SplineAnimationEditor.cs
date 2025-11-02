using Source.Scripts.Gameplay.Gloves;
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
            _timeCurveProperty = FindField(nameof(_splineAnimation.TimeCurve));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var timeCurvePropertyName = GetBackingField(nameof(_splineAnimation.TimeCurve));
            DrawPropertiesExcluding(serializedObject, timeCurvePropertyName);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_timeCurveProperty);
            var curveChanged = EditorGUI.EndChangeCheck();

            if (!_splineAnimation.SplineContainer)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawKnotEditing();

            EditorGUI.BeginChangeCheck();
            _previewProgress = EditorGUILayout.Slider("Preview Progress", _previewProgress, 0f, 1f);
            if (EditorGUI.EndChangeCheck() || curveChanged)
            {
                _splineAnimation.PreviewAtProgress(_previewProgress);
                EditorPrefs.SetFloat(PreviewProgressKey, _previewProgress);
            }

            if (GUILayout.Button("Sync With Knots"))
                SyncRotationDataWithKnots(_splineAnimation);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKnotEditing()
        {
            var knotCount = _splineAnimation.SplineContainer.Spline.Count;

            EditorGUI.BeginChangeCheck();
            _knotIndex = EditorGUILayout.IntSlider("Knot Index", _knotIndex, 0, knotCount - 1);
            if (EditorGUI.EndChangeCheck())
            {
                _splineAnimation.PreviewAtKnot(_knotIndex);
                EditorPrefs.SetInt(KnotIndexKey, _knotIndex);
            }

            var rotationTarget = _splineAnimation.RotationTarget;

            if (!rotationTarget || GUILayout.Button("Capture Current Rotation"))
                CaptureRotationForCurrentKnot(rotationTarget.rotation);
        }

        private void CaptureRotationForCurrentKnot(Quaternion rotation)
        {
            Undo.RecordObject(_splineAnimation, "Capture Rotation");

            while (_splineAnimation.RotationData.Count <= _knotIndex)
            {
                var index = _splineAnimation.RotationData.Count;
                var defaultRotation = _splineAnimation.SplineContainer.Spline[index].Rotation;
                _splineAnimation.RotationData.Add(new DataPoint<quaternion>(index, defaultRotation));
            }

            _splineAnimation.RotationData[_knotIndex] = new DataPoint<quaternion>(_knotIndex, rotation);

            EditorUtility.SetDirty(_splineAnimation);
            serializedObject.Update();
        }

        private void SyncRotationDataWithKnots(SplineAnimation data)
        {
            var spline = data.SplineContainer.Spline;
            var knotCount = spline.Count;

            Undo.RecordObject(data, "Sync Rotations");

            data.RotationData.Clear();

            for (var i = 0; i < knotCount; i++)
            {
                var knotRotation = spline[i].Rotation;
                var rotationData = new DataPoint<quaternion>(i, knotRotation);
                data.RotationData.Add(rotationData);
            }

            EditorUtility.SetDirty(data);
            serializedObject.Update();
        }

        private SerializedProperty FindField(string name) => serializedObject.FindProperty(GetBackingField(name));
        private string GetBackingField(string name) => $"<{name}>k__BackingField";
    }
}
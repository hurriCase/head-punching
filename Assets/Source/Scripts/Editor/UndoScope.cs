using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Source.Editor
{
    internal readonly struct UndoScope : IDisposable
    {
        private readonly SerializedObject _serializedObject;
        private readonly Object _target;

        internal UndoScope(Object target, SerializedObject serializedObject, string name)
        {
            _target = target;
            _serializedObject = serializedObject;

            Undo.RecordObject(target, name);
        }

        public void Dispose()
        {
            EditorUtility.SetDirty(_target);
            _serializedObject.Update();
        }
    }
}
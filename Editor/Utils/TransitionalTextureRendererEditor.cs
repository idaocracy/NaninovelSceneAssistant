using UnityEditor;
using UnityEngine;
using Naninovel;

namespace NaninovelSceneAssistant
{
    [CustomEditor(typeof(TransitionalTextureRenderer))]
    public class TransitionalTextureRendererEditor : Editor
    {
        private SerializedProperty renderRectangleProp;

        private TransitionalTextureRenderer renderer;

        private void OnEnable()
        {
            renderer = (TransitionalTextureRenderer)serializedObject.targetObject;
            renderRectangleProp = serializedObject.FindProperty("<RenderRectangle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            renderer.RenderRectangle = EditorGUILayout.RectField(
                new GUIContent("Render Rectangle", "Modifier of the texture areas to render."),
                renderer.RenderRectangle
            );

            if (serializedObject.ApplyModifiedProperties())
            {
                var renderer = (TransitionalTextureRenderer)target;
                EditorUtility.SetDirty(renderer);
            }
        }

        private void OnSceneGUI()
        {
            var renderer = (TransitionalTextureRenderer)target;
            var rect = renderer.RenderRectangle;

            Handles.color = Color.green;
            Vector3 pos = renderer.transform.position;
            Vector3 size = new Vector3(rect.width, rect.height, 0);
            Vector3 center = pos + new Vector3(rect.x + rect.width / 2, rect.y + rect.height / 2, 0);

            EditorGUI.BeginChangeCheck();

            var newCenter = Handles.PositionHandle(center, Quaternion.identity);
            var newSize = Handles.ScaleHandle(size, center, Quaternion.identity, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(renderer, "Modify Render Rectangle");
                renderer.RenderRectangle = new Rect(
                    newCenter.x - newSize.x / 2 - pos.x,
                    newCenter.y - newSize.y / 2 - pos.y,
                    newSize.x,
                    newSize.y
                );
                EditorUtility.SetDirty(renderer);
            }
        }
    }
}

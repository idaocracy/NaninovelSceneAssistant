using UnityEngine;
using System.Collections.Generic;
using Naninovel;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace NaninovelSceneAssistant
{
    public abstract class SceneAssistantSpawnObject : MonoBehaviour, ISceneAssistantSpawn
    {
        public abstract bool IsTransformable { get; }
        public abstract string CommandId { get; }
        public string SpawnId => GetType().Name;
        public abstract List<ParameterValue> GetParams();

        public SpawnData ObjectSpawnData { get; private set; }

        private SceneAssistantManager sceneAssistantManager;

        protected virtual void Awake() => sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
        protected virtual void OnEnable() => sceneAssistantManager.OnSceneAssistantReset += GetSpawnData;
        protected virtual void OnDisable() => sceneAssistantManager.OnSceneAssistantReset -= GetSpawnData;
        protected virtual void GetSpawnData() => ObjectSpawnData = sceneAssistantManager.ObjectList[SpawnId] as SpawnData;
    }

#if UNITY_EDITOR 

    [CustomEditor(typeof(SceneAssistantSpawnObject))]
    public abstract class SpawnObjectEditor : Editor
    {
        protected SceneAssistantSpawnObject spawnObject;
        private static bool logResult;
        private static bool showDefaultValues;

        protected virtual void Awake() => spawnObject = (SceneAssistantSpawnObject)target;

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Scene Assistant Options:", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawButton(target.name, logResult:logResult);
            GUILayout.Space(5f);
            DrawButton(target.name, inlined:true, logResult:logResult);
            GUILayout.Space(5f);
            DrawButton(target.name, paramsOnly: true, logResult: logResult);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            if(!string.IsNullOrEmpty(spawnObject.CommandId))
            { 
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawButton(target.name, isSpawnEffect:true, logResult: logResult);
                GUILayout.Space(5f);
                DrawButton(target.name, isSpawnEffect: true, inlined: true, logResult: logResult);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(logResult, "Log Results")) logResult = true;
            else logResult = false;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            showDefaultValues = EditorGUILayout.Foldout(showDefaultValues, "Default values");
            serializedObject.Update();
            if (showDefaultValues) DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
            serializedObject.ApplyModifiedProperties();
        }

        public void DrawButton(string name, bool isSpawnEffect = false, bool inlined = false, bool paramsOnly = false, bool logResult = false)
        {
            if (GUILayout.Button(paramsOnly ? "params" : (inlined ? "[" + (isSpawnEffect ? ParameterValue.GetFormattedName(name)  : "spawn") + "]" : "@" + (isSpawnEffect ? ParameterValue.GetFormattedName(name) : "spawn")), GUILayout.Height(30), GUILayout.MaxWidth(150)))
            {
                var spawnString = isSpawnEffect ? spawnObject.ObjectSpawnData.GetSpawnEffectLine(inlined, paramsOnly) : spawnObject.ObjectSpawnData.GetCommandLine(inlined, paramsOnly);
                EditorGUIUtility.systemCopyBuffer = spawnString;
                if (logResult) Debug.Log(spawnString);
            }
        }
    }

#endif
}

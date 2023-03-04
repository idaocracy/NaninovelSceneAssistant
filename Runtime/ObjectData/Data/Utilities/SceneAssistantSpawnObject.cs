#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public abstract class SceneAssistantSpawnObject : MonoBehaviour, ISceneAssistantSpawn
    {
        public abstract bool IsTransformable { get; }
        public virtual string CommandId => GetType().Name;
        public abstract List<ParameterValue> GetParams();
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SceneAssistantSpawnObject))]
    public abstract class SpawnObjectEditor : Editor
    {
        protected SceneAssistantSpawnObject spawnObject;
        private static bool logResult;
        private static bool showDefaultValues;
        protected ISceneAssistantSpawn sceneAssistant;
        private SceneAssistantManager sceneAssistantManager;
        private SpawnData spawnData;

        protected virtual void Awake()
        {
            spawnObject = (SceneAssistantSpawnObject)target;
            sceneAssistant = spawnObject.gameObject.GetComponent<ISceneAssistantSpawn>() ?? null;
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
            if (sceneAssistantManager.Initialised) spawnData = sceneAssistantManager.ObjectList[spawnObject.CommandId] as SpawnData;
            else sceneAssistantManager.OnSceneAssistantReset += () => spawnData = sceneAssistantManager.ObjectList[spawnObject.CommandId] as SpawnData; 
        }

        public override void OnInspectorGUI()
        {
            if (sceneAssistant != null)
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

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawButton(target.name, isSpawnEffect:true, logResult: logResult);
                GUILayout.Space(5f);
                DrawButton(target.name, isSpawnEffect: true, inlined: true, logResult: logResult);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

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

        }

        public void DrawButton(string name, bool isSpawnEffect = false, bool inlined = false, bool paramsOnly = false, bool logResult = false)
        {
            if (GUILayout.Button(paramsOnly ? "params" : (inlined ? "[" + (isSpawnEffect ? ParameterValue.GetFormattedName(name)  : "spawn") + "]" : "@" + (isSpawnEffect ? ParameterValue.GetFormattedName(name) : "spawn")), GUILayout.Height(30), GUILayout.MaxWidth(150)))
            {
                var spawnString = isSpawnEffect ? spawnData.GetSpawnEffectLine(inlined, paramsOnly) : spawnData.GetCommandLine(inlined, paramsOnly);
                EditorGUIUtility.systemCopyBuffer = spawnString;
                if (logResult) Debug.Log(spawnString);
            }
        }
    }

#endif
}

#endif
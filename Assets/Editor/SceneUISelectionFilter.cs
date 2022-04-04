﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;

public class SceneUISelectionFilter : EditorWindow
{
    [MenuItem("Tools/Scene UI Selection Filter")]
    public static void ShowWindow()
    {
        Rect rect = new Rect();
        if (SceneView.lastActiveSceneView != null)
            rect = SceneView.lastActiveSceneView.position;
        rect.size = new Vector2(230, 100);
        rect.position = rect.position + new Vector2(10, 10);
        var win = EditorWindow.GetWindowWithRect<SceneUISelectionFilter>(rect, true);
        win.titleContent = new GUIContent("UI Selector");
        win.Show();
    }

    private MethodInfo Internal_PickClosestGO;

    private void OnEnable()
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        System.Type handleUtilityType = editorAssembly.GetType("UnityEditor.HandleUtility");

        FieldInfo pickClosestDelegateInfo = handleUtilityType.GetField("pickClosestGameObjectDelegate", BindingFlags.Static | BindingFlags.NonPublic);
        Delegate pickHandler = Delegate.CreateDelegate(pickClosestDelegateInfo.FieldType, this, "OnPick");
        pickClosestDelegateInfo.SetValue(null, pickHandler);

        Internal_PickClosestGO = handleUtilityType.GetMethod("Internal_PickClosestGO", BindingFlags.Static | BindingFlags.NonPublic);
    }

    private void OnDisable()
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        System.Type handleUtilityType = editorAssembly.GetType("UnityEditor.HandleUtility");

        FieldInfo pickClosestDelegateInfo = handleUtilityType.GetField("pickClosestGameObjectDelegate", BindingFlags.Static | BindingFlags.NonPublic);
        pickClosestDelegateInfo.SetValue(null, null);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("当前优先选中Image、Text等UI组件");
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("点击Scene窗口中的UI会自动过滤掉哪些遮挡在前面看不见的UI，从而可以快速选中其中的Image、Text等组件。", MessageType.Info);
    }

    private GameObject OnPick(Camera cam, int layers, Vector2 position, GameObject[] ignore, GameObject[] filter, out int materialIndex)
    {
        materialIndex = -1;
        filter = GetPickableObject();

        return (GameObject)Internal_PickClosestGO.Invoke(null, new object[] { cam, layers, position, ignore, filter, materialIndex });
    }

    private GameObject[] GetPickableObject()
    {
        List<GameObject> gameObjects = new List<GameObject>();
        for (int i = 0; i < EditorSceneManager.loadedSceneCount; ++i)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            foreach (var root in scene.GetRootGameObjects())
            {
                gameObjects.AddRange(root.GetComponentsInChildren<Graphic>().Select((a) => { return a.gameObject; }));
            }
        }
        return gameObjects.ToArray();
    }
}
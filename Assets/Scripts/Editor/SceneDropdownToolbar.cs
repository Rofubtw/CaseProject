#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using System.Reflection;

[InitializeOnLoad]
public static class SceneDropdownToolbar
{
	static List<string> sceneNames = new List<string>();
	static int selectedSceneIndex = 0;

	// Unity Editor toolbar'unun tipini reflection ile elde ediyoruz.
	static System.Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
	static FieldInfo mRootVisualElementField = toolbarType.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);

	static SceneDropdownToolbar()
	{
		EditorApplication.delayCall += SetupToolbar;

		EditorBuildSettings.sceneListChanged += OnSceneListChanged;

		OnSceneListChanged();
	}

	private static void OnSceneListChanged()
	{
		// EditorBuildSettings'teki aktif sahneleri her seferinde oku
		List<string> currentScenes = EditorBuildSettings.scenes
			.Where(scene => scene.enabled)
			.Select(scene => Path.GetFileNameWithoutExtension(scene.path))
			.ToList();

		// Eðer listede bir deðiþiklik olduysa (örneðin yeni bir sahne eklendiyse), güncelle
		if (!currentScenes.SequenceEqual(sceneNames))
		{
			sceneNames = currentScenes;
			if (selectedSceneIndex >= sceneNames.Count)
			{
				selectedSceneIndex = 0;
			}
		}
	}

	static void SetupToolbar()
	{
		var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
		if (toolbars.Length > 0)
		{
			var toolbar = toolbars[0];
			var root = mRootVisualElementField.GetValue(toolbar) as VisualElement;
			if (root == null)
			{
				Debug.LogError("Toolbar root bulunamadý.");
				return;
			}
			var rightToolbar = root.Q("ToolbarZoneRightAlign");
			if (rightToolbar != null)
			{
				IMGUIContainer container = new IMGUIContainer(OnGUI)
				{
					name = "SceneDropdownContainer"
				};
				rightToolbar.Add(container);
			}
			else
			{
				Debug.LogError("Toolbar sað hizalý alan bulunamadý.");
			}
		}
		else
		{
			Debug.LogError("Toolbar bulunamadý.");
		}
	}

	static void OnGUI()
	{
		

		GUILayout.BeginHorizontal();
		selectedSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, sceneNames.ToArray(), GUILayout.Width(150));
		if (GUILayout.Button("Load", GUILayout.Width(50)))
		{
			if (sceneNames.Count > 0)
			{
				string selectedScene = sceneNames[selectedSceneIndex];
				string scenePath = EditorBuildSettings.scenes.First(s =>
					Path.GetFileNameWithoutExtension(s.path) == selectedScene).path;
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(scenePath);
				}
			}
		}
		GUILayout.EndHorizontal();
	}
}
#endif

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically resolves [Inject] fields on all MonoBehaviours after every scene load.
/// No scene object required — runs via [RuntimeInitializeOnLoadMethod].
/// </summary>
public static class DependencyInjector
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        InjectAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InjectAll();
    }

    private static void InjectAll()
    {
        MonoBehaviour[] allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

        foreach (var behaviour in allBehaviours)
        {
            InjectInto(behaviour);
        }
    }

    /// <summary>
    /// Injects dependencies into a single MonoBehaviour instance.
    /// Call manually for runtime-instantiated objects that use [Inject].
    /// </summary>
    public static void InjectInto(MonoBehaviour target)
    {
        Type type = target.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<InjectAttribute>() == null) continue;
            if (field.GetValue(target) != null) continue;

            Type fieldType = field.FieldType;

            if (!typeof(Component).IsAssignableFrom(fieldType))
            {
                Debug.LogWarning($"[Inject] field '{field.Name}' on '{type.Name}' is not a Component type. Skipping.");
                continue;
            }

            UnityEngine.Object resolved = UnityEngine.Object.FindObjectOfType(fieldType);

            if (resolved != null)
            {
                field.SetValue(target, resolved);
            }
            else
            {
                Debug.LogWarning($"[Inject] Could not resolve '{fieldType.Name}' for '{type.Name}.{field.Name}'.");
            }
        }
    }
}

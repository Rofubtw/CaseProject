using System;

/// <summary>
/// Marks a field for automatic injection by DependencyInjector.
/// Works on fields of MonoBehaviour types — resolved via FindAnyObjectByType at runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class InjectAttribute : Attribute { }

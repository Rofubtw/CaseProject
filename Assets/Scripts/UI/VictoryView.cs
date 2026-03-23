using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Victory screen UI (MVC View). Shows/hides the victory panel and fires restart event.
/// </summary>
public class VictoryView : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button restartButton;

    public event Action OnRestartClicked;

    private void Awake()
    {
        restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}

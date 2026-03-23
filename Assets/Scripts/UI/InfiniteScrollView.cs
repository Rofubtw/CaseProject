using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Infinite looping scroll view that pools and repositions items.
/// Attach to the ScrollRect GameObject. Disables any LayoutGroup on Content at runtime.
/// ScrollRect Movement Type should be Unrestricted for best results.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class InfiniteScrollView : MonoBehaviour
{
    [SerializeField] private RectTransform itemPrefab;
    [SerializeField] private float itemHeight = 200f;
    [SerializeField] private float spacing = 10f;
    // [SerializeField] private float contentHeightSafetyMargin = 2f;

    private ScrollRect scrollRect;
    private RectTransform content;
    private RectTransform viewport;

    private int dataCount;
    private List<RectTransform> items = new List<RectTransform>();
    private int topDataIndex;
    private Action<RectTransform, int> bindCallback;
    private float contentHeightOffset => Mathf.Max(1f, itemHeight - spacing);
    // private float contentHeightOffset => Mathf.Max(1f, itemHeight - spacing + contentHeightSafetyMargin);

    public void Setup<T>(T[] data, Action<RectTransform, int> onBind)
    {
        dataCount = data.Length;
        if (dataCount == 0) return;

        bindCallback = onBind;
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
        viewport = scrollRect.viewport;

        // Disable layout components — we position items manually
        var layoutGroup = content.GetComponent<LayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = false;
        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;

        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;

        ClearItems();
        StartCoroutine(InitNextFrame());
    }

    private IEnumerator InitNextFrame()
    {
        // Wait one frame so viewport rect is calculated
        yield return null;

        float viewHeight = viewport.rect.height;
        float step = itemHeight + spacing;
        int count = Mathf.CeilToInt(viewHeight / step) + 3;

        // Keep content slightly smaller than the viewport so ScrollRect bounds never sit exactly on the edge.
        float contentHeight = Mathf.Max(1f, viewHeight - contentHeightOffset);
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
        content.anchoredPosition = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            RectTransform item = Instantiate(itemPrefab, content);
            item.anchorMin = new Vector2(0, 1);
            item.anchorMax = new Vector2(1, 1);
            item.pivot = new Vector2(0.5f, 1);
            item.sizeDelta = new Vector2(0, itemHeight);
            item.anchoredPosition = new Vector2(0, -(i * step));
            items.Add(item);

            bindCallback(item, i % dataCount);
        }

        topDataIndex = 0;
    }

    private void LateUpdate()
    {
        if (items.Count == 0 || content == null) return;

        float step = itemHeight + spacing;
        float contentY = content.anchoredPosition.y;
        float viewHeight = viewport.rect.height;

        // Scrolling down — top items scrolled out above → recycle to bottom
        while (true)
        {
            RectTransform topItem = items[0];
            float topEdge = topItem.anchoredPosition.y + contentY;
            if (topEdge > step)
            {
                RectTransform botItem = items[items.Count - 1];
                topItem.anchoredPosition = new Vector2(0, botItem.anchoredPosition.y - step);

                items.RemoveAt(0);
                items.Add(topItem);

                topDataIndex = (topDataIndex + 1) % dataCount;
                int dataIdx = (topDataIndex + items.Count - 1) % dataCount;
                bindCallback(topItem, dataIdx);
            }
            else break;
        }

        // Scrolling up — bottom items scrolled out below → recycle to top
        while (true)
        {
            RectTransform botItem = items[items.Count - 1];
            float botTopEdge = botItem.anchoredPosition.y + contentY;
            if (botTopEdge < -(viewHeight + step))
            {
                RectTransform topItem = items[0];
                botItem.anchoredPosition = new Vector2(0, topItem.anchoredPosition.y + step);

                items.RemoveAt(items.Count - 1);
                items.Insert(0, botItem);

                topDataIndex = (topDataIndex - 1 + dataCount) % dataCount;
                bindCallback(botItem, topDataIndex);
            }
            else break;
        }
    }

    private void ClearItems()
    {
        foreach (var item in items)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        items.Clear();
    }
}

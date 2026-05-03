using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighContrastMenu : MonoBehaviour
{
    [Header("Assign UI elements to override")]
    [Tooltip("Images that will be changed to black when ApplyHighContrast is called")]
    [SerializeField] private List<Image> imagesToTurnBlack = new List<Image>();

    [Tooltip("TextMeshPro (or TMP_Text) components that will be changed to white when ApplyHighContrast is called")]
    [SerializeField] private List<TMP_Text> textsToTurnWhite = new List<TMP_Text>();

    [Header("Behavior")]
    [Tooltip("Apply high contrast automatically on Start")]
    [SerializeField] private bool applyOnStart = false;

    // Stores original colors so ResetColors can restore them
    private readonly Dictionary<Image, Color> originalImageColors = new Dictionary<Image, Color>();
    private readonly Dictionary<TMP_Text, Color> originalTextColors = new Dictionary<TMP_Text, Color>();

    void Start()
    {
        CacheOriginalColors();

        if (applyOnStart)
        {
            ApplyHighContrast();
        }
    }

    // Cache original colors for all assigned elements (safe to call multiple times)
    private void CacheOriginalColors()
    {
        originalImageColors.Clear();
        originalTextColors.Clear();

        if (imagesToTurnBlack != null)
        {
            foreach (var img in imagesToTurnBlack)
            {
                if (img != null && !originalImageColors.ContainsKey(img))
                    originalImageColors[img] = img.color;
            }
        }

        if (textsToTurnWhite != null)
        {
            foreach (var txt in textsToTurnWhite)
            {
                if (txt != null && !originalTextColors.ContainsKey(txt))
                    originalTextColors[txt] = txt.color;
            }
        }
    }

    // Sets images to black and TMP texts to white
    [ContextMenu("Apply High Contrast")]
    public void ApplyHighContrast()
    {
        if (imagesToTurnBlack != null)
        {
            foreach (var img in imagesToTurnBlack)
            {
                if (img == null) continue;
                img.color = Color.black;
            }
        }

        if (textsToTurnWhite != null)
        {
            foreach (var txt in textsToTurnWhite)
            {
                if (txt == null) continue;
                txt.color = Color.white;
            }
        }
    }

    // Restores previously cached original colors
    [ContextMenu("Reset Colors")]
    public void ResetColors()
    {
        // If cache is empty (e.g. added in inspector after Start), refresh it
        if (originalImageColors.Count == 0 && (imagesToTurnBlack != null && imagesToTurnBlack.Count > 0))
            CacheOriginalColors();
        if (originalTextColors.Count == 0 && (textsToTurnWhite != null && textsToTurnWhite.Count > 0))
            CacheOriginalColors();

        foreach (var kvp in originalImageColors)
        {
            if (kvp.Key != null)
                kvp.Key.color = kvp.Value;
        }

        foreach (var kvp in originalTextColors)
        {
            if (kvp.Key != null)
                kvp.Key.color = kvp.Value;
        }
    }

    // Helpers to allow runtime modification from other scripts
    public void AddImage(Image image)
    {
        if (image == null) return;
        if (!imagesToTurnBlack.Contains(image))
            imagesToTurnBlack.Add(image);
        if (!originalImageColors.ContainsKey(image))
            originalImageColors[image] = image.color;
    }

    public void AddText(TMP_Text text)
    {
        if (text == null) return;
        if (!textsToTurnWhite.Contains(text))
            textsToTurnWhite.Add(text);
        if (!originalTextColors.ContainsKey(text))
            originalTextColors[text] = text.color;
    }
}
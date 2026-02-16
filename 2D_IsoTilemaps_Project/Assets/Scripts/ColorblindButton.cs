using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorblindButton : MonoBehaviour
{
    public TMP_Text label;
    public string prefix = "Vision: ";
    public ColorblindApplier applier; // Drag your ColorblindApplier GameObject here

    private static readonly string[] ModeNames =
        { "Normal", "Deuteranopia", "Protanopia", "Tritanopia", "Monochromacy" };

    void Start() => Refresh();

    public void Cycle()
    {
        int next = (PlayerPrefs.GetInt("ColorblindMode", 0) + 1) % ModeNames.Length;
        PlayerPrefs.SetInt("ColorblindMode", next);
        PlayerPrefs.Save();
        Refresh();
        if (applier != null) applier.Apply(); // Trigger immediate recolor
    }

    private void Refresh()
    {
        if (label != null)
            label.text = prefix + ModeNames[PlayerPrefs.GetInt("ColorblindMode", 0)];
    }
}

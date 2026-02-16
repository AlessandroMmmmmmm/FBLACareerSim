using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorblindApplier : MonoBehaviour
{
    [Header("All Buttons To Recolor")]
    public Button[] buttons;

    [Header("All Button Text Labels")]
    public TMP_Text[] buttonTexts;

    [Header("Background Quad (with Custom/FutureBackground shader)")]
    public Renderer backgroundQuad;

    [Header("Title Text")]
    public TMP_Text titleText;

    [Header("Instructions Text")]
    public TMP_Text instructionsText;

    // ── Original colors (from Inspector) ────────────────────────────────
    private static readonly Color OriginalButtonBg = new Color(0.941f, 0.471f, 0.314f); // F07850
    private static readonly Color OriginalTitleText = new Color(1.000f, 0.498f, 0.314f); // FF7F50
    private static readonly Color OriginalInstrText = new Color(0.063f, 0.388f, 0.478f); // 10637A
    private static readonly Color OriginalButtonText = new Color(0.196f, 0.196f, 0.196f); // 323232

    // ── Button background ────────────────────────────────────────────────
    private static Color BtnColor(int m) => m switch
    {
        1 or 2 => new Color(0.20f, 0.45f, 0.80f), // blue
        3 => new Color(0.50f, 0.15f, 0.60f), // purple (tritan safe)
        4 => new Color(0.40f, 0.40f, 0.40f), // gray
        _ => OriginalButtonBg,
    };

    // ── Button text ──────────────────────────────────────────────────────
    private static Color BtnTextColor(int m) => m switch
    {
        1 or 2 => new Color(1.00f, 0.95f, 0.80f), // warm white on blue
        3 => new Color(0.90f, 0.90f, 0.20f), // yellow on purple
        4 => new Color(1.00f, 1.00f, 1.00f), // white on gray
        _ => OriginalButtonText,
    };

    // ── Title text ───────────────────────────────────────────────────────
    private static Color TitleColor(int m) => m switch
    {
        1 or 2 => new Color(1.00f, 0.75f, 0.20f), // amber/gold
        3 => new Color(0.90f, 0.90f, 0.20f), // yellow
        4 => new Color(1.00f, 1.00f, 1.00f), // white
        _ => OriginalTitleText,
    };

    // ── Instructions text ────────────────────────────────────────────────
    private static Color InstrColor(int m) => m switch
    {
        1 or 2 => new Color(0.20f, 0.20f, 0.60f), // dark blue
        3 => new Color(0.40f, 0.10f, 0.50f), // dark purple
        4 => new Color(0.20f, 0.20f, 0.20f), // dark gray
        _ => OriginalInstrText,
    };

    // ── Shader property IDs ───────────────────────────────────────────────
    private static readonly int PropTopColor = Shader.PropertyToID("_TopColor");
    private static readonly int PropMidColor = Shader.PropertyToID("_MidColor");
    private static readonly int PropHorizonColor = Shader.PropertyToID("_HorizonColor");
    private static readonly int PropGlowColor = Shader.PropertyToID("_GlowColor");

    // Original shader defaults
    private static readonly Color OrigTopColor = new Color(0.30f, 0.65f, 1.00f);
    private static readonly Color OrigMidColor = new Color(0.75f, 0.92f, 1.00f);
    private static readonly Color OrigHorizonColor = new Color(1.00f, 0.96f, 0.80f);
    private static readonly Color OrigGlowColor = new Color(1.00f, 0.88f, 0.45f);

    private static Color BgTop(int m) => m switch
    {
        1 or 2 => new Color(0.05f, 0.10f, 0.50f), // deep navy
        3 => new Color(0.35f, 0.05f, 0.45f), // deep purple
        4 => new Color(0.08f, 0.08f, 0.08f), // near black
        _ => OrigTopColor,
    };

    private static Color BgMid(int m) => m switch
    {
        1 or 2 => new Color(0.20f, 0.45f, 0.85f), // medium blue
        3 => new Color(0.55f, 0.10f, 0.60f), // mid purple
        4 => new Color(0.35f, 0.35f, 0.35f), // dark gray
        _ => OrigMidColor,
    };

    private static Color BgHorizon(int m) => m switch
    {
        1 or 2 => new Color(0.40f, 0.75f, 1.00f), // light blue horizon
        3 => new Color(0.85f, 0.55f, 0.10f), // amber horizon
        4 => new Color(0.65f, 0.65f, 0.65f), // light gray
        _ => OrigHorizonColor,
    };

    private static Color BgGlow(int m) => m switch
    {
        1 or 2 => new Color(0.20f, 0.60f, 1.00f), // blue glow
        3 => new Color(0.95f, 0.70f, 0.10f), // orange glow (tritan safe)
        4 => new Color(0.85f, 0.85f, 0.85f), // white glow
        _ => OrigGlowColor,
    };

    // ─────────────────────────────────────────────────────────────────────
    void Start() => Apply();

    public void Apply()
    {
        int mode = PlayerPrefs.GetInt("ColorblindMode", 0);

        // Buttons
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            var img = btn.GetComponent<Image>();
            //if (img != null) img.color = BtnColor(mode);

            var colors = btn.colors;
            colors.normalColor = BtnColor(mode);
            colors.highlightedColor = BtnColor(mode) * 1.2f;
            colors.pressedColor = BtnColor(mode) * 0.8f;
            colors.selectedColor = BtnColor(mode);
            btn.colors = colors;
        }

        // Button texts
        foreach (var t in buttonTexts)
            if (t != null) t.color = BtnTextColor(mode);

        // Title
        if (titleText != null) titleText.color = TitleColor(mode);

        // Instructions
        if (instructionsText != null) instructionsText.color = InstrColor(mode);

        // Background quad shader
        if (backgroundQuad != null)
        {
            Material mat = backgroundQuad.material;
            mat.SetColor(PropTopColor, BgTop(mode));
            mat.SetColor(PropMidColor, BgMid(mode));
            mat.SetColor(PropHorizonColor, BgHorizon(mode));
            mat.SetColor(PropGlowColor, BgGlow(mode));
        }
    }
}

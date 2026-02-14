using UnityEngine;

/// <summary>
/// "Paths of the Future" — Title screen background for a career simulator.
/// Spawns drifting light trails and glowing orbs that loop seamlessly.
/// Attach to an empty GameObject in your title scene.
/// </summary>
public class TitleScreenBackground : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera targetCamera;

    // ── Trails ───────────────────────────────────────────────────────────
    [Header("Light Trails")]
    public int trailCount = 18;
    public float trailMinSpeed = 0.5f;
    public float trailMaxSpeed = 1.2f;
    public float trailMinWidth = 0.008f;
    public float trailMaxWidth = 0.025f;
    public float trailMinLength = 1.5f;
    public float trailMaxLength = 4.5f;
    public float trailMinAlpha = 0.08f;
    public float trailMaxAlpha = 0.35f;
    public Color trailColorA = new Color(1.00f, 0.92f, 0.40f);  // bright sunshine
    public Color trailColorB = new Color(0.40f, 0.95f, 0.80f);

    // ── Orbs ─────────────────────────────────────────────────────────────
    [Header("Glowing Orbs")]
    public int orbCount = 28;
    public float orbMinSpeed = 0.08f;
    public float orbMaxSpeed = 0.28f;
    public float orbMinSize = 0.06f;
    public float orbMaxSize = 0.38f;
    public float orbMinAlpha = 0.04f;
    public float orbMaxAlpha = 0.22f;

    // ── Internal ─────────────────────────────────────────────────────────
    private Bounds _camBounds;

    private struct Trail
    {
        public LineRenderer lr;
        public float speed, length, angle;
        public Vector3 pos;
        public float alpha, maxAlpha;
    }

    private struct Orb
    {
        public SpriteRenderer sr;
        public float speed;
        public Vector3 dir;
        public float alpha, maxAlpha, fadePhase;
    }

    private Trail[] _trails;
    private Orb[] _orbs;
    private Material _trailMat;
    private Sprite _orbSprite;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        RefreshBounds();
        _trailMat = CreateTrailMaterial();
        _orbSprite = CreateSoftCircleSprite(64);
    }

    void Start()
    {
        SpawnTrails();
        SpawnOrbs();
    }

    void Update()
    {
        RefreshBounds();
        UpdateTrails();
        UpdateOrbs();
    }

    // ── Bounds ────────────────────────────────────────────────────────────
    void RefreshBounds()
    {
        float h = targetCamera.orthographicSize;
        float w = h * targetCamera.aspect;
        Vector3 c = targetCamera.transform.position;
        _camBounds = new Bounds(new Vector3(c.x, c.y, 0), new Vector3(w * 2, h * 2, 1));
    }

    // ── Trails ────────────────────────────────────────────────────────────
    void SpawnTrails()
    {
        _trails = new Trail[trailCount];
        for (int i = 0; i < trailCount; i++)
            _trails[i] = CreateTrail(randomY: true);
    }

    Trail CreateTrail(bool randomY)
    {
        var go = new GameObject("Trail");
        go.transform.SetParent(transform);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = _trailMat;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sortingOrder = -5;

        float speed = Random.Range(trailMinSpeed, trailMaxSpeed);
        float width = Random.Range(trailMinWidth, trailMaxWidth);
        float length = Random.Range(trailMinLength, trailMaxLength);
        float maxAlph = Random.Range(trailMinAlpha, trailMaxAlpha);
        float angle = Random.Range(-18f, 18f); // gentle diagonal drift

        // Colour: interpolate between two palettes
        Color c = Color.Lerp(trailColorA, trailColorB, Random.value);
        c.a = 0f;
        lr.startColor = lr.endColor = c;
        lr.startWidth = lr.endWidth = width;

        // Spawn anywhere across the width, starting below screen
        float x = Random.Range(_camBounds.min.x, _camBounds.max.x);
        float y = randomY
                   ? Random.Range(_camBounds.min.y, _camBounds.max.y)
                   : _camBounds.min.y - length;
        var pos = new Vector3(x, y, 0f);

        return new Trail
        {
            lr = lr,
            speed = speed,
            length = length,
            angle = angle,
            pos = pos,
            alpha = 0f,
            maxAlpha = maxAlph
        };
    }

    void UpdateTrails()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < _trails.Length; i++)
        {
            ref Trail t = ref _trails[i];

            // Move upward with slight angle
            float rad = t.angle * Mathf.Deg2Rad;
            t.pos += new Vector3(Mathf.Sin(rad) * t.speed * dt,
                                 Mathf.Cos(rad) * t.speed * dt, 0f);

            // Tail and head positions
            float halfLen = t.length * 0.5f;
            Vector3 head = t.pos + new Vector3(0, halfLen, 0);
            Vector3 tail = t.pos - new Vector3(0, halfLen, 0);
            t.lr.SetPosition(0, tail);
            t.lr.SetPosition(1, head);

            // Fade in/out based on vertical position in camera
            float norm = Mathf.InverseLerp(_camBounds.min.y, _camBounds.max.y, t.pos.y);
            float fade = Mathf.Sin(norm * Mathf.PI); // peaks at mid-screen
            t.alpha = fade * t.maxAlpha;

            Color c = t.lr.startColor;
            c.a = t.alpha;
            t.lr.startColor = t.lr.endColor = c;

            // Reset when gone past top
            if (t.pos.y > _camBounds.max.y + t.length)
            {
                float x = Random.Range(_camBounds.min.x, _camBounds.max.x);
                t.pos = new Vector3(x, _camBounds.min.y - t.length, 0f);
                t.angle = Random.Range(-18f, 18f);
                t.speed = Random.Range(trailMinSpeed, trailMaxSpeed);
            }
        }
    }

    // ── Orbs ──────────────────────────────────────────────────────────────
    void SpawnOrbs()
    {
        _orbs = new Orb[orbCount];
        for (int i = 0; i < orbCount; i++)
            _orbs[i] = CreateOrb(randomPos: true);
    }

    Orb CreateOrb(bool randomPos)
    {
        var go = new GameObject("Orb");
        go.transform.SetParent(transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _orbSprite;
        sr.sortingOrder = -6;

        float size = Random.Range(orbMinSize, orbMaxSize);
        go.transform.localScale = Vector3.one * size;

        // Pale warm or cool tint
        Color tint = Random.value > 0.5f
            ? new Color(1.00f, 0.96f, 0.55f)  // lemon yellow
            : new Color(0.55f, 1.00f, 0.85f);
        tint.a = 0f;
        sr.color = tint;

        float maxAlph = Random.Range(orbMinAlpha, orbMaxAlpha);
        float speed = Random.Range(orbMinSpeed, orbMaxSpeed);
        Vector2 dir = new Vector2(Random.Range(-0.3f, 0.3f), 1f).normalized;

        Vector3 pos = randomPos
            ? new Vector3(Random.Range(_camBounds.min.x, _camBounds.max.x),
                          Random.Range(_camBounds.min.y, _camBounds.max.y), 0.1f)
            : new Vector3(Random.Range(_camBounds.min.x, _camBounds.max.x),
                          _camBounds.min.y - size, 0.1f);

        go.transform.position = pos;

        return new Orb
        {
            sr = sr,
            speed = speed,
            dir = dir,
            alpha = 0f,
            maxAlpha = maxAlph,
            fadePhase = Random.Range(0f, Mathf.PI * 2f)
        };
    }

    void UpdateOrbs()
    {
        float dt = Time.deltaTime;
        float time = Time.time;

        for (int i = 0; i < _orbs.Length; i++)
        {
            ref Orb o = ref _orbs[i];

            o.sr.transform.position += o.dir * (o.speed * dt);

            // Gentle sine-wave alpha pulse
            float norm = Mathf.InverseLerp(_camBounds.min.y, _camBounds.max.y,
                                             o.sr.transform.position.y);
            float pulse = (Mathf.Sin(time * 0.7f + o.fadePhase) * 0.5f + 0.5f);
            float fade = Mathf.Sin(norm * Mathf.PI);
            o.alpha = fade * pulse * o.maxAlpha;

            Color c = o.sr.color;
            c.a = o.alpha;
            o.sr.color = c;

            // Recycle off-screen orbs
            Vector3 p = o.sr.transform.position;
            if (p.y > _camBounds.max.y + 0.5f ||
                p.x < _camBounds.min.x - 0.5f ||
                p.x > _camBounds.max.x + 0.5f)
            {
                float x = Random.Range(_camBounds.min.x, _camBounds.max.x);
                o.sr.transform.position = new Vector3(x, _camBounds.min.y - 0.3f, 0.1f);
                o.dir = new Vector2(Random.Range(-0.3f, 0.3f), 1f).normalized;
                o.speed = Random.Range(orbMinSpeed, orbMaxSpeed);
                o.fadePhase = Random.Range(0f, Mathf.PI * 2f);
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    Material CreateTrailMaterial()
    {
        // Use the Sprites/Default shader — works without URP or HDRP
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // additive glow
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        return mat;
    }

    /// <summary>Generates a soft radial gradient circle sprite at runtime.</summary>
    Sprite CreateSoftCircleSprite(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float half = res * 0.5f;

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - dist);
                a = a * a; // squared falloff = softer glow
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();

        return Sprite.Create(tex,
            new Rect(0, 0, res, res),
            new Vector2(0.5f, 0.5f),
            res);
    }
}
using UnityEngine;

public class TitleScreenBackground : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera targetCamera;

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
    public Color trailColorA = new Color(1.00f, 0.92f, 0.40f);
    public Color trailColorB = new Color(0.40f, 0.95f, 0.80f);

    [Header("Glowing Orbs")]
    public int orbCount = 28;
    public float orbMinSpeed = 0.08f;
    public float orbMaxSpeed = 0.28f;
    public float orbMinSize = 0.06f;
    public float orbMaxSize = 0.38f;
    public float orbMinAlpha = 0.04f;
    public float orbMaxAlpha = 0.22f;

    private Bounds _camBounds;

    private struct Trail
    {
        public LineRenderer lr;
        public float speed, length, angle;
        public Vector3 pos;
        public float alpha, maxAlpha;
        public Color baseColor;
    }

    private struct Orb
    {
        public SpriteRenderer sr;
        public float speed;
        public Vector3 dir;
        public float alpha, maxAlpha, fadePhase;
        public Color baseColor;
    }

    private Trail[] _trails;
    private Orb[] _orbs;
    private Material _trailMat;
    private Sprite _orbSprite;

    private int _lastMode = -1;

    // ── Colorblind palettes ───────────────────────────────────────────────

    private static Color GetTrailA(int mode) => mode switch
    {
        1 or 2 => new Color(0.30f, 0.65f, 1.00f), // blue  (no red/green reliance)
        3 => new Color(1.00f, 0.40f, 0.40f), // red   (fine for tritanopia)
        4 => new Color(0.90f, 0.90f, 0.90f), // light gray
        _ => new Color(1.00f, 0.92f, 0.40f), // default yellow
    };

    private static Color GetTrailB(int mode) => mode switch
    {
        1 or 2 => new Color(1.00f, 0.60f, 0.10f), // orange
        3 => new Color(0.20f, 0.85f, 0.85f), // cyan
        4 => new Color(0.45f, 0.45f, 0.45f), // dark gray
        _ => new Color(0.40f, 0.95f, 0.80f), // default teal
    };

    private static Color GetOrbA(int mode) => mode switch
    {
        1 or 2 => new Color(0.30f, 0.65f, 1.00f), // blue
        3 => new Color(1.00f, 0.50f, 0.50f), // pink-red
        4 => new Color(1.00f, 1.00f, 1.00f), // white
        _ => new Color(1.00f, 0.96f, 0.55f), // default lemon
    };

    private static Color GetOrbB(int mode) => mode switch
    {
        1 or 2 => new Color(1.00f, 0.60f, 0.10f), // orange
        3 => new Color(0.20f, 0.85f, 0.85f), // cyan
        4 => new Color(0.60f, 0.60f, 0.60f), // mid gray
        _ => new Color(0.55f, 1.00f, 0.85f), // default mint
    };

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
        _lastMode = PlayerPrefs.GetInt("ColorblindMode", 0);
    }

    void Update()
    {
        RefreshBounds();

        // Detect mode change and recolor all elements
        int mode = PlayerPrefs.GetInt("ColorblindMode", 0);
        if (mode != _lastMode)
        {
            _lastMode = mode;
            RecolorAll(mode);
        }

        UpdateTrails();
        UpdateOrbs();
    }

    // ── Recolor existing elements when mode changes ───────────────────────

    private void RecolorAll(int mode)
    {
        Color a = GetTrailA(mode);
        Color b = GetTrailB(mode);

        for (int i = 0; i < _trails.Length; i++)
        {
            Color c = Color.Lerp(a, b, Random.value);
            c.a = _trails[i].lr.startColor.a;
            _trails[i].lr.startColor = _trails[i].lr.endColor = c;
            _trails[i].baseColor = c;
        }

        Color oa = GetOrbA(mode);
        Color ob = GetOrbB(mode);

        for (int i = 0; i < _orbs.Length; i++)
        {
            Color c = Random.value > 0.5f ? oa : ob;
            c.a = _orbs[i].sr.color.a;
            _orbs[i].sr.color = c;
            _orbs[i].baseColor = c;
        }
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
        int mode = PlayerPrefs.GetInt("ColorblindMode", 0);
        _trails = new Trail[trailCount];
        for (int i = 0; i < trailCount; i++)
            _trails[i] = CreateTrail(randomY: true, mode);
    }

    Trail CreateTrail(bool randomY, int mode)
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
        float maxA = Random.Range(trailMinAlpha, trailMaxAlpha);
        float angle = Random.Range(-18f, 18f);

        Color c = Color.Lerp(GetTrailA(mode), GetTrailB(mode), Random.value);
        c.a = 0f;
        lr.startColor = lr.endColor = c;
        lr.startWidth = lr.endWidth = width;

        float x = Random.Range(_camBounds.min.x, _camBounds.max.x);
        float y = randomY ? Random.Range(_camBounds.min.y, _camBounds.max.y)
                          : _camBounds.min.y - length;

        return new Trail
        {
            lr = lr,
            speed = speed,
            length = length,
            angle = angle,
            pos = new Vector3(x, y, 0f),
            alpha = 0f,
            maxAlpha = maxA,
            baseColor = c
        };
    }

    void UpdateTrails()
    {
        float dt = Time.deltaTime;
        int mode = _lastMode;

        for (int i = 0; i < _trails.Length; i++)
        {
            ref Trail t = ref _trails[i];

            float rad = t.angle * Mathf.Deg2Rad;
            t.pos += new Vector3(Mathf.Sin(rad) * t.speed * dt,
                                 Mathf.Cos(rad) * t.speed * dt, 0f);

            float halfLen = t.length * 0.5f;
            t.lr.SetPosition(0, t.pos - new Vector3(0, halfLen, 0));
            t.lr.SetPosition(1, t.pos + new Vector3(0, halfLen, 0));

            float norm = Mathf.InverseLerp(_camBounds.min.y, _camBounds.max.y, t.pos.y);
            float fade = Mathf.Sin(norm * Mathf.PI);
            t.alpha = fade * t.maxAlpha;

            Color c = t.lr.startColor;
            c.a = t.alpha;
            t.lr.startColor = t.lr.endColor = c;

            if (t.pos.y > _camBounds.max.y + t.length)
            {
                float x = Random.Range(_camBounds.min.x, _camBounds.max.x);
                t.pos = new Vector3(x, _camBounds.min.y - t.length, 0f);
                t.angle = Random.Range(-18f, 18f);
                t.speed = Random.Range(trailMinSpeed, trailMaxSpeed);

                // Pick fresh color from current palette
                Color nc = Color.Lerp(GetTrailA(mode), GetTrailB(mode), Random.value);
                nc.a = 0f;
                t.lr.startColor = t.lr.endColor = nc;
                t.baseColor = nc;
            }
        }
    }

    // ── Orbs ──────────────────────────────────────────────────────────────
    void SpawnOrbs()
    {
        int mode = PlayerPrefs.GetInt("ColorblindMode", 0);
        _orbs = new Orb[orbCount];
        for (int i = 0; i < orbCount; i++)
            _orbs[i] = CreateOrb(randomPos: true, mode);
    }

    Orb CreateOrb(bool randomPos, int mode)
    {
        var go = new GameObject("Orb");
        go.transform.SetParent(transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _orbSprite;
        sr.sortingOrder = -6;

        float size = Random.Range(orbMinSize, orbMaxSize);
        go.transform.localScale = Vector3.one * size;

        Color tint = Random.value > 0.5f ? GetOrbA(mode) : GetOrbB(mode);
        tint.a = 0f;
        sr.color = tint;

        float maxA = Random.Range(orbMinAlpha, orbMaxAlpha);
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
            maxAlpha = maxA,
            fadePhase = Random.Range(0f, Mathf.PI * 2f),
            baseColor = tint
        };
    }

    void UpdateOrbs()
    {
        float dt = Time.deltaTime;
        float time = Time.time;
        int mode = _lastMode;

        for (int i = 0; i < _orbs.Length; i++)
        {
            ref Orb o = ref _orbs[i];

            o.sr.transform.position += o.dir * (o.speed * dt);

            float norm = Mathf.InverseLerp(_camBounds.min.y, _camBounds.max.y,
                                             o.sr.transform.position.y);
            float pulse = (Mathf.Sin(time * 0.7f + o.fadePhase) * 0.5f + 0.5f);
            o.alpha = Mathf.Sin(norm * Mathf.PI) * pulse * o.maxAlpha;

            Color c = o.sr.color;
            c.a = o.alpha;
            o.sr.color = c;

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

                Color nc = Random.value > 0.5f ? GetOrbA(mode) : GetOrbB(mode);
                nc.a = 0f;
                o.sr.color = nc;
                o.baseColor = nc;
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    Material CreateTrailMaterial()
    {
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        return mat;
    }

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
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}

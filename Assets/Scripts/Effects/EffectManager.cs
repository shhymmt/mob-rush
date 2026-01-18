using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all visual effects in the game.
/// ゲーム内の全てのビジュアルエフェクトを管理。
/// </summary>
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private int explosionPoolSize = 10;
    [SerializeField] private int hitSparkPoolSize = 20;
    [SerializeField] private int flashPoolSize = 5;

    [Header("Effect Settings")]
    [SerializeField] private int explosionParticleCount = 12;
    [SerializeField] private int hitSparkParticleCount = 5;

    // Particle pools
    private List<ParticleSystem> explosionPool = new List<ParticleSystem>();
    private List<ParticleSystem> hitSparkPool = new List<ParticleSystem>();
    private List<ParticleSystem> flashPool = new List<ParticleSystem>();

    // Number popup pool
    private List<NumberPopup> numberPopupPool = new List<NumberPopup>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        // Create explosion pool
        for (int i = 0; i < explosionPoolSize; i++)
        {
            explosionPool.Add(CreateExplosionParticle());
        }

        // Create hit spark pool
        for (int i = 0; i < hitSparkPoolSize; i++)
        {
            hitSparkPool.Add(CreateHitSparkParticle());
        }

        // Create flash pool
        for (int i = 0; i < flashPoolSize; i++)
        {
            flashPool.Add(CreateFlashParticle());
        }

        // Create number popup pool
        for (int i = 0; i < 10; i++)
        {
            numberPopupPool.Add(CreateNumberPopup());
        }

        Debug.Log("EffectManager: Pools initialized");
    }

    #region Explosion Effect

    /// <summary>
    /// 爆発エフェクトを再生
    /// </summary>
    public void PlayExplosion(Vector3 position, Color color)
    {
        ParticleSystem ps = GetFromPool(explosionPool);
        if (ps == null) return;

        ps.transform.position = position;

        // Set color
        var main = ps.main;
        main.startColor = color;

        ps.gameObject.SetActive(true);
        ps.Play();
    }

    /// <summary>
    /// 敵撃破用の爆発（赤/オレンジ）
    /// </summary>
    public void PlayEnemyExplosion(Vector3 position)
    {
        PlayExplosion(position, new Color(1f, 0.4f, 0.2f)); // Orange-red
    }

    private ParticleSystem CreateExplosionParticle()
    {
        GameObject obj = new GameObject("ExplosionEffect");
        obj.transform.SetParent(transform);
        obj.SetActive(false);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.4f, 0.2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.Disable;

        // Emission
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, explosionParticleCount)
        });

        // Shape - Circle
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        // Size over lifetime
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

        // Color over lifetime (fade out)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        // Renderer
        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingOrder = 10;

        return ps;
    }

    #endregion

    #region Hit Spark Effect

    /// <summary>
    /// ヒットスパークエフェクトを再生
    /// </summary>
    public void PlayHitSpark(Vector3 position)
    {
        ParticleSystem ps = GetFromPool(hitSparkPool);
        if (ps == null) return;

        ps.transform.position = position;
        ps.gameObject.SetActive(true);
        ps.Play();
    }

    private ParticleSystem CreateHitSparkParticle()
    {
        GameObject obj = new GameObject("HitSparkEffect");
        obj.transform.SetParent(transform);
        obj.SetActive(false);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.duration = 0.2f;
        main.loop = false;
        main.startLifetime = 0.15f;
        main.startSpeed = 5f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 1f, 0.5f); // Yellow-white
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.Disable;

        // Emission
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, hitSparkParticleCount)
        });

        // Shape - Cone (upward)
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.05f;
        shape.rotation = new Vector3(-90f, 0f, 0f); // Point upward

        // Size over lifetime
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

        // Renderer
        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingOrder = 11;

        return ps;
    }

    #endregion

    #region Flash Effect

    /// <summary>
    /// フラッシュエフェクトを再生
    /// </summary>
    public void PlayFlash(Vector3 position, Color color)
    {
        ParticleSystem ps = GetFromPool(flashPool);
        if (ps == null) return;

        ps.transform.position = position;

        var main = ps.main;
        main.startColor = color;

        ps.gameObject.SetActive(true);
        ps.Play();
    }

    /// <summary>
    /// ゲートタイプに応じたフラッシュ
    /// </summary>
    public void PlayGateFlash(Vector3 position, LaneGate.GateType gateType)
    {
        Color color;
        switch (gateType)
        {
            case LaneGate.GateType.Add:
                color = new Color(0.2f, 1f, 0.2f); // Green
                break;
            case LaneGate.GateType.Subtract:
                color = new Color(1f, 0.2f, 0.2f); // Red
                break;
            case LaneGate.GateType.Multiply:
                color = new Color(1f, 0.8f, 0.2f); // Orange/Gold
                break;
            default:
                color = Color.white;
                break;
        }
        PlayFlash(position, color);
    }

    private ParticleSystem CreateFlashParticle()
    {
        GameObject obj = new GameObject("FlashEffect");
        obj.transform.SetParent(transform);
        obj.SetActive(false);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        // Main module
        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = 0.2f;
        main.startSpeed = 0f;
        main.startSize = 2f;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.Disable;

        // Emission
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 1)
        });

        // Shape - disabled (single point)
        var shape = ps.shape;
        shape.enabled = false;

        // Size over lifetime (expand then shrink)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime (fade out)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        // Renderer
        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();
        renderer.sortingOrder = 9;

        return ps;
    }

    #endregion

    #region Number Popup

    /// <summary>
    /// 数字ポップアップを表示
    /// </summary>
    public void ShowNumberPopup(Vector3 position, string text, Color color)
    {
        NumberPopup popup = GetNumberPopupFromPool();
        if (popup == null) return;

        popup.Show(position, text, color);
    }

    private NumberPopup GetNumberPopupFromPool()
    {
        foreach (var popup in numberPopupPool)
        {
            if (!popup.gameObject.activeInHierarchy)
            {
                return popup;
            }
        }

        // Create new if pool exhausted
        NumberPopup newPopup = CreateNumberPopup();
        numberPopupPool.Add(newPopup);
        return newPopup;
    }

    private NumberPopup CreateNumberPopup()
    {
        GameObject obj = new GameObject("NumberPopup");
        obj.transform.SetParent(transform);
        obj.SetActive(false);

        NumberPopup popup = obj.AddComponent<NumberPopup>();
        return popup;
    }

    #endregion

    #region Utility

    private ParticleSystem GetFromPool(List<ParticleSystem> pool)
    {
        foreach (var ps in pool)
        {
            if (!ps.gameObject.activeInHierarchy)
            {
                return ps;
            }
        }

        // Pool exhausted - could create new one, but for now return null
        Debug.LogWarning("EffectManager: Pool exhausted");
        return null;
    }

    private Material CreateParticleMaterial()
    {
        // Create a simple additive material
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetFloat("_Mode", 0); // Additive
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);

        // Create white circle texture
        Texture2D tex = CreateCircleTexture(32);
        mat.mainTexture = tex;

        return mat;
    }

    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < radius - 1)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (distance < radius)
                {
                    float alpha = radius - distance;
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    #endregion
}

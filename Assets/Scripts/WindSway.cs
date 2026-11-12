using UnityEngine;

/// <summary>
/// Wind Sway - Lam cay lac lu theo gio bang cach xoay Transform.
/// Khong can shader - hoat dong voi bat ky Render Pipeline nao.
/// Gang vao tung crop GameObject (WhiteFlower, PurpleFlower, Herb, v.v.)
/// </summary>
public class WindSway : MonoBehaviour
{
    [Header("=== WIND SETTINGS ===")]
    [Tooltip("Goc xoay toi da (do)")]
    [Range(0f, 30f)] public float swayAngle    = 8f;

    [Tooltip("Toc do lac")]
    [Range(0f, 5f)]  public float swaySpeed    = 2f;

    [Tooltip("Them gust - gio giat bat thuong")]
    [Range(0f, 1f)]  public float gustStrength = 0.4f;
    [Range(0f, 3f)]  public float gustSpeed    = 0.7f;

    [Header("=== ROOT PIVOT ===")]
    [Tooltip("Dich chuyen pivot xuong duoi de xoay tu goc cay (don vi world)")]
    public float pivotOffsetY = -0.5f;

    [Header("=== RANDOMIZATION ===")]
    [Tooltip("Moi cay lac o pha khac nhau - trong tu nhien hon")]
    public bool  randomizePhase  = true;
    [Tooltip("Dao dong toc do ngau nhien (%)")]
    [Range(0f, 0.5f)] public float speedVariance = 0.2f;

    // --- internal ---
    private float phaseOffset;
    private float speedMultiplier;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Awake()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        if (randomizePhase)
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);

        speedMultiplier = 1f + Random.Range(-speedVariance, speedVariance);
    }

    void Update()
    {
        float t = Time.time * swaySpeed * speedMultiplier + phaseOffset;

        // Song chinh
        float wave1 = Mathf.Sin(t);

        // Song gust (gio giat, chieu nguoc lai doi khi)
        float wave2 = Mathf.Sin(t * gustSpeed * 0.73f) * gustStrength;

        // Song micro (rung nhe)
        float wave3 = Mathf.Sin(t * 2.3f) * 0.1f;

        float totalAngle = (wave1 + wave2 + wave3) * swayAngle;

        // Xoay quanh "goc ao" o phia duoi sprite
        Vector3 pivot = originalPosition + Vector3.up * pivotOffsetY;
        Vector3 offset = originalPosition - pivot;

        float rad = totalAngle * Mathf.Deg2Rad;
        float cosA = Mathf.Cos(rad);
        float sinA = Mathf.Sin(rad);

        Vector3 rotated = new Vector3(
            offset.x * cosA - offset.y * sinA,
            offset.x * sinA + offset.y * cosA,
            offset.z
        );

        transform.localPosition = pivot + rotated;
        transform.localRotation = originalRotation * Quaternion.Euler(0, 0, totalAngle);
    }

    void OnDisable()
    {
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }
}

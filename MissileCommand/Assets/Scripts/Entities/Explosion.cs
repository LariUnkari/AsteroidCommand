using UnityEngine;

public class Explosion : Entity
{
    public GameObject m_effectPrefab;
    public SoundEffectPreset m_sfxPreset;

    public float m_duration = 2f;
    public float m_maxRadius = 0.5f;
    public AnimationCurve m_radiusModifierCurve;

    public SphereCollider m_collider;

    public bool m_drawRadius = false;

    private float m_lifeTime;
    private float m_radius;

    private Transform m_effectTransform;

    private LineRenderer m_radiusRenderer;
    private Vector3[] m_radiusRendererPositions;

    private void Awake()
    {
        m_lifeTime = -1f;

        m_radius = m_maxRadius * m_radiusModifierCurve.Evaluate(0f);

        if (m_collider != null)
            m_collider.radius = m_radius;

        if (m_effectPrefab != null)
        {
            GameObject go = Instantiate<GameObject>(m_effectPrefab, transform.position, transform.rotation, transform);
            m_effectTransform = go.transform;
            m_effectTransform.localScale = Vector3.one * m_radius;
        }

        if (m_sfxPreset != null)
            m_sfxPreset.PlayAt(transform.position, Environment.AudioRoot);

        if (m_drawRadius)
            CreateRadiusRenderer();
    }

    private void Update()
    {
        if (m_lifeTime < 0f)
            m_lifeTime = 0f;
        else
            m_lifeTime += Time.deltaTime;
        
        m_radius = m_maxRadius * m_radiusModifierCurve.Evaluate(Mathf.Clamp01(m_lifeTime / m_duration));

        if (m_collider != null)
            m_collider.radius = m_radius;

        if (m_effectTransform != null)
            m_effectTransform.localScale = Vector3.one * m_radius;

        if (m_drawRadius)
            UpdateRadiusRenderer();

        if (m_lifeTime >= m_duration)
        {
            //Debug.Log(DebugUtilities.AddTimestampPrefix("Explosion Effect life expired after " + m_lifeTime + " seconds"));

            // TODO: Cache the effect by ID instead of destroying it
            OnDeath(false);
        }
    }

    private void CreateRadiusRenderer()
    {
        GameObject rrGO = new GameObject("RadiusRenderer");
        rrGO.transform.position = transform.position;
        rrGO.transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
        rrGO.transform.parent = transform;

        m_radiusRendererPositions = new Vector3[32];
        m_radiusRenderer = rrGO.AddComponent<LineRenderer>();
        m_radiusRenderer.useWorldSpace = false;
        m_radiusRenderer.startWidth = 0.1f;
        m_radiusRenderer.endWidth = 0.1f;
        m_radiusRenderer.numPositions = m_radiusRendererPositions.Length + 1;
    }

    private void UpdateRadiusRenderer()
    {
        if (m_radiusRenderer == null)
            CreateRadiusRenderer();

        m_radiusRendererPositions = Math3D.GetCircleVertices(Vector3.zero, Vector3.forward, Vector3.up, m_radius - m_radiusRenderer.startWidth, m_radiusRendererPositions.Length);
        for (int i = 0; i < m_radiusRendererPositions.Length; i++) m_radiusRenderer.SetPosition(i, m_radiusRendererPositions[i]);
        m_radiusRenderer.SetPosition(m_radiusRenderer.numPositions - 1, m_radiusRendererPositions[0]);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // Do nothing
    }
}

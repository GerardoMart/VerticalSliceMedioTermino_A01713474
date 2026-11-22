using UnityEngine;

// Un carril de ataque. Vive en un prefab que el DangerZoneField instancia.
//  Hidden  -> invisible
//  Warning -> amarillo parpadeante (es el aviso, no hace daño)
//  Active  -> rojo (aquí sí hace daño)
public class DangerZone : MonoBehaviour
{
    public enum State { Hidden, Warning, Active }

    [SerializeField] private Renderer visual;
    [Tooltip("Collider usado solo para medir la zona (Is Trigger, no frena a nadie).")]
    [SerializeField] private BoxCollider zoneCollider;
    [SerializeField] private ParticleSystem activateBurst;

    [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.1f, 0.32f);
    [SerializeField] private Color activeColor  = new Color(1f, 0.08f, 0.05f, 0.62f);
    [SerializeField] private float pulseSpeed = 9f;

    private Material material;
    private float stateTime;

    public State Current { get; private set; } = State.Hidden;
    public Bounds Bounds => zoneCollider.bounds;

    private void Awake()
    {
        if (visual != null) material = visual.material; // instancia propia por carril
    }

    /// <summary>Ajusta tamaño de la parte visual, del collider y de las partículas.</summary>
    public void Configure(float width, float height)
    {
        if (visual != null)
        {
            visual.transform.localScale = new Vector3(width, height, 1f);
            visual.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
        }
        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
            zoneCollider.size = new Vector3(width, height, 1f);
            zoneCollider.center = new Vector3(0f, height * 0.5f, 0f);
        }
        if (activateBurst != null)
        {
            ParticleSystem.ShapeModule shape = activateBurst.shape;
            // El emisor está rotado -90° en X
            // X = ancho del carril, Y = profundidad, Z = grosor.
            shape.scale = new Vector3(width, 0.4f, 0.1f);
        }
    }

    public void Show(State state)
    {
        Current = state;
        stateTime = 0f;
        if (visual != null) visual.enabled = state != State.Hidden;
        if (state == State.Active && activateBurst != null) activateBurst.Play();
        ApplyColor(1f);
    }

    private void Update()
    {
        if (Current == State.Hidden) return;
        stateTime += Time.deltaTime;

        if (Current == State.Warning)
        {
            // Parpadeo que se acelera conforme se acerca el golpe.
            float pulse = 0.65f + 0.35f * Mathf.Sin(stateTime * pulseSpeed * (1f + stateTime));
            ApplyColor(pulse);
        }
        else
        {
            // Destello inicial que se asienta en rojo.
            ApplyColor(1f + Mathf.Max(0f, 0.6f - stateTime * 4f));
        }
    }

    private void ApplyColor(float alphaMultiplier)
    {
        if (material == null) return;
        Color c = Current == State.Active ? activeColor : warningColor;
        c.a = Mathf.Clamp01(c.a * alphaMultiplier);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", c);
        if (material.HasProperty("_Color")) material.SetColor("_Color", c);
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material);
    }
}

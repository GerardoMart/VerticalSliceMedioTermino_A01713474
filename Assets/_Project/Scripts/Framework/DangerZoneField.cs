using UnityEngine;

// Divide el ancho de la arena en carriles e instancia un DangerZone (un prefab)
// por carril. Traduce posiciones del mundo a carriles y revisa si la cabeza
// del jugador está dentro de un carril rojo.
public class DangerZoneField : MonoBehaviour
{
    [SerializeField] private DangerZone zonePrefab;
    [SerializeField] private int laneCount = 8;
    [SerializeField] private float laneWidth = 0.75f;
    [SerializeField] private float zoneHeight = 9f;

    private DangerZone[] zones;

    public int LaneCount => laneCount;
    public float LaneWidth => laneWidth;
    public float Width => laneCount * laneWidth;

    private void Awake()
    {
        zones = new DangerZone[laneCount];
        float firstX = -Width * 0.5f + laneWidth * 0.5f;

        for (int i = 0; i < laneCount; i++)
        {
            DangerZone zone = Instantiate(zonePrefab, transform);
            zone.name = "Lane_" + i;
            zone.transform.localPosition = new Vector3(firstX + i * laneWidth, 0f, 0f);
            zone.transform.localRotation = Quaternion.identity;
            zone.Configure(laneWidth, zoneHeight);
            zone.Show(DangerZone.State.Hidden);
            zones[i] = zone;
        }
    }

    public int LaneOf(float worldX)
    {
        float local = worldX - transform.position.x + Width * 0.5f;
        return Mathf.Clamp(Mathf.FloorToInt(local / laneWidth), 0, laneCount - 1);
    }

    public void ShowPattern(bool[] danger, DangerZone.State state)
    {
        for (int i = 0; i < zones.Length; i++)
            zones[i].Show(danger[i] ? state : DangerZone.State.Hidden);
    }

    public void HideAll()
    {
        if (zones == null) return;
        foreach (DangerZone z in zones) z.Show(DangerZone.State.Hidden);
    }

    /// <summary>
    /// ¿La hitbox toca algún carril peligroso? `forgiveness` encoge la hitbox
    /// unos centímetros para que rozar el borde no cuente como golpe.
    /// </summary>
    public bool Overlaps(Bounds hitbox, bool[] danger, float forgiveness)
    {
        hitbox.Expand(-forgiveness * 2f);
        for (int i = 0; i < zones.Length; i++)
            if (danger[i] && zones[i].Bounds.Intersects(hitbox))
                return true;
        return false;
    }
}

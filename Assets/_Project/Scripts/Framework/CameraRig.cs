using UnityEngine;

// Cámara detrás de la nuca del jugador mirando al jefe.
// Por defecto la cámara es fija en X (followX = 0)
// para que los carriles no se muevan en pantalla y el patrón esté completo.
public class CameraRig : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.0f, -3.6f);
    [Range(0f, 1f)]
    [SerializeField] private float followX = 0f;
    [SerializeField] private float smoothing = 8f;

    private Vector3 basePosition;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;

    private void Start()
    {
        basePosition = TargetPosition();
        transform.position = basePosition;
        Aim();
    }

    private void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime; // sigue funcionando durante el hit-stop
        basePosition = Vector3.Lerp(basePosition, TargetPosition(), smoothing * dt);

        Vector3 shake = Vector3.zero;
        if (shakeTimer > 0f)
        {
            shakeTimer -= dt;
            float strength = shakeMagnitude * Mathf.Clamp01(shakeTimer / shakeDuration);
            shake = (Vector3)(Random.insideUnitCircle * strength);
        }

        transform.position = basePosition + shake;
        Aim();
    }

    private Vector3 TargetPosition()
    {
        float x = player != null ? player.position.x * followX : 0f;
        float z = player != null ? player.position.z : 0f;
        return new Vector3(x, 0f, z) + offset;
    }

    private void Aim()
    {
        if (lookTarget == null) return;
        Vector3 look = lookTarget.position;
        look.x = Mathf.Lerp(look.x, basePosition.x, 0.5f);
        transform.rotation = Quaternion.LookRotation(look - basePosition, Vector3.up);
    }

    public void Shake(float duration, float magnitude)
    {
        if (magnitude < shakeMagnitude && shakeTimer > 0f) return; // no pisar una sacudida más fuerte
        shakeDuration = Mathf.Max(0.01f, duration);
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }
}

using UnityEngine;

// Parpadeo suave de una luz (las antorchas). Solo atmósfera.
[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    [SerializeField] private float amount = 0.25f;
    [SerializeField] private float speed = 6f;

    private Light torch;
    private float baseIntensity;

    private void Awake()
    {
        torch = GetComponent<Light>();
        baseIntensity = torch.intensity;
    }

    private void Update()
    {
        float target = baseIntensity * (1f + Random.Range(-amount, amount));
        torch.intensity = Mathf.Lerp(torch.intensity, target, speed * Time.deltaTime);
    }
}

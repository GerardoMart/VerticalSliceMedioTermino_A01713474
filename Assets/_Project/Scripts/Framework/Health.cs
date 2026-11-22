using UnityEngine;

// Vida reutilizable ya que la usan tanto el jugador como el jefe.
// Solo guarda y resta vida; el GameManager revisa el resultado después de cada golpe.
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public int Max => maxHealth;
    public int Current { get; private set; }
    public bool IsDead => Current <= 0;
    public float Normalized => maxHealth > 0 ? (float)Current / maxHealth : 0f;

    private void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        Current = Mathf.Max(0, Current - amount);
    }
}

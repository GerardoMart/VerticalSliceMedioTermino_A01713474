using UnityEngine;

// Movimiento lateral del jugador (A/D o flechas) y sus animaciones.
// Se mueve con el Transform y no con Rigidbody. El único límite son los bordes
// de la arena.
[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float limitX = 2.75f;

    [Header("Referencias")]
    [Tooltip("Animator del modelo (el FBX importado).")]
    [SerializeField] private Animator animator;
    [Tooltip("Collider de la cabeza: es la hitbox contra los rectángulos rojos.")]
    [SerializeField] private Collider headHitbox;
    [Tooltip("Contenedor visual que se inclina al moverse (game feel).")]
    [SerializeField] private Transform model;
    [SerializeField] private float leanAngle = 10f;

    private float input;

    public bool CanMove { get; set; }
    public float MoveSpeed => moveSpeed;
    public Collider HeadHitbox => headHitbox;
    public Health Health { get; private set; }

    private void Awake()
    {
        Health = GetComponent<Health>();
    }

    private void Update()
    {
        input = CanMove ? InputReader.Horizontal() : 0f;

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x + input * moveSpeed * Time.deltaTime, -limitX, limitX);
        transform.position = p;

        // Clase 3 B2/B3: parámetro del Animator que el código cambia.
        if (animator != null) animator.SetFloat(AnimParams.Speed, Mathf.Abs(input));

        if (model != null)
        {
            Quaternion lean = Quaternion.Euler(0f, 0f, -input * leanAngle);
            model.localRotation = Quaternion.Slerp(model.localRotation, lean, 10f * Time.deltaTime);
        }
    }

    public void PlayAttack()
    {
        if (animator != null) animator.SetTrigger(AnimParams.Attack);
    }

    public void PlayHit()
    {
        if (animator != null) animator.SetTrigger(AnimParams.Hit);
    }

    public void PlayDeath()
    {
        if (animator != null) animator.SetBool(AnimParams.Dead, true);
    }

    public void PlayVictory()
    {
        if (animator != null) animator.SetTrigger(AnimParams.Victory);
    }
}

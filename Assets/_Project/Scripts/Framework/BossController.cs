using UnityEngine;

// El jefe con sus reglas de daño y bloqueo según la vida, sus animaciones
// y sus efectos. No decide cuándo actúa, ya que eso lo hace el GameManager.
[RequireComponent(typeof(Health))]
public class BossController : MonoBehaviour
{
    [Header("Reglas")]
    [Tooltip("Daño por golpe en la fase normal (100 a 31 de vida).")]
    [SerializeField] private int normalDamage = 15;
    [Tooltip("Daño por golpe en furia.")]
    [SerializeField] private int rageDamage = 20;
    [Tooltip("Con esta vida o menos entra en furia.")]
    [SerializeField] private int rageThreshold = 30;
    [Tooltip("Con esta vida o menos bloquea 2 recuadros en lugar de 1.")]
    [SerializeField] private int doubleBlockThreshold = 40;

    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private ParticleSystem blockParticles;
    [SerializeField] private ParticleSystem rageParticles;
    [SerializeField] private ParticleSystem stunParticles;

    public Health Health { get; private set; }
    public bool IsEnraged => Health.Current <= rageThreshold;
    public int DamagePerHit => IsEnraged ? rageDamage : normalDamage;
    public int BlocksPerTurn => Health.Current <= doubleBlockThreshold ? 2 : 1;

    private void Awake()
    {
        Health = GetComponent<Health>();
    }

    // Lo llama el GameManager después de cada golpe al jefe.
    public void UpdateRageEffects()
    {
        if (IsEnraged && !Health.IsDead && rageParticles != null && !rageParticles.isPlaying)
            rageParticles.Play();
    }

    public void PlayAttack()
    {
        if (animator != null) animator.SetTrigger(AnimParams.Attack);
    }

    public void PlayHit()
    {
        if (animator != null) animator.SetTrigger(AnimParams.Hit);
        if (hitParticles != null) hitParticles.Play();
    }

    public void PlayBlock()
    {
        if (animator != null) animator.SetTrigger(AnimParams.Block);
        if (blockParticles != null) blockParticles.Play();
    }

    public void SetStunned(bool stunned)
    {
        if (animator != null) animator.SetBool(AnimParams.Stunned, stunned);
        if (stunParticles == null) return;
        if (stunned) stunParticles.Play();
        else stunParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void PlayDeath()
    {
        if (animator != null) animator.SetBool(AnimParams.Dead, true);
        if (rageParticles != null) rageParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void PlayVictory()
    {
        if (animator != null) animator.SetTrigger(AnimParams.Victory);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Director de la pelea
// Controla los turnos con una corrutina, decide victoria/derrota y reinicia.
//
// Loop:
// Intro -> [Turno del jefe (3 ataques) -> Turno del jugador (1 golpe)
//       -> si se rompió la guardia: 2 golpes extra] ... -> Victoria / Derrota
public class GameManager : MonoBehaviour
{
    public enum Phase { Intro, EnemyTurn, PlayerTurn, GuardBreak, Victory, Defeat }

    [Header("Referencias")]
    [SerializeField] private PlayerController player;
    [SerializeField] private BossController boss;
    [SerializeField] private DangerZoneField field;
    [SerializeField] private HUDController hud;
    [SerializeField] private AttackChoiceUI choiceUI;
    [SerializeField] private CameraRig cameraRig;
    [SerializeField] private AudioManager audioManager;

    [Header("Turno del jefe")]
    [SerializeField] private int attacksPerTurn = 3;
    [Tooltip("Segundos del aviso amarillo en los ataques 1 y 2, fase normal (100 a 31 de vida).")]
    [SerializeField] private float warningNormal = 0.82f;
    [Tooltip("Segundos del aviso del tercer ataque (golpe fuerte), fase normal.")]
    [SerializeField] private float strongWarningNormal = 0.75f;
    [Tooltip("Segundos del aviso en los ataques 1 y 2 en furia (30 de vida o menos).")]
    [SerializeField] private float warningRage = 0.63f;
    [Tooltip("Segundos del aviso del tercer ataque en furia.")]
    [SerializeField] private float strongWarningRage = 0.58f;
    [Tooltip("El tercer ataque (golpe fuerte) multiplica el daño: 15 -> 30 normal, 20 -> 40 en furia.")]
    [SerializeField] private float strongAttackMultiplier = 2f;
    [Tooltip("Segundos que la zona roja hace daño.")]
    [SerializeField] private float activeTime = 0.45f;
    [SerializeField] private float pauseBetweenAttacks = 0.45f;
    [Tooltip("La animación de ataque arranca este tiempo antes del rojo para que el golpe coincida.")]
    [SerializeField] private float attackAnticipation = 0.35f;
    [SerializeField] private int minSafeLanes = 2;
    [SerializeField] private int minDangerLanes = 3;
    [SerializeField] private int maxDangerLanes = 5;
    [Range(0f, 1f)]
    [SerializeField] private float targetPlayerChance = 0.75f;
    [Tooltip("Margen (m) que se le perdona a la hitbox de la cabeza.")]
    [SerializeField] private float hitboxForgiveness = 0.04f;

    [Header("Turno del jugador")]
    [SerializeField] private int playerDamagePerHit = 15;
    [SerializeField] private int guardWindow = 5;
    [SerializeField] private int guardThreshold = 3;
    [SerializeField] private int guardBreakHits = 2;
    [SerializeField] private float resultPause = 1.1f;

    [Header("Game feel")]
    [SerializeField] private float hitStopDuration = 0.07f;

    private GuardTracker guard;
    private bool pendingGuardBreak;

    public Phase Current { get; private set; } = Phase.Intro;
    private bool IsOver => Current == Phase.Victory || Current == Phase.Defeat;

    // ─────────────────────────────────────────── ciclo de vida

    private void Awake()
    {
        Time.timeScale = 1f; // por si se reinició durante un hit-stop
        guard = new GuardTracker(guardWindow, guardThreshold);
    }

    private void Start()
    {
        if (!ValidateReferences()) { enabled = false; return; }

        hud.SetPlayerHealth(player.Health.Current, player.Health.Max);
        hud.SetBossHealth(boss.Health.Current, boss.Health.Max);
        hud.SetGuard(0, 0, guardWindow, guardThreshold);
        hud.SetRage(false);
        hud.ShowTurn("");
        hud.ShowIntro(true);

        choiceUI.Show(false);
        field.HideAll();
        player.CanMove = false;
        Current = Phase.Intro;
    }

    private void Update()
    {
        if (InputReader.RestartPressed())
        {
            RestartGame();
            return;
        }

        if (Current == Phase.Intro && InputReader.ConfirmPressed())
            StartFight();
    }

    // ─────────────────────────────────────────── botones de la UI

    public void StartFight()
    {
        if (Current != Phase.Intro) return;
        hud.ShowIntro(false);
        Current = Phase.EnemyTurn;
        StartCoroutine(FightLoop());
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ─────────────────────────────────────────── loop de turnos

    private IEnumerator FightLoop()
    {
        yield return new WaitForSeconds(0.6f);

        while (!IsOver)
        {
            yield return EnemyTurn();
            if (IsOver) yield break;

            yield return PlayerTurn();
            if (IsOver) yield break;

            if (pendingGuardBreak)
            {
                pendingGuardBreak = false;
                // El jefe "pierde su turno": en lugar de atacar, recibe 2 golpes.
                yield return GuardBreakTurn();
            }
        }
    }

    private IEnumerator EnemyTurn()
    {
        Current = Phase.EnemyTurn;
        player.CanMove = true;
        hud.ShowTurn(GameText.EnemyTurn);
        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < attacksPerTurn && !IsOver; i++)
        {
            hud.ShowTurn(GameText.EnemyTurn + "   (" + (i + 1) + "/" + attacksPerTurn + ")");

            // Cambios del playtest: avisos más cortos (más aún en furia) y el tercer
            // ataque es un golpe fuerte, con menos aviso y el doble de daño.
            bool isStrongAttack = i == attacksPerTurn - 1;
            float warningTime;
            if (boss.IsEnraged) warningTime = isStrongAttack ? strongWarningRage : warningRage;
            else warningTime = isStrongAttack ? strongWarningNormal : warningNormal;
            int damage = boss.DamagePerHit;
            if (isStrongAttack) damage = Mathf.RoundToInt(damage * strongAttackMultiplier);

            // 1) Patrón justo a partir de dónde está el jugador AHORA.
            int playerLane = field.LaneOf(player.transform.position.x);
            int reach = Mathf.FloorToInt(player.MoveSpeed * warningTime * 0.75f / field.LaneWidth);
            bool[] pattern = AttackPatternGenerator.Generate(
                field.LaneCount, playerLane, minSafeLanes, minDangerLanes, maxDangerLanes,
                reach, targetPlayerChance);

            // 2) Aviso amarillo.
            field.ShowPattern(pattern, DangerZone.State.Warning);
            PlaySfx(Sfx.Warning);
            if (isStrongAttack) hud.ShowMessage("¡GOLPE FUERTE!", new Color(1f, 0.55f, 0.15f), warningTime);
            yield return new WaitForSeconds(Mathf.Max(0f, warningTime - attackAnticipation));
            boss.PlayAttack();
            yield return new WaitForSeconds(Mathf.Min(attackAnticipation, warningTime));

            // 3) Rojo: durante activeTime se revisa cada frame si la cabeza está dentro.
            field.ShowPattern(pattern, DangerZone.State.Active);
            PlaySfx(Sfx.Slam);
            cameraRig.Shake(0.15f, 0.06f);

            bool wasHit = false;
            float t = 0f;
            while (t < activeTime && !IsOver)
            {
                if (!wasHit && field.Overlaps(player.HeadHitbox.bounds, pattern, hitboxForgiveness))
                {
                    wasHit = true;
                    DamagePlayer(damage);
                }
                t += Time.deltaTime;
                yield return null;
            }

            field.HideAll();
            if (IsOver) yield break;
            if (!wasHit) hud.ShowMessage("¡Esquivado!", new Color(0.6f, 1f, 0.7f), 0.6f);

            yield return new WaitForSeconds(pauseBetweenAttacks);
        }

        player.CanMove = false;
    }

    private IEnumerator PlayerTurn()
    {
        Current = Phase.PlayerTurn;
        player.CanMove = false;
        hud.ShowTurn(GameText.PlayerTurn);

        // El jefe decide en secreto qué recuadros bloquea antes de que elijas.
        bool[] blocked = BlockSelector.Pick(choiceUI.Count, boss.BlocksPerTurn);

        choiceUI.ResetVisuals();
        choiceUI.Show(true);

        int choice = 0;
        while (choice == 0)
        {
            if (IsOver) yield break;
            yield return null;
            choice = choiceUI.ConsumeChoice();
        }

        int index = choice - 1;
        bool wasBlocked = blocked[index];
        PlaySfx(Sfx.Select);
        player.PlayAttack();
        yield return new WaitForSeconds(0.25f);

        choiceUI.Reveal(index, blocked);
        if (wasBlocked)
        {
            boss.PlayBlock();
            PlaySfx(Sfx.Block);
            cameraRig.Shake(0.1f, 0.05f);
            hud.ShowMessage("¡BLOQUEADO!", new Color(0.75f, 0.8f, 0.9f));
        }
        else
        {
            DamageBoss(playerDamagePerHit);
        }

        if (!IsOver)
        {
            pendingGuardBreak = guard.Register(wasBlocked);
            hud.SetGuard(guard.BlocksInWindow, guard.Attempts, guard.Window, guard.Threshold);
        }

        yield return new WaitForSeconds(resultPause);
        choiceUI.Show(false);
    }

    private IEnumerator GuardBreakTurn()
    {
        Current = Phase.GuardBreak;
        boss.SetStunned(true);
        PlaySfx(Sfx.GuardBreak);
        cameraRig.Shake(0.3f, 0.15f);
        hud.ShowMessage("¡GUARDIA ROTA!", new Color(1f, 0.85f, 0.2f), 1.2f);
        hud.ShowTurn(GameText.GuardBreakTurn);
        hud.SetGuard(0, 0, guardWindow, guardThreshold);
        yield return new WaitForSeconds(1f);

        bool[] noBlocks = new bool[choiceUI.Count];
        for (int h = 0; h < guardBreakHits && !IsOver; h++)
        {
            hud.ShowTurn(GameText.GuardBreakTurn + "   (" + (h + 1) + "/" + guardBreakHits + ")");
            choiceUI.ResetVisuals();
            choiceUI.Show(true);

            int choice = 0;
            while (choice == 0)
            {
                if (IsOver) yield break;
                yield return null;
                choice = choiceUI.ConsumeChoice();
            }

            PlaySfx(Sfx.Select);
            player.PlayAttack();
            yield return new WaitForSeconds(0.2f);
            choiceUI.Reveal(choice - 1, noBlocks);
            DamageBoss(playerDamagePerHit);

            yield return new WaitForSeconds(0.7f);
            choiceUI.Show(false);
        }

        if (!IsOver) boss.SetStunned(false);
    }

    // ────────────────────────────────────────── daño y feedback

    private void DamagePlayer(int amount)
    {
        player.PlayHit();
        PlaySfx(Sfx.PlayerHit);
        cameraRig.Shake(0.3f, 0.25f);
        hud.FlashDamage();
        hud.ShowMessage("-" + amount, new Color(1f, 0.35f, 0.3f), 0.7f);
        StartCoroutine(HitStop());
        player.Health.TakeDamage(amount);
        hud.SetPlayerHealth(player.Health.Current, player.Health.Max);
        if (player.Health.IsDead) EndGame(false);
    }

    private void DamageBoss(int amount)
    {
        boss.PlayHit();
        PlaySfx(Sfx.BossHit);
        cameraRig.Shake(0.15f, 0.1f);
        hud.ShowMessage("¡GOLPE!  -" + amount, new Color(0.5f, 1f, 0.6f), 0.8f);
        StartCoroutine(HitStop());

        // Para avisar cuando el jefe cambia de fase (cambio del playtest).
        bool wasEnraged = boss.IsEnraged;
        bool wasDoubleBlock = boss.BlocksPerTurn == 2;

        boss.Health.TakeDamage(amount);
        hud.SetBossHealth(boss.Health.Current, boss.Health.Max);
        hud.SetRage(boss.IsEnraged && !boss.Health.IsDead);
        boss.UpdateRageEffects();
        if (boss.Health.IsDead)
        {
            EndGame(true);
            return;
        }

        if (!wasEnraged && boss.IsEnraged)
            StartCoroutine(PhaseAlert("¡FURIA!  Golpes de 20 y 40", new Color(1f, 0.45f, 0.1f)));
        else if (!wasDoubleBlock && boss.BlocksPerTurn == 2)
            StartCoroutine(PhaseAlert("¡AHORA BLOQUEA 2!", new Color(0.6f, 0.8f, 1f)));
    }

    // Espera a que termine el mensaje del golpe y luego muestra el aviso de fase.
    private IEnumerator PhaseAlert(string text, Color color)
    {
        yield return new WaitForSecondsRealtime(0.9f);
        if (IsOver) yield break;
        cameraRig.Shake(0.35f, 0.2f);
        hud.ShowMessage(text, color, 2f);
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }

    private void EndGame(bool victory)
    {
        if (IsOver) return;
        Current = victory ? Phase.Victory : Phase.Defeat;

        player.CanMove = false;
        field.HideAll();
        choiceUI.Show(false);
        boss.SetStunned(false);
        hud.SetRage(false);
        hud.ShowTurn("");

        if (victory)
        {
            boss.PlayDeath();
            player.PlayVictory();
        }
        else
        {
            player.PlayDeath();
            boss.PlayVictory();
        }

        if (audioManager != null) audioManager.StopMusic();
        PlaySfx(victory ? Sfx.Victory : Sfx.Defeat);
        StartCoroutine(ShowEndAfterDelay(victory));
    }

    private IEnumerator ShowEndAfterDelay(bool victory)
    {
        yield return new WaitForSecondsRealtime(1.3f);
        hud.ShowEnd(victory);
    }

    private void PlaySfx(Sfx sfx)
    {
        if (audioManager != null) audioManager.Play(sfx);
    }

    private bool ValidateReferences()
    {
        bool ok = true;
        if (player == null)   { Debug.LogError("[GameManager] Falta asignar Player.", this); ok = false; }
        if (boss == null)     { Debug.LogError("[GameManager] Falta asignar Boss.", this); ok = false; }
        if (field == null)    { Debug.LogError("[GameManager] Falta asignar Field.", this); ok = false; }
        if (hud == null)      { Debug.LogError("[GameManager] Falta asignar HUD.", this); ok = false; }
        if (choiceUI == null) { Debug.LogError("[GameManager] Falta asignar Choice UI.", this); ok = false; }
        if (cameraRig == null){ Debug.LogError("[GameManager] Falta asignar Camera Rig.", this); ok = false; }
        if (player != null && player.HeadHitbox == null)
        { Debug.LogError("[GameManager] El Player no tiene Head Hitbox asignada.", player); ok = false; }
        return ok;
    }
}

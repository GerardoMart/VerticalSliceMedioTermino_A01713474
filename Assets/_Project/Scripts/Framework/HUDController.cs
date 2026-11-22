using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Toda la información que el jugador ve en pantalla.
public class HUDController : MonoBehaviour
{
    [Header("Barras de vida (el relleno se escala con anchorMax.x)")]
    [SerializeField] private RectTransform playerHealthFill;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private RectTransform bossHealthFill;
    [SerializeField] private TMP_Text bossHealthText;

    [Header("Textos")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text guardText;
    [SerializeField] private TMP_Text rageText;

    [Header("Paneles")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text endTitle;
    [SerializeField] private TMP_Text endSubtitle;
    [SerializeField] private Image damageFlash;

    private Coroutine messageRoutine;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (rageText != null) rageText.gameObject.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);
        if (damageFlash != null) damageFlash.color = new Color(1f, 0f, 0f, 0f);
    }

    public void SetPlayerHealth(int current, int max) => SetBar(playerHealthFill, playerHealthText, current, max);
    public void SetBossHealth(int current, int max)   => SetBar(bossHealthFill, bossHealthText, current, max);

    private static void SetBar(RectTransform fill, TMP_Text label, int current, int max)
    {
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        if (fill != null)
        {
            Vector2 a = fill.anchorMax;
            a.x = ratio;
            fill.anchorMax = a;
        }
        if (label != null) label.text = current + " / " + max;
    }

    public void ShowTurn(string text)
    {
        if (turnText != null) turnText.text = text;
    }

    public void SetGuard(int blocks, int attempts, int window, int threshold)
    {
        if (guardText == null) return;
        guardText.text = "Guardia del jefe: " + blocks + "/" + threshold +
                         " bloqueos  (últimos " + attempts + "/" + window + " intentos)";
    }

    public void SetRage(bool enraged)
    {
        if (rageText != null) rageText.gameObject.SetActive(enraged);
    }

    public void ShowIntro(bool visible)
    {
        if (introPanel != null) introPanel.SetActive(visible);
    }

    public void ShowEnd(bool victory)
    {
        if (endPanel == null) return;
        endPanel.SetActive(true);
        if (endTitle != null)
        {
            endTitle.text = victory ? "¡VICTORIA!" : "DERROTA";
            endTitle.color = victory ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.3f, 0.3f);
        }
        if (endSubtitle != null)
            endSubtitle.text = victory ? "Derribaste al Coloso." : "El Coloso te aplastó.";
    }

    public void ShowMessage(string text, Color color, float duration = 0.9f)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text, color, duration));
    }

    private IEnumerator MessageRoutine(string text, Color color, float duration)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = text;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;
            // Aparece grande y se encoge (pequeño "punch").
            messageText.transform.localScale = Vector3.one * Mathf.Lerp(1.35f, 1f, Mathf.Min(1f, k * 5f));
            Color c = color;
            c.a = k < 0.7f ? 1f : Mathf.Lerp(1f, 0f, (k - 0.7f) / 0.3f);
            messageText.color = c;
            yield return null;
        }
        messageText.gameObject.SetActive(false);
        messageRoutine = null;
    }

    public void FlashDamage()
    {
        if (damageFlash == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float t = 0f;
        const float duration = 0.35f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            damageFlash.color = new Color(1f, 0f, 0f, Mathf.Lerp(0.35f, 0f, t / duration));
            yield return null;
        }
        damageFlash.color = new Color(1f, 0f, 0f, 0f);
        flashRoutine = null;
    }
}

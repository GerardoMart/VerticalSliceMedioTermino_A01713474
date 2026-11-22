using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Los cuatro recuadros semitransparentes del turno del jugador.
// Se elige con las teclas 1-4 o haciendo clic.
public class AttackChoiceUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image[] boxes = new Image[4];
    [SerializeField] private TMP_Text[] statusLabels = new TMP_Text[4];

    [SerializeField] private Color normalColor        = new Color(0.15f, 0.45f, 1f, 0.35f);
    [SerializeField] private Color hitColor           = new Color(0.3f, 1f, 0.55f, 0.6f);
    [SerializeField] private Color blockedColor       = new Color(0.45f, 0.45f, 0.5f, 0.6f);
    [SerializeField] private Color chosenBlockedColor = new Color(1f, 0.25f, 0.25f, 0.65f);

    private int clicked;

    public int Count => boxes.Length;

    public void Show(bool visible)
    {
        if (root != null) root.SetActive(visible);
        clicked = 0;
        // Si un botón quedó seleccionado tras un clic, la barra espaciadora lo
        // "volvería a pulsar" en el siguiente turno. Entonces le quitamos la selección.
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void ResetVisuals()
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i] != null)
            {
                boxes[i].color = normalColor;
                boxes[i].transform.localScale = Vector3.one;
            }
            if (statusLabels.Length > i && statusLabels[i] != null) statusLabels[i].text = "";
        }
    }

    /// <summary>Lo llaman los botones (el número va de 1 a 4).</summary>
    public void Click(int number)
    {
        clicked = number;
    }

    /// <summary>Devuelve 1-4 si el jugador eligió en este frame (teclado o clic), si no 0.</summary>
    public int ConsumeChoice()
    {
        int choice = clicked;
        clicked = 0;
        if (choice == 0) choice = InputReader.AttackChoicePressed();
        return choice > boxes.Length ? 0 : choice;
    }

    /// <summary>Muestra qué recuadros bloqueó el jefe y si el elegido conectó.</summary>
    public void Reveal(int chosenIndex, bool[] blocked)
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i] == null || !blocked[i]) continue;
            boxes[i].color = blockedColor;
            SetStatus(i, "BLOQUEADO");
        }

        if (boxes[chosenIndex] == null) return;
        boxes[chosenIndex].transform.localScale = Vector3.one * 1.1f;
        if (blocked[chosenIndex])
        {
            boxes[chosenIndex].color = chosenBlockedColor;
            SetStatus(chosenIndex, "¡BLOQUEADO!");
        }
        else
        {
            boxes[chosenIndex].color = hitColor;
            SetStatus(chosenIndex, "¡GOLPE!");
        }
    }

    private void SetStatus(int i, string text)
    {
        if (statusLabels.Length > i && statusLabels[i] != null) statusLabels[i].text = text;
    }
}

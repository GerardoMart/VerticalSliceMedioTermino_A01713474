using UnityEngine.InputSystem;

// Lectura de teclado en un solo lugar.
// El proyecto usa el Input System nuevo de Unity 6 (Keyboard.current).
public static class InputReader
{
    public static float Horizontal()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return 0f;

        float value = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)  value -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) value += 1f;
        return value;
    }

    // Devuelve 1-4 si se presionó esa tecla en este frame, 0 si no.
    public static int AttackChoicePressed()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return 0;

        if (k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame) return 1;
        if (k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame) return 2;
        if (k.digit3Key.wasPressedThisFrame || k.numpad3Key.wasPressedThisFrame) return 3;
        if (k.digit4Key.wasPressedThisFrame || k.numpad4Key.wasPressedThisFrame) return 4;
        return 0;
    }

    public static bool ConfirmPressed()
    {
        Keyboard k = Keyboard.current;
        return k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
    }

    public static bool RestartPressed()
    {
        Keyboard k = Keyboard.current;
        return k != null && k.rKey.wasPressedThisFrame;
    }
}

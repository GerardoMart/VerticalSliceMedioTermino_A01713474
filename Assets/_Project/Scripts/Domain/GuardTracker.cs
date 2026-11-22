using System.Collections.Generic;

// Regla "si el jefe bloquea 3 ataques dentro de los últimos 5 intentos,
// se le rompe la guardia". Sólo cuentan los 5 intentos más recientes.
public class GuardTracker
{
    private readonly int window;
    private readonly int threshold;
    private readonly Queue<bool> history = new Queue<bool>();

    public int BlocksInWindow { get; private set; }
    public int Attempts => history.Count;
    public int Window => window;
    public int Threshold => threshold;

    public GuardTracker(int window = 5, int threshold = 3)
    {
        this.window = window;
        this.threshold = threshold;
    }

    /// <summary>Registra un ataque del jugador. Devuelve true si la guardia se rompe.</summary>
    public bool Register(bool wasBlocked)
    {
        history.Enqueue(wasBlocked);
        if (wasBlocked) BlocksInWindow++;

        if (history.Count > window && history.Dequeue())
            BlocksInWindow--;

        if (BlocksInWindow >= threshold)
        {
            Reset();
            return true;
        }
        return false;
    }

    public void Reset()
    {
        history.Clear();
        BlocksInWindow = 0;
    }
}

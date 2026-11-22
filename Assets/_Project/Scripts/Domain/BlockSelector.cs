using UnityEngine;

// El jefe elige al azar qué recuadros bloquea (1 recuadro cuando tiene
// 100 de vida hasta 41, 2 recuadros cuando tiene 40 de vida o menos).
public static class BlockSelector
{
    public static bool[] Pick(int options, int blockCount)
    {
        bool[] blocked = new bool[options];
        // Nunca bloquea todo: siempre queda al menos un recuadro libre.
        int count = Mathf.Max(0, Mathf.Min(blockCount, options - 1));

        int placed = 0;
        while (placed < count)
        {
            int i = Random.Range(0, options);
            if (blocked[i]) continue;
            blocked[i] = true;
            placed++;
        }
        return blocked;
    }
}

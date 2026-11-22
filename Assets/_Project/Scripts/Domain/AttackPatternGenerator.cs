using System.Collections.Generic;
using UnityEngine;

// Genera qué carriles ataca el jefe garantizando que el
// patrón sea esquivable.
//   1. Siempre existe un hueco seguro de al menos `minSafeLanes` carriles
//      contiguos (más ancho que la cabeza del jugador).
//   2. Ese hueco está a una distancia alcanzable antes de que la advertencia
//      se vuelva roja (`maxReachLanes`).
//   3. Nunca se cubren más de `maxDangerLanes` carriles.
//   4. Con probabilidad `targetPlayerChance` el carril actual del jugador es
//      peligroso, para que quedarse quieto no sea una estrategia.
public static class AttackPatternGenerator
{
    public static bool[] Generate(
        int laneCount,
        int playerLane,
        int minSafeLanes,
        int minDangerLanes,
        int maxDangerLanes,
        int maxReachLanes,
        float targetPlayerChance)
    {
        bool[] danger = new bool[laneCount];
        if (laneCount <= minSafeLanes) return danger; // sin espacio para atacar con justicia

        playerLane = Mathf.Clamp(playerLane, 0, laneCount - 1);
        maxReachLanes = Mathf.Max(0, maxReachLanes);

        // 1) Bloques seguros candidatos que el jugador alcanza a tiempo.
        List<int> reachable = new List<int>();
        List<int> reachableWithoutPlayer = new List<int>();
        for (int start = 0; start <= laneCount - minSafeLanes; start++)
        {
            int end = start + minSafeLanes - 1;
            int distance = playerLane < start ? start - playerLane
                         : playerLane > end   ? playerLane - end
                         : 0;
            if (distance > maxReachLanes) continue;

            reachable.Add(start);
            if (distance > 0) reachableWithoutPlayer.Add(start);
        }

        if (reachable.Count == 0)
        {
            // Respaldo: hueco centrado en el jugador.
            reachable.Add(Mathf.Clamp(playerLane - minSafeLanes / 2, 0, laneCount - minSafeLanes));
        }

        // 2) ¿Este ataque apunta al jugador? Si sí, el hueco queda en otro lado.
        bool targetPlayer = Random.value < targetPlayerChance && reachableWithoutPlayer.Count > 0;
        List<int> pool = targetPlayer ? reachableWithoutPlayer : reachable;
        int safeStart = pool[Random.Range(0, pool.Count)];
        int safeEnd = safeStart + minSafeLanes - 1;

        // 3) Carriles que sí se pueden atacar.
        List<int> attackable = new List<int>();
        for (int i = 0; i < laneCount; i++)
            if (i < safeStart || i > safeEnd) attackable.Add(i);

        int maxAllowed = Mathf.Min(maxDangerLanes, attackable.Count);
        int minAllowed = Mathf.Min(Mathf.Max(1, minDangerLanes), maxAllowed);
        int target = Random.Range(minAllowed, maxAllowed + 1);

        int placed = 0;
        if (targetPlayer)
        {
            danger[playerLane] = true;
            attackable.Remove(playerLane);
            placed = 1;
        }

        while (placed < target && attackable.Count > 0)
        {
            int index = Random.Range(0, attackable.Count);
            danger[attackable[index]] = true;
            attackable.RemoveAt(index);
            placed++;
        }

        return danger;
    }
}

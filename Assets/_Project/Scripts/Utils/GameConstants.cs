// Utils: constantes compartidas por todo el proyecto (Lab 4, carpeta Utils).
// Tener los nombres de los parámetros del Animator en un solo lugar evita
// errores de dedo entre el código y el Animator Controller.

public static class AnimParams
{
    // Jugador y jefe
    public const string Speed   = "Speed";    // float  (jugador)
    public const string Attack  = "Attack";   // trigger
    public const string Hit     = "Hit";      // trigger
    public const string Dead    = "Dead";     // bool
    public const string Victory = "Victory";  // trigger

    // Solo jefe
    public const string Block   = "Block";    // trigger
    public const string Stunned = "Stunned";  // bool
}

public static class GameText
{
    public const string EnemyTurn      = "TURNO DEL JEFE  ·  ¡Esquiva con A / D!";
    public const string PlayerTurn     = "TU TURNO  ·  Presiona 1, 2, 3 o 4 (o haz clic en un recuadro)";
    public const string GuardBreakTurn = "¡GUARDIA ROTA!  ·  Golpea dos veces: presiona 1, 2, 3 o 4";
}

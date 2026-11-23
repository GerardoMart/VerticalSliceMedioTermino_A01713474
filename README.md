# LastDuel (Coloso)

Vertical slice de una pelea contra un jefe, hecho en Unity 6 (URP). Un caballero enfrenta a un yeti gigante en un cementerio, esquivando sus golpes y buscando el momento para romper su guardia.

Actividad de medio término · Modelación de sistemas multiagentes con gráficas computacionales  
Gerardo Martínez Carbajal · A01713474

## Cómo jugar

1. Abre el proyecto con **Unity 6.6 (6000.6.8f1)**.
2. Abre la escena `Assets/_Project/Scenes/BossFight.unity`.
3. En la vista Game selecciona **16:9 Aspect** y pulsa **Play**.

## Controles

| Acción | Tecla |
|---|---|
| Empezar la partida | Espacio o Enter |
| Moverse a la izquierda / derecha (turno del jefe) | A / D o flechas ← → |
| Atacar (turno del jugador) | 1, 2, 3 o 4, o clic en un recuadro |
| Reiniciar | R o botón «Reintentar» |

## Reglas

- El jugador y el jefe empiezan con 100 de vida. Gana quien deje al otro en 0.
- **Turno del jefe:** tres ataques. Los rectángulos amarillos avisan dónde va a golpear y luego se vuelven rojos; si tu cabeza está dentro, recibes daño. El tercer ataque es un golpe fuerte que hace el doble.
- **Tu turno:** elige uno de los cuatro recuadros. El jefe bloquea uno al azar, o dos cuando tiene 40 de vida o menos. Cada golpe que conecta le quita 15.
- **Guardia rota:** si el jefe bloquea 3 de tus últimos 5 ataques, pierde su turno y lo golpeas dos veces.
- **Furia:** con 30 de vida o menos, sus avisos son más cortos y sus golpes hacen 20 (40 el golpe fuerte).

## Créditos de assets

| Asset | Autor | Enlace | Licencia |
|---|---|---|---|
| Yeti (Ultimate Monsters) | Quaternius | https://quaternius.com/packs/ultimatemonsters.html | CC0 1.0 |
| Knight_Male (Ultimate Animated Character Pack) | Quaternius | https://quaternius.com | CC0 1.0 |
| Graveyard Kit | Kenney | https://kenney.nl/assets/graveyard-kit | CC0 1.0 |
| Fuente Liberation Sans (TextMesh Pro) | Red Hat, incluida por Unity | Incluida en TextMesh Pro | SIL OFL 1.1 |

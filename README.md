# Ki Zen

Juego 2D de disparos con temática zen para la materia de Desarrollo de Videojuegos.
Un monje defiende el jardín zen lanzando esferas de ki contra espíritus oscuros.

## Controles
- A/D o flechas: moverse
- Espacio: disparar ki
- R: reiniciar tras Game Over

## Estructura
- `Assets/Scripts/KiZenGame.cs`: controlador del juego (cámara, jugador, spawner, puntaje, vidas, UI).
- `Assets/Scripts/PlayerController.cs`: movimiento y disparo del monje.
- `Assets/Scripts/KiBall.cs`: proyectil; destruye espíritus y suma puntos.
- `Assets/Scripts/Spirit.cs`: enemigo que cae; si llega abajo resta una vida.
- `Assets/Scripts/Sprites.cs`: genera sprites por código.
- `Assets/Editor/KiZenSetup.cs`: crea la escena `Assets/Scenes/Main.unity` (menú *Ki Zen > Setup Scene*).

## MCP
Incluye el paquete [MCP for Unity](https://github.com/CoplayDev/unity-mcp) en `Packages/manifest.json`.
Abrir `Window > MCP for Unity`, configurar el cliente (Claude Code / Claude Desktop) y verificar el estado `Connected`.

# Roll a Ball

Proyecto de **Programación de Videojuegos** (Prof. Iván Mendoza), hecho en Unity 6 a partir del tutorial *Roll-a-ball*.

**Alumno:** Christopher Payero Estrella — **Matrícula:** 1-21-0909

**Jugar en línea:** https://christ2723.github.io/RollABall/

## Cambios sobre el tutorial

- Los coleccionables son **cilindros** (y algunos **cápsulas**) y en los niveles nuevos se acomodan en líneas, diagonales, zigzag y anillos.
- El texto de **"¡Ganaste!"** sale grande en el centro de la pantalla.
- **Música de ambiente** que sigue sonando al cambiar de escena (`MusicaAmbiente`).
- **Nombre y matrícula** en la esquina superior derecha de todas las escenas.
- **Menú principal** (Jugar, Opciones, Salir, elegir nivel) y una escena de **Opciones** con los controles.
  Al terminar el juego, el letrero se muestra 5 segundos y se vuelve al menú principal.
- **Timer de 60 segundos**: si llega a 0 el jugador pierde.
- **5 niveles nuevos**, todos con timer:
  | Nivel | Descripción |
  |---|---|
  | 1 | Clásico (el del tutorial, 12 cilindros en reloj) |
  | 2 | Líneas: arena más grande con muros interiores |
  | 3 | **Laberinto** |
  | 4 | **Obstáculos móviles**: barras que se mueven de lado a lado |
  | 5 | Plataformas flotantes y puentes estrechos |
  | 6 | Final: aspas giratorias, barras móviles y muros |

## Controles

- **Flechas / W A S D**: mover la bola
- **Esc**: volver al menú principal

## Scripts

| Script | Función |
|---|---|
| `JugadorController` | Movimiento con fuerzas, recoger coleccionables, contador, reaparecer al caer o chocar con un obstáculo |
| `CamaraController` | La cámara sigue al jugador manteniendo la distancia inicial |
| `Rotador` | Hace girar los coleccionables |
| `ControladorNivel` | Timer, ganar/perder y cambio de escena |
| `ObstaculoMovil` | Mueve o hace girar los obstáculos |
| `MenuController` | Botones del menú y de opciones |
| `MusicaAmbiente` | Música que no se corta entre escenas |
| `Editor/ConstructorProyecto` | Genera escenas, materiales, prefabs y la música, y compila el juego (menú **Roll a Ball** en Unity) |

## Cómo abrirlo

1. Abrir la carpeta con Unity 6 (6000.6.4f1) desde Unity Hub.
2. Abrir `Assets/Scenes/MenuPrincipal.unity` y darle Play.
3. Para compilar: menú **Roll a Ball > Compilar WebGL** o **Compilar Windows**.

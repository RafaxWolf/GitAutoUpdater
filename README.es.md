# GitAutoUpdater

Una aplicación de escritorio multiplataforma para mantener sincronizado un repositorio Git clonado con una rama remota. La rama `Desktop` es la edición con interfaz gráfica de GitAutoUpdater y está pensada para utilizar [Avalonia UI](https://avaloniaui.net/) con una experiencia de escritorio nativa en Windows, Linux y macOS.

***Language***
- [🇺🇸 English](README.md)
- 🇪🇸 Español

> **Estado de desarrollo:** La rama Desktop se está preparando para la migración a Avalonia UI. Los servicios de sincronización y el modelo de configuración son la base de la interfaz de escritorio; las vistas, los view models y el empaquetado para cada plataforma pueden seguir en desarrollo.

## Características

- Clona automáticamente un repositorio cuando la copia local no existe o no es válida.
- Supervisa una rama remota para detectar nuevos commits.
- Obtiene las referencias remotas y descarga los cambios cuando hay actualizaciones.
- Permite configurar la URL del repositorio, la rama, la ruta local, el intervalo de comprobación y el archivo de logs mediante `settings.json`.
- Muestra el estado de sincronización, el progreso, las advertencias y los errores mediante una interfaz de escritorio mientras avanza la migración a Avalonia.
- Guarda información detallada de la actividad en el directorio `Logs`.
- Detecta errores comunes de Git, incluidos errores de autenticación, red, repositorio y rama.
- Permite cancelar operaciones activas de Git y limpia los procesos de Git cuando se cierra la aplicación.

## Tecnología

- **C# / .NET 10**
- **Avalonia UI** para la interfaz de escritorio multiplataforma
- **Git CLI** para clonar, obtener referencias, comparar revisiones y descargar actualizaciones
- **Configuración JSON** mediante `settings.json`

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/downloads) instalado y disponible en el `PATH` del sistema
- Permiso de lectura sobre el repositorio remoto
- Un sistema operativo compatible con Avalonia UI, como Windows, Linux o macOS

Los repositorios privados pueden requerir credenciales de Git o un gestor de credenciales configurado. GitAutoUpdater no almacena las credenciales del repositorio.

## Configuración

Edita `settings.json` antes de iniciar la aplicación:

```json
{
  "AppSettings": {
    "GitSettings": {
      "RepoUrl": "https://github.com/owner/repository.git",
      "Branch": "main",
      "LocalPath": "Default",
      "UseDedicatedFolder": false
    },
    "IntSeconds": 60,
    "LogFile": "logs.log"
  }
}
```

### Referencia de configuración

| Propiedad | Descripción |
| --- | --- |
| `RepoUrl` | URL del repositorio Git que se clonará y supervisará. |
| `Branch` | Rama remota que se seguirá, por ejemplo `main` o `develop`. |
| `LocalPath` | Ruta de destino relativa al directorio de la aplicación. `Default` y `./` utilizan el directorio `Repository`. |
| `UseDedicatedFolder` | Cuando es `true`, crea una subcarpeta con el nombre del repositorio dentro de `LocalPath`. |
| `IntSeconds` | Segundos entre comprobaciones de actualizaciones. El valor debe ser mayor que 10. |
| `LogFile` | Nombre del archivo de logs. Los logs se almacenan en el directorio `Logs`. |

> La configuración predeterminada contiene un `RepoUrl` vacío. Sustitúyelo y revisa el resto de valores antes de ejecutar la aplicación.

## Compilar y ejecutar

Clona el repositorio y cambia a la rama Desktop:

```bash
git clone https://github.com/RafaxWolf/GitAutoUpdater.git
cd GitAutoUpdater
git checkout Desktop
```

Restaura las dependencias y compila el proyecto:

```bash
dotnet restore
dotnet build --configuration Release
```

Ejecuta la aplicación de escritorio con:

```bash
dotnet run --configuration Release
```

Cuando se agreguen los proyectos de plataforma y los destinos de empaquetado de Avalonia, aquí se documentarán los comandos específicos para publicar cada plataforma. Por ahora, utiliza la configuración del proyecto presente en esta rama como referencia principal.

## Dirección de la interfaz de escritorio

La interfaz de Avalonia está pensada para ofrecer:

- Una pantalla de configuración del repositorio para editar y validar los valores de `settings.json`.
- Un panel con el repositorio, la rama, la ruta local, el estado actual y la última comprobación.
- Controles para iniciar, pausar, actualizar y cancelar la sincronización.
- Un área de actividad en tiempo real para mostrar la salida de Git y mensajes de error comprensibles.
- Un visor de logs o un acceso directo al archivo generado.
- Un cierre controlado que cancele los procesos de Git activos antes de cerrar la ventana.

La lógica de sincronización debe mantenerse separada de la capa de presentación para que las operaciones de Git se ejecuten de forma asíncrona sin bloquear el hilo de la interfaz de Avalonia.

## Funcionamiento de la sincronización

1. Carga y valida `settings.json`.
2. Comprueba que Git esté instalado y sea accesible.
3. Resuelve la ruta del repositorio local.
4. Clona la rama configurada si el directorio local no es un repositorio Git válido.
5. Obtiene las referencias remotas en el intervalo configurado.
6. Compara el `HEAD` local con `origin/<Branch>`.
7. Descarga la rama cuando detecta una diferencia.
8. Muestra el progreso y los errores en la interfaz y en el archivo de logs.

## Estructura del proyecto

- `App.cs` — Punto de entrada actual y bucle de sincronización.
- `Core/` — Servicios de configuración, logs, temporización y Git.
- `Schemas/` — Modelos de configuración.
- `settings.json` — Plantilla de configuración de ejecución.
- `GitAutoUpdater.csproj` — Definición del proyecto .NET y sus dependencias.
- `GitAutoUpdater.slnx` — Archivo de solución.

A medida que avance la migración a Avalonia, pueden añadirse carpetas específicas de la interfaz como `Views/`, `ViewModels/`, `Assets/` y archivos de configuración de cada plataforma.

## Limitaciones y notas

- La rama Desktop está en desarrollo activo y su interfaz de Avalonia todavía puede no estar completa.
- Los cambios locales sin commit y los conflictos de merge no se resuelven automáticamente.
- Las credenciales de Git son gestionadas por Git y no se almacenan en esta aplicación.
- La aplicación está diseñada para continuar ejecutándose hasta que el usuario la pause o la cierre.

## Licencia

Este proyecto se distribuye bajo la [Licencia MIT](LICENSE.txt).

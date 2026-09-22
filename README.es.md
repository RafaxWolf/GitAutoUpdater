# GitAutoUpdater

Una aplicación ligera de consola para .NET que mantiene sincronizado un repositorio Git clonado con una rama remota. Clona automáticamente el repositorio cuando no existe una copia local, comprueba los cambios remotos en un intervalo configurable y descarga las actualizaciones cuando están disponibles.


***Language***
- 🇪🇸 Español
- [🇺🇸 English](README.md)

## Características

- Clona automáticamente un repositorio cuando la copia local no existe o no es válida.
- Supervisa una rama remota configurada para detectar cambios.
- Ejecuta `pull` automáticamente cuando los commits local y remoto son diferentes.
- Permite configurar el intervalo de comprobación, la rama, la ruta local y el archivo de logs mediante `settings.json`.
- Muestra mensajes de estado con colores en la consola y registra información detallada en un archivo de logs.
- Detecta e informa errores comunes de Git, incluidos errores de autenticación, red, repositorio y rama.
- Permite cancelar una operación de Git activa presionando `Q`.
- Limpia los procesos activos de Git cuando la aplicación finaliza.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/downloads) instalado y disponible en el `PATH` del sistema.
- Permiso de lectura sobre el repositorio remoto. Los repositorios privados pueden requerir credenciales de Git o un gestor de credenciales configurado.

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

> El `settings.json` inicial contiene valores de ejemplo. Sustituye `RepoUrl` y revisa el resto de valores antes de ejecutar el actualizador.

## Compilar y ejecutar

Clona el repositorio y cambia a la rama CLI:

```bash
git clone https://github.com/RafaxWolf/GitAutoUpdater.git
cd GitAutoUpdater
git checkout CLI
```

Restaura las dependencias, compila y ejecuta la aplicación:

```bash
dotnet restore
dotnet build --configuration Release
dotnet run --configuration Release
```

Para ejecutar la aplicación compilada:

```bash
dotnet ./bin/Release/net10.0/GitAutoUpdater.dll
```

La aplicación crea el directorio del repositorio local configurado y el directorio `Logs` relativos al directorio de ejecución.

## Controles

- Presiona `Q` para solicitar la cancelación de la operación de Git actual.
- Presiona `Ctrl+C` para detener la aplicación.

## Funcionamiento

1. Carga y valida `settings.json`.
2. Comprueba que Git esté instalado y sea accesible.
3. Resuelve la ruta del repositorio local.
4. Clona la rama configurada si el directorio local no es un repositorio Git válido.
5. Obtiene las referencias remotas en el intervalo configurado.
6. Compara el `HEAD` local con `origin/<Branch>`.
7. Ejecuta `git pull` cuando detecta una diferencia.
8. Registra la actividad y los errores en la consola y en el archivo de logs.

## Estructura del proyecto

- `App.cs` — Punto de entrada y bucle de actualización.
- `Core/` — Servicios de configuración, logs, temporización y Git.
- `Schemas/` — Modelos de configuración.
- `settings.json` — Plantilla de configuración de ejecución.
- `GitAutoUpdater.csproj` — Definición del proyecto .NET.

## Limitaciones y notas

- El actualizador está diseñado para sincronizar un directorio de trabajo y no resuelve automáticamente cambios locales sin commit ni conflictos de merge.
- Las credenciales de Git son gestionadas por Git y no se almacenan en esta aplicación.
- La aplicación se ejecuta continuamente hasta que se interrumpe.

## Licencia

Este proyecto se distribuye bajo la [Licencia MIT](LICENSE.txt).

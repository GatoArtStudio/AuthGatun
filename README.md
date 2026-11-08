# AuthGatun

AuthGatun es una aplicación de escritorio para la gestión de claves de doble factor de autenticación (2FA). No es un gestor de contraseñas, sino una herramienta para centralizar y administrar tus claves TOTP (Time-based One-Time Password) de tus aplicaciones favoritas.

La aplicación utiliza Argon2 para el login, asegurando que solo tú puedas acceder a tus claves 2FA, incluso si otros usuarios tienen acceso al mismo ordenador. Además, AuthGatun soporta múltiples cuentas de usuario, permitiendo que cada persona gestione su propio conjunto de claves de forma segura y separada.

## Características

*   **Gestión de Claves:** Almacena y gestiona tus claves de forma segura.
*   **Autenticación de Dos Factores (2FA):** Soporte para TOTP (Time-based One-Time Password) para una capa extra de seguridad.
*   **Cifrado Robusto:** Utiliza Argon2 para el hashing de contraseñas, garantizando que tus datos estén protegidos con los más altos estándares de seguridad.
*   **Multiplataforma:** Funciona en Windows y Linux.
*   **Compilación Nativa (AOT):** Compilado Ahead-Of-Time (AOT) para un rendimiento óptimo y un arranque más rápido.
*   **Almacenamiento Local:** Utiliza SQLite para almacenar los datos de forma local y segura en tu equipo.
*   **Segundo plano:** Desde Ajustes puedes mantener la app y tu sesión abiertas al cerrar la ventana (icono de bandeja con *Abrir*, *Cerrar sesión* y *Salir*). Si ejecutas AuthGatun de nuevo, trae al frente la ventana ya abierta. En Linux el icono de bandeja depende del escritorio (en GNOME necesitas la extensión AppIndicator). Con la sesión en segundo plano la clave de tu bóveda permanece en memoria, por eso el bloqueo automático por inactividad (configurable) también cuenta en segundo plano y *Salir* la borra de memoria.
*   **Códigos en vivo:** La pantalla Home muestra cada código TOTP con una cuenta regresiva circular que pasa a ámbar a los 10 segundos y a rojo a los 5, antes de que el código cambie.
*   **Privacidad de la ventana:** Desde Ajustes puedes hacer la ventana invisible o visible en capturas de pantalla y grabaciones. Está activado por defecto y solo está disponible en Windows; en otros sistemas la opción aparece deshabilitada.
*   **Modo privacidad:** Si lo activas en Ajustes, los códigos de Home se muestran ocultos (`••• •••`). Se revelan al pasar el puntero por la tarjeta o al pulsar el botón *Mostrar*, y vuelven a ocultarse solos a los 10 segundos o al salir el puntero. *Copiar* copia el código real sin revelarlo.
*   **Idiomas:** La app está disponible en inglés y español. La primera vez que se abre en un equipo usa el idioma del sistema (inglés si no es uno de los soportados) y se puede cambiar en cualquier momento desde Ajustes, sin reiniciar.
*   **Editar claves:** Desde el menú contextual de cada tarjeta (clic derecho) puedes renombrar el servicio y, opcionalmente, sustituir la clave secreta; si dejas la clave vacía se conserva la actual.
*   **Escanear QR:** En la pestaña de claves puedes leer el código QR que da el servicio desde una imagen del portapapeles, desde un archivo o soltándola sobre la ventana (también acepta un enlace `otpauth://` copiado como texto). Se rellena el formulario para que revises el nombre y guardes. Se admiten claves TOTP con SHA1, SHA256 o SHA512, de 6 a 8 dígitos y periodos de 1 a 300 segundos (los parámetros no estándar se muestran bajo el formulario y se guardan con la clave). También se admite el QR de migración de Google Authenticator (`otpauth-migration://`): si trae varias cuentas se importan todas, omitiendo las repetidas y las no compatibles (HOTP o MD5), y se avisa cuántas se importaron y cuántas se omitieron. Los respaldos exportados incluyen los parámetros de cada clave y los respaldos antiguos siguen pudiendo importarse.
*   **Búsqueda, favoritas y orden:** En la pantalla principal puedes filtrar tus claves por nombre del servicio (sin distinguir mayúsculas ni tildes), marcar claves como favoritas con la estrella (se muestran primero) y cambiar su orden con *Subir* / *Bajar* en el menú contextual de cada clave (el orden no se puede cambiar mientras hay una búsqueda activa).
*   **Tema:** Desde Ajustes puedes elegir entre tema oscuro (por defecto), claro, el del sistema o Catppuccin Mocha. El cambio se aplica al instante en todas las ventanas.

## Cómo Empezar

1.  **Descarga la última versión:** Ve a la [página de releases](https://github.com/gatoartstudio/AuthGatun/releases) y descarga el archivo `AuthGatun-linux-x64.zip` para Linux o `AuthGatun-win-x64.zip` para Windows.
2.  **Descomprime el archivo:** Extrae el contenido del archivo zip en una carpeta de tu elección.
3.  **Ejecuta la aplicación:**
    *   En **Windows**, ejecuta el archivo `AuthGatun.exe`.
    *   En **Linux**, abre una terminal, navega a la carpeta donde extrajiste los archivos y ejecuta el siguiente comando:
        ```bash
        ./AuthGatun
        ```

## Releases

Las versiones estables de AuthGatun se publican en la [página de releases de GitHub](https://github.com/gatoartstudio/AuthGatun/releases). Cada release incluye binarios precompilados para Windows y Linux.

El proceso de release se automatiza mediante GitHub Actions. Cuando se crea un nuevo tag con el formato `v*`, se activa un flujo de trabajo que compila la aplicación para Windows y Linux, y crea una nueva release con los binarios adjuntos.

### Integración continua

Cada push a `main`, `release`, `develop` o `hotfix`, y cada pull request, compila la solución y ejecuta los tests tanto en Linux como en Windows. Los pull requests hacia `develop`, `release` o `main` comprueban además que la publicación Native AOT (`linux-x64` y `win-x64`) compila correctamente.

### Build de desarrollo (`dev`)

Cada push a la rama `develop` publica automáticamente los binarios de Windows y Linux en un pre-release con el tag `dev` ("Development build <sha>"), que se sustituye en cada nuevo push. Sus archivos son `AuthGatun-dev-linux-x64.zip` y `AuthGatun-dev-win-x64.zip`, con versión `<versión>-dev.<número de ejecución>`.

> **Advertencia:** el pre-release `dev` es una build automática e inestable. Puede contener funcionalidades incompletas o errores y existe solo para probar los últimos cambios. Para uso normal descarga siempre la última release estable.

## Compilación desde el Código Fuente

Si prefieres compilar la aplicación desde el código fuente, sigue estos pasos:

1.  **Clona el repositorio:**
    ```bash
    git clone https://github.com/gatoartstudio/AuthGatun.git
    cd AuthGatun
    ```

2.  **Instala el SDK de .NET 8:** Asegúrate de tener instalado el [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) o una versión superior.

3.  **Restaura las dependencias:**
    ```bash
    dotnet restore AuthGatun.sln
    ```

4.  **Compila la aplicación:**
    *   Para una compilación de **Debug**:
        ```bash
        dotnet build AuthGatun.sln --configuration Debug
        ```
    *   Para una compilación de **Release**:
        ```bash
        dotnet build AuthGatun.sln --configuration Release
        ```

5.  **Ejecuta la aplicación:**
    ```bash
    dotnet run --project AuthGatun/AuthGatun.csproj
    ```

## Tecnologías Utilizadas

*   [.NET 8](https://dotnet.microsoft.com/)
*   [Avalonia UI](https://avaloniaui.net/) - Framework de UI multiplataforma.
*   [ReactiveUI](https://reactiveui.net/) - Framework MVVM para interfaces de usuario reactivas.
*   [SQLite](https://www.sqlite.org/) - Motor de base de datos SQL.
*   [Konscious.Security.Cryptography.Argon2](https://github.com/kmaragon/Konscious.Security.Cryptography) - Implementación de Argon2 para .NET.
*   [Otp.NET](https://github.com/kspearrin/Otp.NET) - Librería para la generación de contraseñas de un solo uso (OTP).
*   [GitHub Actions](https://github.com/features/actions) - Para la integración y el despliegue continuo (CI/CD).

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

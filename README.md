# 💬 ChatApp — Chat en tiempo real (Angular + .NET)

Aplicación de mensajería en tiempo real estilo Telegram/Discord, desarrollada como proyecto de portfolio. Permite chatear en tiempo real con amigos y grupos, con autenticación propia o mediante Google, estados de conexión en vivo, y envío de archivos adjuntos.

> 🎓 Proyecto desarrollado con fines de portfolio y aprendizaje, aplicando arquitectura en capas, buenas prácticas de separación de responsabilidades, y comunicación en tiempo real con SignalR.

---

## ✨ Funcionalidades

- **Autenticación** con email/contraseña (JWT) y con Google (OAuth 2.0)
- **Perfil de usuario**: foto de perfil, foto de portada, edición de datos
- **Sistema de amigos**: búsqueda de usuarios, solicitudes, aceptar/rechazar, eliminar
- **Preview de perfil**: ver el perfil de un amigo en un modal sin salir de la pantalla
- **Chats 1 a 1**: conversaciones privadas entre amigos
- **Grupos**: creación, gestión de miembros con roles (Admin/Miembro), salir del grupo
- **Mensajería en tiempo real** con SignalR (sin necesidad de recargar la página)
- **Estado online/offline en vivo**, sincronizado entre todas las pantallas
- **Archivos adjuntos** en los mensajes: imágenes con preview y otros archivos como descarga
- **Diseño responsive**, mobile-first, con soporte de tema claro/oscuro (DaisyUI)

---

## 🛠️ Stack tecnológico

### Backend
- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** (SQL Server)
- **SignalR** para comunicación en tiempo real
- **JWT** (JSON Web Tokens) para autenticación
- **Google.Apis.Auth** para login con Google
- **BCrypt.Net** para hasheo de contraseñas
- **Swagger / Swashbuckle** para documentación de la API

### Frontend
- **Angular 20** (standalone components, signals, control flow moderno `@if`/`@for`)
- **Tailwind CSS v4** + **DaisyUI** para estilos y componentes UI
- **PrimeNG** para componentes complejos puntuales
- **@microsoft/signalr** (cliente) para tiempo real
- **RxJS** para manejo de streams asíncronos

---

## 🏗️ Arquitectura

### Backend — Arquitectura en capas

El backend está separado en 4 proyectos, cada uno con una única responsabilidad. La regla de dependencias es estricta y se aplica a nivel de compilación (no se puede violar por accidente):

```
ChatApp.Api            → Controllers, Program.cs, Hubs de SignalR
    ↓ depende de
ChatApp.Application     → Servicios (lógica de negocio), DTOs, interfaces
    ↓ depende de
ChatApp.Domain          → Entidades puras (sin dependencias externas)

ChatApp.Infrastructure  → Implementación de repositorios, DbContext, EF Core
    ↓ depende de Domain y Application (implementa sus interfaces)
```

**Principios aplicados:**
- Los controladores **nunca** acceden directamente a la base de datos — todo pasa por servicios.
- Toda comunicación entre capas se hace a través de **interfaces** (inyección de dependencias), lo que permite testear la lógica de negocio sin depender de una base de datos real.
- Los **DTOs** (Data Transfer Objects) son los únicos objetos que viajan entre el frontend y el backend — las entidades de dominio nunca se exponen directamente, evitando filtrar datos sensibles (como el hash de contraseña).
- Validaciones de formato con **Data Annotations** en los DTOs; validaciones de negocio (reglas como "solo un admin puede...") viven en los servicios.

### Frontend — Organización por dominio

```
src/app/
├── core/           → Servicios globales, guards e interceptors (únicos en toda la app)
├── shared/          → Componentes y pipes reutilizables entre features
├── features/         → Cada funcionalidad de negocio, autocontenida
│   ├── auth/
│   ├── profile/
│   ├── friends/
│   ├── chat/
│   └── groups/
├── layout/          → Estructura visual general (sidebar, layout principal)
└── models/          → Interfaces TypeScript (espejo de los DTOs del backend)
```

**Principios aplicados:**
- **Signals** como fuente de verdad reactiva y centralizada en los servicios (`AuthService.currentUser`, `ChatService.chats`, `FriendService.friends`) — cualquier componente que los lea se actualiza automáticamente sin código adicional.
- **Standalone components** (sin NgModules), con `input()`/`output()` en vez de decoradores clásicos.
- Interceptor HTTP que agrega el JWT automáticamente a cada request, sin repetir lógica en cada servicio.
- Guards funcionales para proteger rutas que requieren sesión iniciada.

---

## 📋 Requisitos previos

Antes de clonar el proyecto, asegurate de tener instalado:

| Herramienta | Versión | Notas |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 o superior | |
| [Node.js](https://nodejs.org/) | 20.x o superior | Incluye npm |
| [Angular CLI](https://angular.dev/tools/cli) | 20.x o superior | `npm install -g @angular/cli` |
| [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) | — | Incluido con Visual Studio, o instalable por separado |
| [Git](https://git-scm.com/) | — | |

---

## 🚀 Instalación y puesta en marcha

### 1. Clonar el repositorio

```bash
git clone https://github.com/TU-USUARIO/chat-app-portfolio.git
cd chat-app-portfolio
```

### 2. Configurar el backend

```bash
cd backend/ChatApp.Api
cp appsettings.Example.json appsettings.Development.json
```

Editá `appsettings.Development.json` y completá:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ChatAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "ChatApp",
    "Audience": "ChatAppUsers",
    "ExpirationMinutes": 120
  },
  "Google": {
    "ClientId": "TU_CLIENT_ID.apps.googleusercontent.com"
  }
}
```

> 🔑 Para generar una clave JWT aleatoria en PowerShell:
> ```powershell
> [System.Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
> ```

> 🔑 Para obtener el `Client ID` de Google, creá credenciales OAuth en [Google Cloud Console](https://console.cloud.google.com) → APIs y servicios → Credenciales, agregando `http://localhost:4200` como origen autorizado.

### 3. Confiar el certificado HTTPS de desarrollo

```bash
dotnet dev-certs https --trust
```

### 4. Aplicar las migraciones de base de datos

```bash
cd ../  # volver a backend/
dotnet ef database update --project ChatApp.Infrastructure --startup-project ChatApp.Api
```

### 5. Levantar el backend

```bash
cd ChatApp.Api
dotnet run
```

La API queda disponible en `https://localhost:7xxx` (fijate el puerto exacto en la consola). Swagger disponible en `https://localhost:7xxx/swagger`.

### 6. Configurar el frontend

```bash
cd frontend/chat-app
npm install
```

Editá `src/environments/environment.development.ts` con el puerto real de tu backend:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7xxx/api'
};
```

Y en `src/app/features/auth/login/login.component.ts`, completá tu `client_id` de Google (el mismo del paso 2):

```typescript
google.accounts.id.initialize({
  client_id: 'TU_CLIENT_ID.apps.googleusercontent.com',
  // ...
});
```

### 7. Levantar el frontend

```bash
npm start
```

La app queda disponible en `http://localhost:4200`.

---

## 📁 Estructura del repositorio

```
chat-app-portfolio/
├── backend/
│   ├── ChatApp.sln
│   ├── ChatApp.Api/              # Controllers, Hubs, Program.cs
│   ├── ChatApp.Application/      # Servicios, DTOs, interfaces
│   ├── ChatApp.Domain/           # Entidades de dominio
│   └── ChatApp.Infrastructure/   # DbContext, repositorios, migraciones
├── frontend/
│   └── chat-app/
│       └── src/app/
│           ├── core/
│           ├── shared/
│           ├── features/
│           ├── layout/
│           └── models/
└── README.md
```

---


## 📄 Licencia

Este proyecto está bajo la licencia MIT — libre para usar como referencia de aprendizaje.
Gym Management System / Sistema de Gestión de Gimnasio

---

English Version

Description
The Gym Management System is an application designed to efficiently manage the operations of a gym. 
Members can book group classes, schedule personal trainer sessions, and access personalized workout routines. 
Gym staff can manage schedules, equipment, payments, and generate usage reports.

Key Features
- User Management: Registration, login, and role-based access (Administrator and Regular User).
- Full CRUD: Create, read, update, and delete memberships, classes, trainers, and equipment.
- Reservations & Scheduling: Members can book classes and personal trainer sessions.
- Reports & Analytics: Staff can generate reports on facility usage and activities.
- Security: Restricted access with authentication and role-based authorization.
- User Interface: Built with Razor Pages or MVC, using CSS and Bootstrap for an attractive design.

Detailed Features

1. Login and Registration
- Registration form for new users.
- Login system to validate credentials.

2. Authentication and Authorization
- Access restricted to registered users.
- Role-based permissions:
  - Administrator: Full permissions (create, edit, delete, view).
  - Regular User: Can view and edit their own data only.

3. Full CRUD
- Manage memberships, classes, trainers, and equipment.
- Actions controlled according to user permissions.

4. Database
- Entity Framework Core for data persistence.
- Database creation via migrations.

5. Best Practices
- Well-commented and documented code.
- Clean architecture and dependency injection.
- Installation guide included.

Installation
1. Clone the repository:
   git clone <REPOSITORY_URL>
2. Open Smart_Gym.sln in Visual Studio.
3. Restore NuGet packages:
   dotnet restore.
4. Apply migrations to create the database:
   Update-Database.
5. Run the Project.

Technologies Used
- Language: C#.
- Framework: ASP.NET Core (Razor Pages or MVC).
- Database: SQL Server with Entity Framework Core.
- Styling: CSS, Bootstrap.
- Version Control: Git.

Security & Roles
- Only authenticated users can access sensitive features.
- Administrator: Full control over all data.
- Regular User: Limited access to own data and reservations.

Contributions
1. Fork the repository.
2. Create a branch:
   git checkout -b feature-name.
3. Commit your changes:
   git commit -m "Description of changes"
4. Push to your fork and open a Pull Request.

Contact
For questions or suggestions:
devpablo.corrales@gmail.com

---

Versión en Español

Descripción
El Sistema de Gestión de Gimnasio es una aplicación diseñada para administrar eficientemente las operaciones de un gimnasio. 
Los miembros pueden reservar clases grupales, agendar sesiones con entrenadores personales y consultar rutinas personalizadas. 
El personal del gimnasio puede gestionar horarios, equipamiento, pagos y generar reportes de uso de las instalaciones.

Funcionalidades Principales
- Gestión de usuarios: Registro, login y manejo de roles (Administrador y Usuario Regular).
- CRUD completo: Creación, lectura, actualización y eliminación de membresías, clases, entrenadores y equipamiento.
- Reservas y agendas: Los miembros pueden reservar clases y sesiones con entrenadores.
- Reportes e informes: El personal puede generar reportes de uso de instalaciones y actividades.
- Seguridad: Acceso restringido mediante autenticación y autorización basada en roles.
- Interfaz de usuario: Construida con Razor Pages o MVC, usando CSS y Bootstrap para una experiencia visual atractiva.

Funcionalidades Detalladas

1. Login y Registro
- Formulario de registro para nuevos usuarios.
- Sistema de login para validar credenciales.

2. Autenticación y Autorización
- Acceso exclusivo a usuarios registrados.
- Roles definidos:
  - Administrador: Permisos completos (crear, editar, eliminar, visualizar).
  - Usuario Regular: Puede ver y editar solo sus propios datos.

3. CRUD Completo
- Gestión completa de membresías, clases, entrenadores y equipamiento.
- Acciones controladas según permisos de usuario.

4. Base de Datos
- Persistencia de datos mediante Entity Framework Core.
- Creación de base de datos mediante migraciones.

5. Buenas Prácticas
- Código comentado y documentado
- Arquitectura limpia e inyección de dependencias.
- Guía de instalación incluida.

Instalación
1. Clonar el repositorio:
   git clone <URL_DEL_REPOSITORIO>
2. Abrir Smart_Gym.sln en Visual Studio.
3. Restaurar paquetes NuGet:
   dotnet restore.
4. Aplicar migraciones para crear la base de datos:
   Update-Database
5. Ejecutar la aplicación.

Tecnologías Utilizadas
- Lenguaje: C#.
- Framework: ASP.NET Core (Razor Pages o MVC).
- Base de datos: SQL Server con Entity Framework Core.
- Estilos y diseño: CSS, Bootstrap.
- Control de versiones: Git.

Seguridad y Roles
- Solo usuarios autenticados pueden acceder a funcionalidades sensibles.
- Administrador: Control total sobre todos los datos.
- Usuario Regular: Acceso limitado a sus propios datos y reservas.

Contribuciones
1. Hacer un fork del repositorio.
2. Crear una rama:
   git checkout -b nombre-rama.
3. Hacer commit de los cambios:
   git commit -m "Descripción de los cambios"
4. Hacer push a la rama del fork y abrir un Pull Request

Contacto
Para dudas o sugerencias:
devpablo.corrales@gmail.com


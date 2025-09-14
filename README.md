Enterprise Platform for Project and Resource Management
Introduction
This project is a complete enterprise web application developed with ASP.NET Core MVC and Entity Framework Core. The goal is to simulate a real-world platform to support the management of a company's projects, contracts, and human resources, consolidating knowledge in software development, database modeling, and the implementation of security and authentication systems.

Key Features
The platform provides centralized management of a service company's main business processes:

Client Management: Registration of client companies, with access to their history of projects and contracts.

Employee Management: Registration of employees, including management of their positions and areas of expertise.

Project Management: Creation of detailed projects, associated with clients and contracts, including budgets, deadlines, and team allocation.

Contract Management: Creation of service contracts, associated with projects and clients.

Performance Reports: Generation and consultation of reports that consolidate critical business information, such as actual vs. budgeted cost and time spent on projects.

User Roles and Security
The application implements a robust authentication and authorization system based on roles, using ASP.NET Core Identity. Access to features is strictly controlled based on the user's role:

Administrator: Full control over the platform. Can manage clients, employees (and their login accounts), projects, and contracts.

Project Manager: Can create and manage projects and contracts. Has access to all performance reports.

Employee: Restricted access. Can only view the projects they are assigned to.

Client: Limited access to the details and reports of their own projects and contracts.

Technologies Used
Backend: ASP.NET Core MVC (.NET 8)

Database: Microsoft SQL Server

ORM: Entity Framework Core

Authentication: ASP.NET Core Identity

Frontend: HTML, CSS, JavaScript, Bootstrap

Database Structure
The entity-relationship diagram was modeled to reflect the business rules:

Client ↔ Project: 1-to-N (A client can have multiple projects).

Client ↔ Contract: 1-to-N (A client can have multiple contracts).

Project → Contract: 1-to-N (A project can have multiple contracts).

Employee ↔ Project: N-to-N (Implemented via a direct association in EF Core).

Employee ↔ Contract: N-to-N (Implemented with an explicit join table EmployeeContract).

How to Run the Project
Clone the Repository:

git clone <YOUR_REPOSITORY_URL>

Configure the Database:

Open the appsettings.json file.

Change the ConnectionString to point to your local SQL Server instance.

Apply Migrations:

Open the Package Manager Console in Visual Studio.

Run the command Update-Database to create the database schema.

Run the Application:

Press F5 or the start button in Visual Studio.

On startup, the Seeder will run automatically to populate the database with roles and test data.

Access Credentials (Seeder Examples)
Use the following credentials to test the different access levels:

Administrator:

Email: admin@hr.com

Password: Admin@123!

Project Manager (Example):

Email: gestor.ana@email.com

Password: Funcionario@123!

Employee (Example):

Email: dev.joao@email.com

Password: Funcionario@123!

Client (Example):

Email: cliente.tech@email.com

Password: Cliente@123!

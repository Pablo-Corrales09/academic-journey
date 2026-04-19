<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->
- [x] Verify that the copilot-instructions.md file in the .github directory is created.

- [x] Clarify Project Requirements

- [x] Scaffold the Project

- [x] Customize the Project

- [x] Install Required Extensions

- [x] Compile the Project

- [x] Create and Run Task

- [x] Launch the Project

- [x] Ensure Documentation is Complete


Workspace Custom Instructions
Core Technical Stack
This project utilizes C# with .NET 10.0. The web client is built using the Model-View-Controller (MVC) architecture, implementing the Repository Pattern for all data access logic. The frontend framework is Bootstrap 5.

Execution Guidelines
Progress Tracking: Use available tools to manage the checklist. After completing each step, mark it complete and provide a brief summary. Always read the current status before beginning a new task.

Communication Rules: Maintain conciseness. Avoid verbose explanations or printing exhaustive command outputs. If a step is skipped, state it briefly (e.g., No extensions needed). Do not explain the project structure unless specifically asked.

Development Rules: Use the current directory (.) as the working directory. Avoid adding media or external links. Use placeholders only with a note for replacement. Use DotNet Secrets for sensitive credentials and follow a Data-First design approach. Do not suggest commands to open the project in Visual Studio again once it is already active in VS Code.

Folder and Extensions: Always use the current directory (.) as the project root for terminal commands. Do not create new folders unless explicitly requested, except for a .vscode folder. If scaffolding mentions incorrect folder names, instruct the user to rename and reopen. Install only the extensions specified by the setup tools.

Project Content: If details are missing, start with a Hello World template. Avoid unnecessary links, images, or media. Confirm assumed features with the user before implementation.

Task Completion: The task is finished when the project scaffolds and compiles without errors, the .github/copilot-instructions.md and README.md files are up to date, and the user has clear instructions to debug or launch the project.



## Execution Guidelines
PROGRESS TRACKING:
- If any tools are available to manage the above todo list, use it to track progress through this checklist.
- After completing each step, mark it complete and add a summary.
- Read current todo list status before starting each new step.

COMMUNICATION RULES:
- Avoid verbose explanations or printing full command outputs.
- If a step is skipped, state that briefly (e.g. "No extensions needed").
- Do not explain project structure unless asked.
- Keep explanations concise and focused.

DEVELOPMENT RULES:
- Use '.' as the working directory unless user specifies otherwise.
- Avoid adding media or external links unless explicitly requested.
- Use placeholders only with a note that they should be replaced.
- Use VS Code API tool only for VS Code extension projects.
- Once the project is created, it is already opened in Visual Studio Code—do not suggest commands to open this project in Visual Studio again.
- If the project setup information has additional rules, follow them strictly.

FOLDER CREATION RULES:
- Always use the current directory as the project root.
- If you are running any terminal commands, use the '.' argument to ensure that the current working directory is used ALWAYS.
- Do not create a new folder unless the user explicitly requests it besides a .vscode folder for a tasks.json file.
- If any of the scaffolding commands mention that the folder name is not correct, let the user know to create a new folder with the correct name and then reopen it again in vscode.

EXTENSION INSTALLATION RULES:
- Only install extension specified by the get_project_setup_info tool. DO NOT INSTALL any other extensions.

PROJECT CONTENT RULES:
- If the user has not specified project details, assume they want a "Hello World" project as a starting point.
- Avoid adding links of any type (URLs, files, folders, etc.) or integrations that are not explicitly required.
- Avoid generating images, videos, or any other media files unless explicitly requested.
- If a feature is assumed but not confirmed, prompt the user for clarification before including it.
- If you are working on a VS Code extension, use the VS Code API tool with a query to find relevant VS Code API references and samples related to that query.

TASK COMPLETION RULES:
- Your task is complete when:
  - Project is successfully scaffolded and compiled without errors
  - copilot-instructions.md file in the .github directory exists
  - README.md file exists and is up to date
  - User is provided with clear instructions to debug/launch the project

- Work through each checklist item systematically.
- Keep communication concise and focused.
- Follow development best practices.
# Frontend and Razor Views (.cshtml) Guidelines

When assisting with the creation or modification of frontend code (HTML, CSS, JavaScript, and Razor `.cshtml` views), act as a UI/UX and ASP.NET Core 10 expert, strictly following these rules:

1. **Razor Ecosystem:** Always use native ASP.NET Core Tag Helpers (`asp-for`, `asp-action`, `asp-controller`, `asp-validation-for`) instead of standard HTML attributes when handling forms and links. Keep C# logic embedded in the view to an absolute minimum; complex logic must reside in the backend.

2. **Bootstrap Framework & Responsiveness:** You must use Bootstrap for styling and layouts. Ensure all interfaces are fully responsive (mobile-first approach), adapting seamlessly to different screen sizes using Bootstrap's grid system and native utility classes.

3. **Minimalist Dashboard Aesthetics:** Design visually clean and professional interfaces. Structure information using Bootstrap components like cards, modals, and well-spaced tables to avoid visual clutter. Prioritize consistent margins and padding.

4. **Interactivity and "Drill-down":** For data visualization in tables or lists, implement "drill-down" functionalities. Allow the user to click on rows or buttons to expand additional details dynamically using `fetch` requests (AJAX), avoiding full page reloads.

5. **Animations and Fluid Experience:** Integrate subtle CSS transitions and lightweight JavaScript animations (e.g., fade-in effects when loading asynchronous data, or smooth transitions when opening/closing menus and modals) to create a high-quality user experience.

6. **Form Handling and Validations:** All forms must be configured for client-side validation (ensuring the inclusion of scripts like `_ValidationScriptsPartial`). Form designs should visually highlight required fields and display error messages in an elegant, non-intrusive manner using Bootstrap's validation styling.
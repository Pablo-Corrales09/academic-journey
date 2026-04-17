document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-toggle-password]").forEach((button) => {
        button.addEventListener("click", () => {
            const targetId = button.getAttribute("data-toggle-password");
            if (!targetId) {
                return;
            }

            const input = document.getElementById(targetId);
            if (!input) {
                return;
            }

            const isPassword = input.getAttribute("type") === "password";
            input.setAttribute("type", isPassword ? "text" : "password");
            button.textContent = isPassword ? "Hide Password" : "See Password";
        });
    });
});

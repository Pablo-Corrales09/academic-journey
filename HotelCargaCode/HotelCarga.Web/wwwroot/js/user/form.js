const formObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            formObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.2 });

document.querySelectorAll("[data-animate]").forEach((element) => formObserver.observe(element));

const hashInput = document.querySelector("input[name='PasswordHash']");
if (hashInput) {
    hashInput.addEventListener("focus", () => hashInput.select());
}
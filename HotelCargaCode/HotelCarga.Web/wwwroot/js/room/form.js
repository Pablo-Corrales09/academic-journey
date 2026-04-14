document.querySelectorAll("[data-animate]").forEach((node, index) => {
    window.setTimeout(() => node.classList.add("is-visible"), 80 * index);
});

document.querySelectorAll(".room-input").forEach((input) => {
    input.addEventListener("focus", () => input.closest(".room-field")?.classList.add("is-active"));
    input.addEventListener("blur", () => input.closest(".room-field")?.classList.remove("is-active"));
});

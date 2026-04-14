document.querySelectorAll("[data-animate]").forEach((node, index) => {
    window.setTimeout(() => node.classList.add("is-visible"), 90 * index);
});

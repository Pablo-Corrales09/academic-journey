const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            observer.unobserve(entry.target);
        }
    });
}, { threshold: 0.18 });

document.querySelectorAll("[data-animate]").forEach((element) => observer.observe(element));

document.querySelectorAll("[data-count]").forEach((element) => {
    const target = Number(element.getAttribute("data-count"));
    const duration = 700;
    const startedAt = performance.now();

    const tick = (now) => {
        const progress = Math.min((now - startedAt) / duration, 1);
        element.textContent = Math.round(target * progress).toString();
        if (progress < 1) {
            requestAnimationFrame(tick);
        }
    };

    requestAnimationFrame(tick);
});
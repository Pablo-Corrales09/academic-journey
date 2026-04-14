const animatedNodes = document.querySelectorAll("[data-animate]");

const revealObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            revealObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.15 });

animatedNodes.forEach((node, index) => {
    node.style.transitionDelay = `${index * 60}ms`;
    revealObserver.observe(node);
});

document.querySelectorAll("[data-count]").forEach((node) => {
    const finalValue = Number(node.getAttribute("data-count"));
    let current = 0;
    const step = Math.max(1, Math.ceil(finalValue / 24));
    const timer = window.setInterval(() => {
        current += step;
        if (current >= finalValue) {
            node.textContent = finalValue.toString();
            window.clearInterval(timer);
            return;
        }

        node.textContent = current.toString();
    }, 24);
});

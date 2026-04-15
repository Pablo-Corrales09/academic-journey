const customerFormObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            customerFormObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.2 });

document.querySelectorAll("[data-animate]").forEach((element) => customerFormObserver.observe(element));
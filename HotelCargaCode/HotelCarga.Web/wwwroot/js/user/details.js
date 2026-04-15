const detailObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            detailObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.2 });

document.querySelectorAll("[data-animate]").forEach((element) => detailObserver.observe(element));
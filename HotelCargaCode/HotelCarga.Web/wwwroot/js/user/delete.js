const deleteObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            deleteObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.2 });

document.querySelectorAll("[data-animate]").forEach((element) => deleteObserver.observe(element));
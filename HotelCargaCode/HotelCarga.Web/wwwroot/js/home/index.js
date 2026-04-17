document.addEventListener("DOMContentLoaded", () => {
    // Sticky navbar scroll effect
    const landingHeader = document.querySelector(".landing-header");
    if (landingHeader) {
        const onScroll = () => {
            landingHeader.classList.toggle("is-scrolled", window.scrollY > 10);
        };
        window.addEventListener("scroll", onScroll, { passive: true });
        onScroll();
    }

    const revealElements = document.querySelectorAll("[data-reveal]");

    if (!("IntersectionObserver" in window)) {
        revealElements.forEach((element) => element.classList.add("is-visible"));
        return;
    }

    const observer = new IntersectionObserver(
        (entries, obs) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) {
                    return;
                }

                entry.target.classList.add("is-visible");
                obs.unobserve(entry.target);
            });
        },
        {
            threshold: 0.2,
            rootMargin: "0px 0px -8% 0px"
        }
    );

    revealElements.forEach((element) => observer.observe(element));
});

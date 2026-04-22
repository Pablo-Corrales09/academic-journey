document.addEventListener("DOMContentLoaded", () => {
    const animatedElements = document.querySelectorAll("[data-animate]");

    if ("IntersectionObserver" in window) {
        const bookingIndexObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.18 });

        animatedElements.forEach((element) => bookingIndexObserver.observe(element));
    } else {
        animatedElements.forEach((element) => element.classList.add("is-visible"));
    }

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

    const sliderContainer = document.querySelector(".services-slider-container");
    if (!sliderContainer) {
        return;
    }

    const cards = Array.from(sliderContainer.querySelectorAll(".landing-service-card"));
    const prevButton = sliderContainer.querySelector(".slider-btn.prev");
    const nextButton = sliderContainer.querySelector(".slider-btn.next");

    if (cards.length === 0) {
        return;
    }

    let currentIndex = cards.findIndex((card) => card.classList.contains("active"));
    if (currentIndex < 0) {
        currentIndex = 0;
        cards[0].classList.add("active");
    }

    let autoplayId = null;

    const setActive = (nextIndex) => {
        cards[currentIndex].classList.remove("active");
        currentIndex = (nextIndex + cards.length) % cards.length;
        cards[currentIndex].classList.add("active");
    };

    const nextSlide = () => setActive(currentIndex + 1);
    const previousSlide = () => setActive(currentIndex - 1);

    const startAutoplay = () => {
        if (autoplayId === null) {
            autoplayId = window.setInterval(nextSlide, 5000);
        }
    };

    const stopAutoplay = () => {
        if (autoplayId !== null) {
            window.clearInterval(autoplayId);
            autoplayId = null;
        }
    };

    const resetAutoplay = () => {
        stopAutoplay();
        startAutoplay();
    };

    prevButton?.addEventListener("click", () => {
        previousSlide();
        resetAutoplay();
    });

    nextButton?.addEventListener("click", () => {
        nextSlide();
        resetAutoplay();
    });

    startAutoplay();
});
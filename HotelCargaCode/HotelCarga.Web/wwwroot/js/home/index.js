document.addEventListener("DOMContentLoaded", () => {
    const currencyFormatter = new Intl.NumberFormat("es-CR", {
        style: "currency",
        currency: "CRC",
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    const landingHeader = document.querySelector(".landing-header");
    if (landingHeader) {
        const onScroll = () => {
            landingHeader.classList.toggle("is-scrolled", window.scrollY > 10);
        };
        window.addEventListener("scroll", onScroll, { passive: true });
        onScroll();
    }

    const revealElements = document.querySelectorAll("[data-reveal]");

    if ("IntersectionObserver" in window) {
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
    } else {
        revealElements.forEach((element) => element.classList.add("is-visible"));
    }

    const roomShowcase = document.querySelector("#room-showcase-content");
    if (!roomShowcase) {
        return;
    }

    const endpoint = roomShowcase.getAttribute("data-room-showcase-url");
    if (!endpoint) {
        return;
    }

    fetch(endpoint, {
        headers: {
            Accept: "application/json"
        }
    })
        .then(async (response) => {
            if (!response.ok) {
                const errorPayload = await response.json().catch(() => null);
                throw new Error(errorPayload?.message || "No fue posible cargar las habitaciones destacadas.");
            }

            return response.json();
        })
        .then((payload) => renderRoomShowcase(roomShowcase, payload.roomCategories || [], currencyFormatter))
        .catch((error) => {
            renderShowcaseMessage(
                roomShowcase,
                "Las categorías de habitaciones no están disponibles temporalmente.",
                error.message || "Inicie el servicio y actualice esta página para cargar la información en tiempo real."
            );
        });
});

function renderRoomShowcase(container, roomCategories, currencyFormatter) {
    if (!Array.isArray(roomCategories) || roomCategories.length === 0) {
        renderShowcaseMessage(
            container,
            "No hay habitaciones disponibles.",
            "Por favor, vuelva a consultar más tarde."
        );
        return;
    }

    const cardsMarkup = roomCategories
        .map((room) => {
            const amenities = Array.isArray(room.amenities)
                ? room.amenities.map((amenity) => `<span class="landing-amenity">${escapeHtml(amenity)}</span>`).join("")
                : "";

            return `
                <article class="landing-room-card is-visible">
                    <div class="landing-room-card__image" style="background-image: url('${escapeAttribute(room.imageUrl || "")}');"></div>
                    <div class="landing-room-card__body">
                        <div class="landing-room-card__head">
                            <span class="landing-chip">${escapeHtml(room.categoryName || "Standard")}</span>
                            <span class="landing-availability">${escapeHtml(room.availabilityLabel || "")}</span>
                        </div>
                        <p>${escapeHtml(room.description || "")}</p>
                        <div class="landing-room-card__amenities">${amenities}</div>
                        <div class="landing-rate-block">
                            <span>Tarifa por noche</span>
                            <strong>${formatPriceRange(room.nightlyRateFrom, room.nightlyRateTo, currencyFormatter)}</strong>
                        </div>
                    </div>
                </article>`;
        })
        .join("");

    container.innerHTML = `<div class="landing-room-grid">${cardsMarkup}</div>`;
}

function renderShowcaseMessage(container, title, description) {
    container.innerHTML = `
        <div class="landing-empty-state is-visible">
            <h3>${escapeHtml(title)}</h3>
            <p>${escapeHtml(description)}</p>
        </div>`;
}

function formatPriceRange(minRate, maxRate, currencyFormatter) {
    const min = Number(minRate || 0);
    const max = Number(maxRate || 0);

    if (min === max) {
        return currencyFormatter.format(min);
    }

    return `${currencyFormatter.format(min)} - ${currencyFormatter.format(max)}`;
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

function escapeAttribute(value) {
    return escapeHtml(value).replaceAll("`", "");
}

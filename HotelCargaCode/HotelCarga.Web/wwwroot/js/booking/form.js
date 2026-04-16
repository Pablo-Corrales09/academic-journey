const bookingFormObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            bookingFormObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.2 });

document.querySelectorAll("[data-animate]").forEach((element) => bookingFormObserver.observe(element));

const openInlineCustomerModalButton = document.getElementById("open-inline-customer-modal");
const inlineCustomerModal = document.getElementById("inline-customer-modal");
const inlineCustomerForm = document.getElementById("inline-customer-form");
const inlineCustomerFeedback = document.getElementById("inline-customer-feedback");
const bookingCustomerSelect = document.querySelector("select[name='CustomerId']");

const setInlineFeedback = (message, isError) => {
    if (!inlineCustomerFeedback) {
        return;
    }

    inlineCustomerFeedback.hidden = false;
    inlineCustomerFeedback.textContent = message;
    inlineCustomerFeedback.classList.toggle("user-alert", true);
    inlineCustomerFeedback.classList.toggle("user-alert--error", isError);
    inlineCustomerFeedback.classList.toggle("user-alert--success", !isError);
};

const closeInlineCustomerModal = () => {
    if (!inlineCustomerModal) {
        return;
    }

    inlineCustomerModal.classList.remove("is-open");
    inlineCustomerModal.setAttribute("aria-hidden", "true");
};

const openInlineCustomerModal = () => {
    if (!inlineCustomerModal) {
        return;
    }

    inlineCustomerModal.classList.add("is-open");
    inlineCustomerModal.setAttribute("aria-hidden", "false");
};

if (openInlineCustomerModalButton && inlineCustomerModal) {
    openInlineCustomerModalButton.addEventListener("click", openInlineCustomerModal);

    inlineCustomerModal.querySelectorAll("[data-close-inline-customer-modal='true']").forEach((element) => {
        element.addEventListener("click", closeInlineCustomerModal);
    });
}

if (inlineCustomerForm) {
    inlineCustomerForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        const formData = new FormData(inlineCustomerForm);
        const token = inlineCustomerForm.querySelector("input[name='__RequestVerificationToken']")?.value;

        try {
            const response = await fetch("/Booking/CreateCustomerInline", {
                method: "POST",
                body: formData,
                headers: {
                    RequestVerificationToken: token || ""
                }
            });

            const payload = await response.json();
            if (!response.ok || !payload.success) {
                setInlineFeedback(payload.message || "Customer creation failed.", true);
                return;
            }

            if (bookingCustomerSelect) {
                const option = document.createElement("option");
                option.value = payload.customerId;
                option.textContent = payload.customerLabel;
                option.selected = true;
                bookingCustomerSelect.appendChild(option);
                bookingCustomerSelect.value = String(payload.customerId);
            }

            setInlineFeedback(payload.message || "Customer created.", false);
            inlineCustomerForm.reset();
            setTimeout(closeInlineCustomerModal, 700);
        } catch {
            setInlineFeedback("Customer service is unavailable right now.", true);
        }
    });
}
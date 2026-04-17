(function () {
    const feedback = document.getElementById("queue-feedback");
    const useJsonCheckbox = document.getElementById("use-json-backend");
    const tableBody = document.getElementById("queue-table-body");
    const rawResponse = document.getElementById("raw-response");
    const resultSource = document.getElementById("result-source");
    const deleteModalId = "#waitingQueueDeleteModal";

    let pendingDeleteId = null;

    wireQuickButtons();
    wireCrudForms();
    wireQueryForms();
    wireDeleteModal();

    function wireQuickButtons() {
        document.querySelectorAll("[data-run-endpoint]").forEach((button) => {
            button.addEventListener("click", async () => {
                const endpoint = button.getAttribute("data-run-endpoint");
                await executeGet(endpoint, {});
            });
        });

        const clearButton = document.getElementById("clear-results-btn");
        clearButton?.addEventListener("click", () => {
            renderQueueRows([]);
            renderRaw([]);
            resultSource.textContent = "No data";
            hideFeedback();
        });
    }

    function wireCrudForms() {
        document.getElementById("create-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();

            const payload = {
                customer_id: toUInt(document.getElementById("create-customer-id")?.value),
                room_category_id: toByte(document.getElementById("create-room-category-id")?.value),
                status_id: toByte(document.getElementById("create-status-id")?.value),
                requested_check_in: toIso(document.getElementById("create-requested-check-in")?.value),
                check_out: toIsoOrNull(document.getElementById("create-check-out")?.value)
            };

            const data = await executeMutation("Create", "POST", payload);
            if (data) {
                await executeGet("GetAll", {});
            }
        });

        document.getElementById("update-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();

            const payload = {
                id: toUInt(document.getElementById("update-id")?.value),
                customer_id: toUInt(document.getElementById("update-customer-id")?.value),
                room_category_id: toByte(document.getElementById("update-room-category-id")?.value),
                status_id: toByte(document.getElementById("update-status-id")?.value),
                requested_check_in: toIso(document.getElementById("update-requested-check-in")?.value),
                check_out: toIsoOrNull(document.getElementById("update-check-out")?.value)
            };

            const data = await executeMutation("Update", "PUT", payload);
            if (data) {
                await executeGet("GetAll", {});
            }
        });

        document.getElementById("delete-open-modal-btn")?.addEventListener("click", () => {
            const idValue = document.getElementById("delete-id")?.value;
            pendingDeleteId = toUInt(idValue);
            document.getElementById("waitingQueueDeleteModalMessage").textContent = `¿Desea eliminar el registro #${pendingDeleteId} de la cola?`;
            $(deleteModalId).appendTo("body").modal("show");
        });

        document.getElementById("process-next-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();

            const roomCategoryId = toByte(document.getElementById("process-category-id")?.value);
            const targetStatusId = toByte(document.getElementById("process-target-status")?.value);

            const queue = await executeGet("GetFIFOQueueByRoomCategory", { roomCategoryId }, true);
            if (!Array.isArray(queue) || queue.length === 0) {
                showFeedback("warning", "No hay elementos pendientes para procesar en esa categoría.");
                return;
            }

            const first = queue[0];
            const payload = {
                id: Number(first.id),
                customer_id: Number(first.customer_id),
                room_category_id: Number(first.room_category_id),
                status_id: targetStatusId,
                requested_check_in: first.requested_check_in,
                check_out: first.check_out
            };

            await executeMutation("Update", "PUT", payload);
            await executeGet("GetFIFOQueueByRoomCategory", { roomCategoryId });
        });
    }

    function wireDeleteModal() {
        document.getElementById("confirm-delete-btn")?.addEventListener("click", async () => {
            if (!pendingDeleteId) {
                showFeedback("danger", "Debes indicar un Queue Id válido.");
                return;
            }

            $(deleteModalId).modal("hide");
            await executeDelete(pendingDeleteId);
            pendingDeleteId = null;
            await executeGet("GetAll", {});
        });
    }

    function wireQueryForms() {
        document.getElementById("get-by-id-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await executeGet("GetById", { id: toUInt(document.getElementById("query-id")?.value) });
        });

        document.getElementById("get-categories-by-customer-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await executeGet("GetCategoriesByCustomerId", { customerId: toUInt(document.getElementById("categories-customer-id")?.value) });
        });

        document.getElementById("related-by-customer-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await executeGet("GetWithRelatedEntitiesByCustomerId", { customerId: toUInt(document.getElementById("related-customer-id")?.value) });
        });

        document.getElementById("categories-by-status-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await executeGet("GetCategoriesByQueueStatusName", { statusName: document.getElementById("category-status-name")?.value?.trim() || "" });
        });

        document.getElementById("with-customer-by-status-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await executeGet("GetWithCustomerByQueueStatusName", { statusName: document.getElementById("with-customer-status-name")?.value?.trim() || "" });
        });

        document.getElementById("fifo-by-category-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await executeGet("GetFIFOQueueByRoomCategory", { roomCategoryId: toByte(document.getElementById("fifo-category-id")?.value) });
        });

        document.getElementById("fifo-by-category-date-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();

            await executeGet("GetFIFOQueueByRoomCategoryAndDateRange", {
                roomCategoryId: toByte(document.getElementById("fifo-range-category-id")?.value),
                checkInDate: toIso(document.getElementById("fifo-check-in")?.value),
                checkOutDate: toIso(document.getElementById("fifo-check-out")?.value)
            });
        });

        document.getElementById("count-pending-form")?.addEventListener("submit", async (event) => {
            event.preventDefault();
            await executeGet("CountPendingByRoomCategory", { roomCategoryId: toByte(document.getElementById("count-pending-category-id")?.value) });
        });
    }

    async function executeDelete(id) {
        const url = buildUrl("Delete", { id, useJson: useJsonCheckbox.checked });

        try {
            const response = await fetch(url, { method: "DELETE" });
            const data = await parseResponse(response);
            if (!response.ok) {
                throw buildError(data, response.status, "No se pudo eliminar el registro de cola.");
            }

            showFeedback("success", `Registro #${id} eliminado exitosamente.`);
            renderResult("Delete", data);
            return data;
        } catch (error) {
            showFeedback("danger", error.message || "Error inesperado al eliminar.");
            throw error;
        }
    }

    async function executeMutation(endpoint, method, payload) {
        const url = buildUrl(endpoint, { useJson: useJsonCheckbox.checked });

        try {
            const response = await fetch(url, {
                method,
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            const data = await parseResponse(response);
            if (!response.ok) {
                throw buildError(data, response.status, `No se pudo ejecutar ${endpoint}.`);
            }

            showFeedback("success", `${endpoint} ejecutado correctamente.`);
            renderResult(endpoint, data);
            return data;
        } catch (error) {
            showFeedback("danger", error.message || `Error ejecutando ${endpoint}.`);
            return null;
        }
    }

    async function executeGet(endpoint, queryParams, returnRaw = false) {
        const url = buildUrl(endpoint, {
            ...queryParams,
            useJson: useJsonCheckbox.checked
        });

        try {
            const response = await fetch(url, { method: "GET" });
            const data = await parseResponse(response);
            if (!response.ok) {
                throw buildError(data, response.status, `No se pudo consultar ${endpoint}.`);
            }

            showFeedback("success", `${endpoint} consultado correctamente.`);
            renderResult(endpoint, data);
            if (returnRaw) {
                return data;
            }
        } catch (error) {
            showFeedback("danger", error.message || `Error consultando ${endpoint}.`);
            return returnRaw ? null : undefined;
        }

        return returnRaw ? null : undefined;
    }

    function renderResult(source, data) {
        resultSource.textContent = source;
        renderRaw(data);

        if (Array.isArray(data)) {
            renderQueueRows(data);
            return;
        }

        if (isQueueRecord(data)) {
            renderQueueRows([data]);
            return;
        }

        renderQueueRows([]);
    }

    function renderRaw(data) {
        rawResponse.textContent = JSON.stringify(data, null, 2);
    }

    function renderQueueRows(items) {
        const queueItems = items.filter((item) => isQueueRecord(item));

        if (!queueItems.length) {
            tableBody.innerHTML = "<tr><td colspan='7' class='text-center text-muted py-4'>Resultado no corresponde a filas de waiting_queue.</td></tr>";
            return;
        }

        tableBody.innerHTML = queueItems.map((item) => {
            const statusText = extractStatusName(item);
            const statusClass = mapStatusClass(item, statusText);

            return `
                <tr>
                    <td>${escapeHtml(item.id)}</td>
                    <td>${escapeHtml(item.customer?.full_name || item.customer_name || item.customer_id)}</td>
                    <td>${escapeHtml(item.room_category?.category_name || item.category_name || item.room_category_id)}</td>
                    <td><span class="user-status-pill ${statusClass}">${escapeHtml(statusText)}</span></td>
                    <td>${formatDate(item.requested_check_in)}</td>
                    <td>${formatDate(item.check_out)}</td>
                    <td>${formatDate(item.created_at)}</td>
                </tr>`;
        }).join("");
    }

    function extractStatusName(item) {
        return item.status?.status_name || item.status_name || `STATUS ${item.status_id ?? "N/A"}`;
    }

    function mapStatusClass(item, statusText) {
        const normalized = (statusText || "").toString().trim().toUpperCase();
        const statusId = Number(item.status_id ?? 0);

        if (statusId === 1 || normalized.includes("PENDING") || normalized.includes("ACTIVE")) {
            return "user-status-pill--active";
        }

        if (statusId === 2 || normalized.includes("NOTIFIED") || normalized.includes("PROCESS")) {
            return "user-status-pill--pending";
        }

        if (statusId === 3 || normalized.includes("REMOVED") || normalized.includes("CANCEL") || normalized.includes("EXPIRED")) {
            return "user-status-pill--canceled";
        }

        return "user-status-pill--active";
    }

    function isQueueRecord(item) {
        return item && typeof item === "object" && "id" in item && "customer_id" in item && "room_category_id" in item;
    }

    function showFeedback(type, message) {
        feedback.classList.remove("d-none", "alert-success", "alert-danger", "alert-warning");
        feedback.classList.add(type === "danger" ? "alert-danger" : type === "warning" ? "alert-warning" : "alert-success");
        feedback.textContent = message;
    }

    function hideFeedback() {
        feedback.classList.add("d-none");
        feedback.textContent = "";
    }

    function buildUrl(endpoint, queryParams) {
        const params = new URLSearchParams();

        Object.entries(queryParams || {}).forEach(([key, value]) => {
            if (value === undefined || value === null || value === "") {
                return;
            }

            params.set(key, String(value));
        });

        const qs = params.toString();
        return `/WaitingQueue/Api/${endpoint}${qs ? `?${qs}` : ""}`;
    }

    async function parseResponse(response) {
        const text = await response.text();
        if (!text) {
            return {};
        }

        try {
            return JSON.parse(text);
        } catch {
            return { raw: text };
        }
    }

    function buildError(data, status, fallbackMessage) {
        const message = data?.message || data?.title || data?.raw || `${fallbackMessage} (HTTP ${status})`;
        return new Error(message);
    }

    function toUInt(value) {
        return Number.parseInt(String(value), 10);
    }

    function toByte(value) {
        return Number.parseInt(String(value), 10);
    }

    function toIso(value) {
        return new Date(value).toISOString();
    }

    function toIsoOrNull(value) {
        if (!value) {
            return null;
        }

        return toIso(value);
    }

    function formatDate(value) {
        if (!value) {
            return "-";
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return escapeHtml(value);
        }

        return date.toLocaleString();
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }
})();

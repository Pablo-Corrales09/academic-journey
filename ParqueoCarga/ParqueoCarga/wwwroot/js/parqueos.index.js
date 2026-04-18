(() => {
  const page = document.querySelector('.parqueos-page');
  if (!page) {
    return;
  }

  const endpoints = {
    listado: page.dataset.endpointListado,
    base: page.dataset.endpointBase
  };

  const token = document.querySelector('#antiForgeryFormParqueos input[name="__RequestVerificationToken"]')?.value ?? '';
  const filtersForm = document.getElementById('parqueosFiltersForm');
  const tableBody = document.querySelector('#parqueosTable tbody');
  const loadingState = document.getElementById('parqueosLoadingState');
  const emptyState = document.getElementById('parqueosEmptyState');
  const statusBadge = document.getElementById('parqueosStatusBadge');
  const summaryText = document.getElementById('parqueosSummaryText');

  const createButton = document.getElementById('openCreateParqueoModal');
  const emptyCreateButton = document.getElementById('emptyCreateParqueoButton');
  const refreshButton = document.getElementById('refreshParqueosButton');
  const saveButton = document.getElementById('saveParqueoButton');
  const deleteButton = document.getElementById('confirmDeleteParqueoButton');

  const modalElement = document.getElementById('parqueoModal');
  const deleteModalElement = document.getElementById('deleteParqueoModal');
  const modal = modalElement ? new bootstrap.Modal(modalElement) : null;
  const deleteModal = deleteModalElement ? new bootstrap.Modal(deleteModalElement) : null;

  const toastElement = document.getElementById('parqueosToast');
  const toast = toastElement ? new bootstrap.Toast(toastElement, { delay: 3200 }) : null;

  const state = {
    items: [],
    editingId: null,
    deletingId: null
  };

  const formatMoney = (value) => new Intl.NumberFormat('es-CR', {
    style: 'currency',
    currency: 'CRC',
    minimumFractionDigits: 2
  }).format(value ?? 0);

  const showToast = (message) => {
    const body = document.getElementById('parqueosToastBody');
    if (body) {
      body.textContent = message;
    }

    toast?.show();
  };

  const setBusy = (button, busy) => {
    if (!button) {
      return;
    }

    button.disabled = busy;
    button.querySelector('.spinner-border')?.classList.toggle('d-none', !busy);
  };

  const setStatus = (text) => {
    if (statusBadge) {
      statusBadge.textContent = text;
    }
  };

  const clearValidation = () => {
    document.querySelectorAll('#parqueoForm .is-invalid').forEach((input) => input.classList.remove('is-invalid'));
    document.querySelectorAll('#parqueoForm [data-valmsg-for]').forEach((node) => {
      node.textContent = '';
    });
  };

  const applyValidationErrors = (errors) => {
    Object.entries(errors || {}).forEach(([key, messages]) => {
      const normalized = key.replace('request.', '').replace('Request.', '');
      const inputName = normalized.charAt(0).toLowerCase() + normalized.slice(1);
      const input = document.querySelector(`#parqueoForm [name="${inputName}"]`);
      const feedback = document.querySelector(`#parqueoForm [data-valmsg-for="${normalized}"]`);

      if (input) {
        input.classList.add('is-invalid');
      }

      if (feedback) {
        feedback.textContent = Array.isArray(messages) ? messages.join(' ') : String(messages);
      }
    });
  };

  const collectFilters = () => {
    const formData = new FormData(filtersForm);
    const params = new URLSearchParams();

    for (const [key, value] of formData.entries()) {
      const text = String(value).trim();
      if (text) {
        params.append(key, text);
      }
    }

    return params;
  };

  const renderStats = (payload) => {
    document.getElementById('totalParqueosStat').textContent = payload.total ?? 0;
    document.getElementById('precioMinimoStat').textContent = payload.stats?.precioMinimo == null ? '-' : formatMoney(payload.stats.precioMinimo);
    document.getElementById('precioMaximoStat').textContent = payload.stats?.precioMaximo == null ? '-' : formatMoney(payload.stats.precioMaximo);
  };

  const renderTable = (items) => {
    tableBody.innerHTML = items.map((item) => `
      <tr>
        <td>#${item.id}</td>
        <td>${item.nombre}</td>
        <td>${item.provincia}</td>
        <td>${formatMoney(item.precioHora)}</td>
        <td class="text-end">
          <button type="button" class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${item.id}">Editar</button>
          <button type="button" class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${item.id}">Eliminar</button>
        </td>
      </tr>`).join('');

    emptyState.classList.toggle('d-none', items.length > 0);
  };

  const openCreateModal = () => {
    state.editingId = null;
    clearValidation();
    document.getElementById('parqueoForm').reset();
    document.getElementById('parqueoId').value = '';
    document.getElementById('parqueoModalTitle').textContent = 'Crear parqueo';
    document.getElementById('parqueoFormFeedback').classList.add('d-none');
    modal?.show();
  };

  const openEditModal = async (id) => {
    setBusy(saveButton, true);
    setStatus('Cargando detalle...');

    try {
      const response = await fetch(`${endpoints.base}/${id}`);
      const payload = await response.json();

      if (!response.ok) {
        throw new Error(payload.message || 'No fue posible obtener el parqueo.');
      }

      state.editingId = Number(id);
      clearValidation();
      document.getElementById('parqueoModalTitle').textContent = 'Editar parqueo';
      document.getElementById('parqueoId').value = payload.id;
      document.getElementById('parqueoNombreInput').value = payload.nombre ?? '';
      document.getElementById('parqueoProvinciaInput').value = payload.provincia ?? '';
      document.getElementById('parqueoPrecioInput').value = payload.precioHora ?? 0;
      document.getElementById('parqueoFormFeedback').classList.add('d-none');
      modal?.show();
    } catch (error) {
      showToast(error.message || 'No fue posible abrir el parqueo.');
    } finally {
      setBusy(saveButton, false);
      setStatus('Sincronizado');
    }
  };

  const openDeleteModal = (id) => {
    const item = state.items.find((row) => Number(row.id) === Number(id));
    if (!item) {
      return;
    }

    state.deletingId = Number(id);
    document.getElementById('deleteParqueoSummary').textContent = `${item.nombre} (${item.provincia})`;
    deleteModal?.show();
  };

  const fetchListado = async () => {
    setBusy(document.getElementById('aplicarFiltrosParqueosButton'), true);
    loadingState.classList.remove('d-none');
    setStatus('Sincronizando...');

    try {
      const query = collectFilters();
      const response = await fetch(`${endpoints.listado}?${query.toString()}`);
      const payload = await response.json();

      if (!response.ok) {
        throw new Error(payload.message || payload.title || 'No fue posible obtener el listado de parqueos.');
      }

      state.items = payload.items ?? [];
      renderTable(state.items);
      renderStats(payload);
      summaryText.textContent = `${payload.total ?? state.items.length} parqueos visibles`;
      setStatus('Sincronizado');
    } catch (error) {
      state.items = [];
      renderTable([]);
      renderStats({ total: 0, stats: {} });
      summaryText.textContent = 'No fue posible cargar datos';
      setStatus('Con incidencia');
      showToast(error.message || 'Error al consultar parqueos.');
    } finally {
      loadingState.classList.add('d-none');
      setBusy(document.getElementById('aplicarFiltrosParqueosButton'), false);
    }
  };

  const submitParqueo = async () => {
    clearValidation();
    setBusy(saveButton, true);

    const feedback = document.getElementById('parqueoFormFeedback');
    feedback.classList.add('d-none');

    const payload = {
      nombre: document.getElementById('parqueoNombreInput').value.trim(),
      provincia: document.getElementById('parqueoProvinciaInput').value.trim(),
      precioHora: Number(document.getElementById('parqueoPrecioInput').value)
    };

    const isEditing = Number.isInteger(state.editingId) && state.editingId > 0;
    const method = isEditing ? 'PUT' : 'POST';
    const url = isEditing ? `${endpoints.base}/${state.editingId}` : endpoints.base;

    try {
      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          RequestVerificationToken: token
        },
        body: JSON.stringify(payload)
      });

      const text = await response.text();
      const result = text ? JSON.parse(text) : {};

      if (!response.ok) {
        if (result.errors) {
          applyValidationErrors(result.errors);
          feedback.textContent = 'Corrige los campos marcados para continuar.';
          feedback.classList.remove('d-none');
          return;
        }

        throw new Error(result.message || result.title || 'No fue posible guardar el parqueo.');
      }

      modal?.hide();
      showToast(result.message || 'Operación completada.');
      await fetchListado();
    } catch (error) {
      feedback.textContent = error.message || 'No fue posible guardar el parqueo.';
      feedback.classList.remove('d-none');
    } finally {
      setBusy(saveButton, false);
    }
  };

  const deleteParqueo = async () => {
    if (!state.deletingId) {
      return;
    }

    setBusy(deleteButton, true);

    try {
      const response = await fetch(`${endpoints.base}/${state.deletingId}`, {
        method: 'DELETE',
        headers: {
          RequestVerificationToken: token
        }
      });

      const text = await response.text();
      const result = text ? JSON.parse(text) : {};

      if (!response.ok) {
        throw new Error(result.message || result.title || 'No fue posible eliminar el parqueo.');
      }

      deleteModal?.hide();
      state.deletingId = null;
      showToast(result.message || 'Parqueo eliminado correctamente.');
      await fetchListado();
    } catch (error) {
      showToast(error.message || 'No fue posible eliminar el parqueo.');
    } finally {
      setBusy(deleteButton, false);
    }
  };

  filtersForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    await fetchListado();
  });

  createButton?.addEventListener('click', openCreateModal);
  emptyCreateButton?.addEventListener('click', openCreateModal);
  refreshButton?.addEventListener('click', async () => {
    await fetchListado();
  });
  saveButton?.addEventListener('click', submitParqueo);
  deleteButton?.addEventListener('click', deleteParqueo);

  tableBody.addEventListener('click', async (event) => {
    const button = event.target.closest('[data-action]');
    if (!button) {
      return;
    }

    const id = Number(button.dataset.id);
    if (button.dataset.action === 'edit') {
      await openEditModal(id);
      return;
    }

    if (button.dataset.action === 'delete') {
      openDeleteModal(id);
    }
  });

  fetchListado();
})();

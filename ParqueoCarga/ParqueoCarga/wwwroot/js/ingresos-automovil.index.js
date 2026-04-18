(() => {
  const page = document.querySelector('.ingresos-page');
  if (!page) {
    return;
  }

  const endpoints = {
    listado: page.dataset.endpointListado,
    base: page.dataset.endpointBase
  };

  const token = document.querySelector('#antiForgeryFormIngresos input[name="__RequestVerificationToken"]')?.value ?? '';
  const filtersForm = document.getElementById('ingresosFiltersForm');
  const tableBody = document.querySelector('#ingresosTable tbody');
  const loadingState = document.getElementById('ingresosLoadingState');
  const emptyState = document.getElementById('ingresosEmptyState');
  const summaryText = document.getElementById('ingresosSummaryText');
  const statusBadge = document.getElementById('ingresosStatusBadge');

  const createButton = document.getElementById('openCreateIngresoModal');
  const emptyCreateButton = document.getElementById('emptyCreateIngresoButton');
  const refreshButton = document.getElementById('refreshIngresosButton');
  const saveButton = document.getElementById('saveIngresoButton');
  const deleteButton = document.getElementById('confirmDeleteIngresoButton');
  const consultarPrecioButton = document.getElementById('consultarPrecioHoraButton');

  const ingresoModalElement = document.getElementById('ingresoModal');
  const deleteModalElement = document.getElementById('deleteIngresoModal');
  const ingresoModal = ingresoModalElement ? new bootstrap.Modal(ingresoModalElement) : null;
  const deleteModal = deleteModalElement ? new bootstrap.Modal(deleteModalElement) : null;

  const toastElement = document.getElementById('ingresosToast');
  const toast = toastElement ? new bootstrap.Toast(toastElement, { delay: 3200 }) : null;

  const state = {
    items: [],
    editingId: null,
    deletingId: null
  };

  const formatDateTime = (value) => {
    if (!value) {
      return '-';
    }

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat('es-CR', {
      dateStyle: 'short',
      timeStyle: 'short'
    }).format(parsed);
  };

  const toDateTimeLocal = (value) => {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }

    const pad = (n) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  };

  const formatMoney = (value) => new Intl.NumberFormat('es-CR', {
    style: 'currency',
    currency: 'CRC',
    minimumFractionDigits: 2
  }).format(value ?? 0);

  const showToast = (message) => {
    const body = document.getElementById('ingresosToastBody');
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
    document.querySelectorAll('#ingresoForm .is-invalid').forEach((input) => input.classList.remove('is-invalid'));
    document.querySelectorAll('#ingresoForm [data-valmsg-for]').forEach((node) => {
      node.textContent = '';
    });
  };

  const applyValidationErrors = (errors) => {
    Object.entries(errors || {}).forEach(([key, messages]) => {
      const normalized = key.replace('request.', '').replace('Request.', '');
      const inputName = normalized.charAt(0).toLowerCase() + normalized.slice(1);
      const input = document.querySelector(`#ingresoForm [name="${inputName}"]`);
      const feedback = document.querySelector(`#ingresoForm [data-valmsg-for="${normalized}"]`);

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
    document.getElementById('totalIngresosStat').textContent = payload.stats?.total ?? payload.total ?? 0;
    document.getElementById('activosIngresosStat').textContent = payload.stats?.activos ?? 0;
    document.getElementById('montoIngresosStat').textContent = formatMoney(payload.stats?.montoAcumulado ?? 0);
  };

  const renderTable = (items) => {
    tableBody.innerHTML = items.map((item) => `
      <tr>
        <td>#${item.consecutivo}</td>
        <td>${item.idParqueo}</td>
        <td>${item.idAutomovil}</td>
        <td>${formatDateTime(item.fechaEntrada)}</td>
        <td>${formatDateTime(item.fechaSalida)}</td>
        <td>${item.montoTotalPagar == null ? '-' : formatMoney(item.montoTotalPagar)}</td>
        <td class="text-end">
          <button type="button" class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${item.consecutivo}">Editar</button>
          <button type="button" class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${item.consecutivo}">Eliminar</button>
        </td>
      </tr>`).join('');

    emptyState.classList.toggle('d-none', items.length > 0);
  };

  const openCreateModal = () => {
    state.editingId = null;
    clearValidation();
    document.getElementById('ingresoForm').reset();
    document.getElementById('ingresoConsecutivo').value = '';
    document.getElementById('ingresoModalTitle').textContent = 'Registrar ingreso';
    document.getElementById('ingresoFechaEntradaInput').value = toDateTimeLocal(new Date());
    document.getElementById('ingresoFormFeedback').classList.add('d-none');
    ingresoModal?.show();
  };

  const openEditModal = async (id) => {
    setBusy(saveButton, true);
    setStatus('Cargando detalle...');

    try {
      const response = await fetch(`${endpoints.base}/${id}`);
      const payload = await response.json();

      if (!response.ok) {
        throw new Error(payload.message || 'No fue posible cargar el ingreso.');
      }

      state.editingId = Number(id);
      clearValidation();
      document.getElementById('ingresoModalTitle').textContent = 'Editar ingreso';
      document.getElementById('ingresoConsecutivo').value = payload.consecutivo;
      document.getElementById('ingresoIdParqueoInput').value = payload.idParqueo;
      document.getElementById('ingresoIdAutomovilInput').value = payload.idAutomovil;
      document.getElementById('ingresoFechaEntradaInput').value = toDateTimeLocal(payload.fechaEntrada);
      document.getElementById('ingresoFechaSalidaInput').value = toDateTimeLocal(payload.fechaSalida);
      document.getElementById('ingresoFormFeedback').classList.add('d-none');
      ingresoModal?.show();
    } catch (error) {
      showToast(error.message || 'No fue posible cargar el ingreso.');
    } finally {
      setBusy(saveButton, false);
      setStatus('Sincronizado');
    }
  };

  const openDeleteModal = (id) => {
    const ingreso = state.items.find((row) => Number(row.consecutivo) === Number(id));
    if (!ingreso) {
      return;
    }

    state.deletingId = Number(id);
    document.getElementById('deleteIngresoSummary').textContent = `Consecutivo #${ingreso.consecutivo} · Parqueo ${ingreso.idParqueo} · Automóvil ${ingreso.idAutomovil}`;
    deleteModal?.show();
  };

  const fetchListado = async () => {
    setBusy(document.getElementById('aplicarFiltrosIngresosButton'), true);
    loadingState.classList.remove('d-none');
    setStatus('Sincronizando...');

    try {
      const query = collectFilters();
      const response = await fetch(`${endpoints.listado}?${query.toString()}`);
      const payload = await response.json();

      if (!response.ok) {
        throw new Error(payload.message || payload.title || 'No fue posible consultar ingresos.');
      }

      state.items = payload.items ?? [];
      renderTable(state.items);
      renderStats(payload);
      summaryText.textContent = `${payload.total ?? state.items.length} ingresos visibles`;
      setStatus('Sincronizado');
    } catch (error) {
      state.items = [];
      renderTable([]);
      renderStats({ total: 0, stats: {} });
      summaryText.textContent = 'No fue posible cargar datos';
      setStatus('Con incidencia');
      showToast(error.message || 'No fue posible consultar ingresos.');
    } finally {
      loadingState.classList.add('d-none');
      setBusy(document.getElementById('aplicarFiltrosIngresosButton'), false);
    }
  };

  const submitIngreso = async () => {
    clearValidation();
    setBusy(saveButton, true);

    const feedback = document.getElementById('ingresoFormFeedback');
    feedback.classList.add('d-none');

    const payload = {
      idParqueo: Number(document.getElementById('ingresoIdParqueoInput').value),
      idAutomovil: Number(document.getElementById('ingresoIdAutomovilInput').value),
      fechaEntrada: document.getElementById('ingresoFechaEntradaInput').value,
      fechaSalida: document.getElementById('ingresoFechaSalidaInput').value || null
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

        throw new Error(result.message || result.title || 'No fue posible guardar el ingreso.');
      }

      ingresoModal?.hide();
      showToast(result.message || 'Operación completada.');
      await fetchListado();
    } catch (error) {
      feedback.textContent = error.message || 'No fue posible guardar el ingreso.';
      feedback.classList.remove('d-none');
    } finally {
      setBusy(saveButton, false);
    }
  };

  const deleteIngreso = async () => {
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
        throw new Error(result.message || result.title || 'No fue posible eliminar el ingreso.');
      }

      deleteModal?.hide();
      state.deletingId = null;
      showToast(result.message || 'Ingreso eliminado correctamente.');
      await fetchListado();
    } catch (error) {
      showToast(error.message || 'No fue posible eliminar el ingreso.');
    } finally {
      setBusy(deleteButton, false);
    }
  };

  const consultarPrecioPorHora = async () => {
    const input = document.getElementById('precioParqueoIdInput');
    const output = document.getElementById('precioHoraOutput');
    const idParqueo = Number(input.value);

    if (!Number.isInteger(idParqueo) || idParqueo < 1) {
      output.className = 'alert alert-warning mb-0';
      output.textContent = 'Ingresa un ID de parqueo válido para consultar precio.';
      return;
    }

    setBusy(consultarPrecioButton, true);

    try {
      const response = await fetch(`${endpoints.base}/precio-por-hora/${idParqueo}`);
      const payload = await response.json();

      if (!response.ok) {
        throw new Error(payload.message || 'No fue posible obtener el precio por hora.');
      }

      output.className = 'alert alert-success mb-0';
      output.textContent = `Parqueo #${idParqueo}: ${formatMoney(payload.precioHora)}`;
    } catch (error) {
      output.className = 'alert alert-danger mb-0';
      output.textContent = error.message || 'No fue posible obtener el precio por hora.';
    } finally {
      setBusy(consultarPrecioButton, false);
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

  saveButton?.addEventListener('click', submitIngreso);
  deleteButton?.addEventListener('click', deleteIngreso);
  consultarPrecioButton?.addEventListener('click', consultarPrecioPorHora);

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

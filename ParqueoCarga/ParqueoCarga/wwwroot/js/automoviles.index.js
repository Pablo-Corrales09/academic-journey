(() => {
  const page = document.querySelector('.automoviles-page');
  if (!page) {
    return;
  }

  const endpoints = {
    listado: page.dataset.endpointListado,
    base: page.dataset.endpointBase
  };

  const filtersForm = document.getElementById('filtersForm');
  const automovilForm = document.getElementById('automovilForm');
  const automovilesGrid = document.getElementById('automovilesGrid');
  const emptyState = document.getElementById('emptyState');
  const cardsLoadingState = document.getElementById('cardsLoadingState');
  const filtersFeedback = document.getElementById('filtersFeedback');
  const formFeedback = document.getElementById('formFeedback');
  const refreshDataButton = document.getElementById('refreshDataButton');
  const resetFiltersButton = document.getElementById('resetFiltersButton');
  const openCreateModalButton = document.getElementById('openCreateModalButton');
  const emptyStateCreateButton = document.getElementById('emptyStateCreateButton');
  const saveAutomovilButton = document.getElementById('saveAutomovilButton');
  const confirmDeleteButton = document.getElementById('confirmDeleteButton');
  const deleteSummary = document.getElementById('deleteSummary');
  const statusBadge = document.getElementById('statusBadge');
  const token = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]')?.value ?? '';

  const modalElement = document.getElementById('automovilModal');
  const deleteModalElement = document.getElementById('deleteAutomovilModal');
  const automovilModal = modalElement ? new bootstrap.Modal(modalElement) : null;
  const deleteModal = deleteModalElement ? new bootstrap.Modal(deleteModalElement) : null;
  const toastElement = document.getElementById('appToast');
  const appToast = toastElement ? new bootstrap.Toast(toastElement, { delay: 3200 }) : null;

  const state = {
    items: [],
    editingId: null,
    deletingId: null
  };

  const metricElements = {
    total: document.getElementById('totalAutomovilesValue'),
    fabricantes: document.getElementById('totalFabricantesValue'),
    anioMinimo: document.getElementById('anioMinimoValue'),
    anioMaximo: document.getElementById('anioMaximoValue'),
    status: document.getElementById('metricStatusText')
  };

  const showToast = (message) => {
    const body = document.getElementById('appToastBody');
    if (body) {
      body.textContent = message;
    }

    appToast?.show();
  };

  const setButtonBusy = (button, busy) => {
    if (!button) {
      return;
    }

    button.disabled = busy;
    button.querySelector('.spinner-border')?.classList.toggle('d-none', !busy);
  };

  const showFeedback = (element, message, isError) => {
    if (!element) {
      return;
    }

    element.textContent = message;
    element.classList.remove('d-none', 'is-error', 'is-success');
    element.classList.add(isError ? 'is-error' : 'is-success');
  };

  const clearFeedback = (element) => {
    if (!element) {
      return;
    }

    element.textContent = '';
    element.classList.add('d-none');
    element.classList.remove('is-error', 'is-success');
  };

  const setStatusText = (text) => {
    const span = statusBadge?.querySelector('span:last-child');
    if (span) {
      span.textContent = text;
    }
  };

  const collectFilters = () => {
    const formData = new FormData(filtersForm);
    const params = new URLSearchParams();

    for (const [key, value] of formData.entries()) {
      const normalizedValue = String(value).trim();
      if (normalizedValue) {
        params.append(key, normalizedValue);
      }
    }

    return params;
  };

  const renderMetrics = (stats, total) => {
    metricElements.total.textContent = total;
    metricElements.fabricantes.textContent = stats?.modelosUnicos ?? 0;
    metricElements.anioMinimo.textContent = stats?.anioMinimo ?? '-';
    metricElements.anioMaximo.textContent = stats?.anioMaximo ?? '-';
    metricElements.status.textContent = total === 1 ? '1 automóvil visible' : `${total} automóviles visibles`;
  };

  const getMonogram = (automovil) => {
    const base = automovil.fabricante || automovil.tipo || 'PC';
    return base
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('') || 'PC';
  };

  const getVehicleImage = (automovil) => {
    const descriptor = `${automovil.tipo ?? ''} ${automovil.fabricante ?? ''}`.toLowerCase();

    if (descriptor.includes('moto') || descriptor.includes('motocic') || descriptor.includes('motorcycle')) {
      return 'https://images.unsplash.com/photo-1558981806-ec527fa84c39?auto=format&fit=crop&w=900&q=80';
    }

    if (descriptor.includes('pickup') || descriptor.includes('pick up') || descriptor.includes('camioneta')) {
      return 'https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?auto=format&fit=crop&w=900&q=80';
    }

    if (descriptor.includes('4x4') || descriptor.includes('suv') || descriptor.includes('todoterreno') || descriptor.includes('offroad')) {
      return 'https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=900&q=80';
    }

    if (descriptor.includes('electrico') || descriptor.includes('eléctrico') || descriptor.includes('electric') || descriptor.includes('ev') || descriptor.includes('tesla')) {
      return 'https://images.unsplash.com/photo-1549924231-f129b911e442?auto=format&fit=crop&w=900&q=80';
    }

    return 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=900&q=80';
  };

  const escapeHtml = (value) => String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');

  const renderCards = (items) => {
    automovilesGrid.innerHTML = items.map((automovil, index) => {
      const vehicleImage = getVehicleImage(automovil);
      const vehicleAlt = `Fotografía editorial de ${escapeHtml(automovil.fabricante)} ${escapeHtml(automovil.tipo)}`;

      return `
      <article class="automovil-card" data-reveal style="transition-delay:${index * 45}ms">
        <img class="vehicle-photo" src="${vehicleImage}" alt="${vehicleAlt}" />
        <div class="vehicle-surface">
          <div class="vehicle-topline">
            <span class="vehicle-monogram">${escapeHtml(getMonogram(automovil))}</span>
            <span class="vehicle-year">${escapeHtml(automovil.anio)}</span>
          </div>

          <div class="vehicle-bottom">
            <div class="vehicle-copy">
              <h3 class="vehicle-title">${escapeHtml(automovil.fabricante)}</h3>
              <p class="vehicle-subtitle">${escapeHtml(automovil.tipo)}</p>
            </div>

            <div class="detail-stack">
              <span class="meta-chip"><span class="meta-caption">Color</span>${escapeHtml(automovil.color)}</span>
              <span class="detail-pill"><span class="meta-caption">ID</span>#${escapeHtml(automovil.id)}</span>
            </div>

            <div class="card-footer-actions">
              <button type="button" class="card-action-btn card-action-edit" data-action="edit" data-id="${escapeHtml(automovil.id)}">Editar</button>
              <button type="button" class="card-action-btn" data-action="view" data-id="${escapeHtml(automovil.id)}">Ver detalle</button>
              <button type="button" class="card-action-btn card-action-delete" data-action="delete" data-id="${escapeHtml(automovil.id)}">Eliminar</button>
            </div>
          </div>
        </div>
      </article>`;
    }).join('');

    if ('IntersectionObserver' in window) {
      const revealItems = automovilesGrid.querySelectorAll('[data-reveal]:not(.is-revealed)');
      const observer = new IntersectionObserver((entries, currentObserver) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('is-revealed');
            currentObserver.unobserve(entry.target);
          }
        });
      }, {
        threshold: 0.15,
        rootMargin: '0px 0px -30px 0px'
      });

      revealItems.forEach((item) => observer.observe(item));
    } else {
      automovilesGrid.querySelectorAll('[data-reveal]').forEach((item) => item.classList.add('is-revealed'));
    }

    emptyState.classList.toggle('d-none', items.length > 0);
  };

  const resetValidationState = () => {
    automovilForm.querySelectorAll('.is-invalid').forEach((input) => input.classList.remove('is-invalid'));
    automovilForm.querySelectorAll('[data-valmsg-for]').forEach((node) => {
      node.textContent = '';
    });
  };

  const applyValidationErrors = (errors) => {
    if (!errors) {
      return;
    }

    Object.entries(errors).forEach(([key, messages]) => {
      const normalizedKey = key.replace('request.', '').replace('Request.', '');
      const input = automovilForm.querySelector(`[name="${normalizedKey.charAt(0).toLowerCase()}${normalizedKey.slice(1)}"]`);
      const feedback = automovilForm.querySelector(`[data-valmsg-for="${normalizedKey}"]`);

      if (input) {
        input.classList.add('is-invalid');
      }

      if (feedback) {
        feedback.textContent = Array.isArray(messages) ? messages.join(' ') : String(messages);
      }
    });
  };

  const openCreateModal = () => {
    state.editingId = null;
    clearFeedback(formFeedback);
    resetValidationState();
    automovilForm.reset();
    document.getElementById('automovilId').value = '';
    document.getElementById('anioInput').value = new Date().getFullYear();
    document.getElementById('automovilModalTitle').textContent = 'Crear automóvil';
    document.getElementById('automovilModalKicker').textContent = 'Nuevo registro';
    document.getElementById('saveAutomovilButtonLabel').textContent = 'Guardar automóvil';
    automovilModal?.show();
  };

  const openEditModal = async (id) => {
    setStatusText('Cargando detalle...');
    clearFeedback(formFeedback);
    resetValidationState();
    setButtonBusy(saveAutomovilButton, true);

    try {
      const response = await fetch(`${endpoints.base}/${id}`);
      const payload = await response.json();

      if (!response.ok) {
        throw new Error(payload.message || 'No fue posible cargar el detalle del automóvil.');
      }

      state.editingId = id;
      document.getElementById('automovilId').value = payload.id;
      document.getElementById('fabricanteInput').value = payload.fabricante;
      document.getElementById('tipoInput').value = payload.tipo;
      document.getElementById('colorInput').value = payload.color;
      document.getElementById('anioInput').value = payload.anio;
      document.getElementById('automovilModalTitle').textContent = 'Editar automóvil';
      document.getElementById('automovilModalKicker').textContent = 'Actualización en línea';
      document.getElementById('saveAutomovilButtonLabel').textContent = 'Guardar cambios';
      automovilModal?.show();
    } catch (error) {
      showToast(error.message || 'No fue posible cargar el automóvil.');
    } finally {
      setButtonBusy(saveAutomovilButton, false);
      setStatusText('Sincronizado');
    }
  };

  const openDeleteModal = (id) => {
    const automovil = state.items.find((item) => Number(item.id) === Number(id));
    if (!automovil) {
      return;
    }

    state.deletingId = Number(id);
    deleteSummary.innerHTML = `
      <strong>${escapeHtml(automovil.fabricante)} ${escapeHtml(automovil.tipo)}</strong><br />
      <span>Color: ${escapeHtml(automovil.color)} · Año: ${escapeHtml(automovil.anio)} · ID: ${escapeHtml(automovil.id)}</span>`;
    deleteModal?.show();
  };

  const viewDetail = (id) => {
    const automovil = state.items.find((item) => Number(item.id) === Number(id));
    if (!automovil) {
      return;
    }

    showToast(`${automovil.fabricante} ${automovil.tipo} · ${automovil.color} · año ${automovil.anio}`);
  };

  const fetchListado = async ({ silent = false } = {}) => {
    clearFeedback(filtersFeedback);
    if (!silent) {
      cardsLoadingState.classList.remove('d-none');
      automovilesGrid.innerHTML = '';
      emptyState.classList.add('d-none');
    }

    setStatusText('Sincronizando...');
    setButtonBusy(filtersForm.querySelector('button[type="submit"]'), true);

    try {
      const response = await fetch(`${endpoints.listado}?${collectFilters().toString()}`);
      const payload = await response.json();

      if (!response.ok) {
        const errorMessage = payload?.title || payload?.message || 'No fue posible consultar el inventario.';
        throw new Error(errorMessage);
      }

      state.items = payload.items ?? [];
      renderCards(state.items);
      renderMetrics(payload.stats, payload.total ?? state.items.length);
      setStatusText('Sincronizado');
    } catch (error) {
      state.items = [];
      renderCards([]);
      renderMetrics({}, 0);
      showFeedback(filtersFeedback, error.message || 'No fue posible cargar los automóviles.', true);
      setStatusText('Con incidencia');
    } finally {
      cardsLoadingState.classList.add('d-none');
      setButtonBusy(filtersForm.querySelector('button[type="submit"]'), false);
    }
  };

  const submitForm = async () => {
    clearFeedback(formFeedback);
    resetValidationState();
    setButtonBusy(saveAutomovilButton, true);

    const payload = {
      fabricante: document.getElementById('fabricanteInput').value.trim(),
      tipo: document.getElementById('tipoInput').value.trim(),
      color: document.getElementById('colorInput').value.trim(),
      anio: Number(document.getElementById('anioInput').value)
    };

    const isEditing = Number.isInteger(state.editingId) && state.editingId > 0;
    const url = isEditing ? `${endpoints.base}/${state.editingId}` : endpoints.base;
    const method = isEditing ? 'PUT' : 'POST';

    try {
      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          RequestVerificationToken: token
        },
        body: JSON.stringify(payload)
      });

      const raw = await response.text();
      const result = raw ? JSON.parse(raw) : {};

      if (!response.ok) {
        if (result?.errors) {
          applyValidationErrors(result.errors);
          showFeedback(formFeedback, 'Revisa los campos marcados antes de continuar.', true);
          return;
        }

        throw new Error(result?.message || result?.title || 'No fue posible guardar el automóvil.');
      }

      automovilModal?.hide();
      showToast(result.message || 'Operación completada correctamente.');
      await fetchListado({ silent: true });
    } catch (error) {
      showFeedback(formFeedback, error.message || 'No fue posible guardar el automóvil.', true);
    } finally {
      setButtonBusy(saveAutomovilButton, false);
    }
  };

  const deleteAutomovil = async () => {
    if (!state.deletingId) {
      return;
    }

    setButtonBusy(confirmDeleteButton, true);

    try {
      const response = await fetch(`${endpoints.base}/${state.deletingId}`, {
        method: 'DELETE',
        headers: {
          RequestVerificationToken: token
        }
      });

      const raw = await response.text();
      const result = raw ? JSON.parse(raw) : {};

      if (!response.ok) {
        throw new Error(result?.message || result?.title || 'No fue posible eliminar el automóvil.');
      }

      deleteModal?.hide();
      showToast(result.message || 'Automóvil eliminado correctamente.');
      state.deletingId = null;
      await fetchListado({ silent: true });
    } catch (error) {
      showToast(error.message || 'No fue posible eliminar el automóvil.');
    } finally {
      setButtonBusy(confirmDeleteButton, false);
    }
  };

  filtersForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    await fetchListado();
  });

  resetFiltersButton.addEventListener('click', async () => {
    filtersForm.reset();
    document.getElementById('orden').value = 'desc';
    document.getElementById('campoBusqueda').value = 'todos';
    clearFeedback(filtersFeedback);
    await fetchListado();
  });

  refreshDataButton.addEventListener('click', async () => {
    await fetchListado();
  });

  openCreateModalButton.addEventListener('click', openCreateModal);
  emptyStateCreateButton.addEventListener('click', openCreateModal);
  saveAutomovilButton.addEventListener('click', submitForm);
  confirmDeleteButton.addEventListener('click', deleteAutomovil);

  automovilesGrid.addEventListener('click', async (event) => {
    const button = event.target.closest('[data-action]');
    if (!button) {
      return;
    }

    const id = Number(button.dataset.id);
    switch (button.dataset.action) {
      case 'edit':
        await openEditModal(id);
        break;
      case 'delete':
        openDeleteModal(id);
        break;
      case 'view':
        viewDetail(id);
        break;
      default:
        break;
    }
  });

  fetchListado();
})();
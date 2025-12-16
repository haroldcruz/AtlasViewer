// Sistema de persistencia automática de formularios
(function() {
    'use strict';

    const STORAGE_PREFIX = 'formData_';
    const STORAGE_DURATION = 3600000; // 1 hora en milisegundos

    // Obtener clave única para el formulario basada en la URL
    function getFormStorageKey(form) {
        const formId = form.id || 'default';
        const path = window.location.pathname;
        return STORAGE_PREFIX + path + '_' + formId;
    }

    // Guardar datos del formulario
    function saveFormData(form) {
        const key = getFormStorageKey(form);
        const formData = {};
        const elements = form.elements;

        for (let i = 0; i < elements.length; i++) {
            const element = elements[i];
            const name = element.name;
            const type = element.type;

            if (!name || element.disabled) continue;

            // Guardar según el tipo de elemento
            if (type === 'checkbox') {
                formData[name] = element.checked;
            } else if (type === 'radio') {
                if (element.checked) {
                    formData[name] = element.value;
                }
            } else if (type === 'select-multiple') {
                const selected = [];
                for (let j = 0; j < element.options.length; j++) {
                    if (element.options[j].selected) {
                        selected.push(element.options[j].value);
                    }
                }
                formData[name] = selected;
            } else if (type !== 'submit' && type !== 'button' && type !== 'file') {
                formData[name] = element.value;
            }
        }

        // Guardar con timestamp
        const dataToStore = {
            data: formData,
            timestamp: Date.now()
        };

        try {
            sessionStorage.setItem(key, JSON.stringify(dataToStore));
        } catch (e) {
            console.warn('No se pudo guardar los datos del formulario:', e);
        }
    }

    // Restaurar datos del formulario
    function restoreFormData(form) {
        const key = getFormStorageKey(form);
        
        try {
            const stored = sessionStorage.getItem(key);
            if (!stored) return false;

            const { data, timestamp } = JSON.parse(stored);

            // Verificar si los datos no han expirado
            if (Date.now() - timestamp > STORAGE_DURATION) {
                sessionStorage.removeItem(key);
                return false;
            }

            const elements = form.elements;
            let restored = false;

            for (let i = 0; i < elements.length; i++) {
                const element = elements[i];
                const name = element.name;
                const type = element.type;

                if (!name || !(name in data)) continue;

                if (type === 'checkbox') {
                    element.checked = data[name];
                    restored = true;
                } else if (type === 'radio') {
                    if (element.value === data[name]) {
                        element.checked = true;
                        restored = true;
                    }
                } else if (type === 'select-multiple') {
                    const values = data[name];
                    for (let j = 0; j < element.options.length; j++) {
                        element.options[j].selected = values.includes(element.options[j].value);
                    }
                    restored = true;
                } else if (type !== 'submit' && type !== 'button' && type !== 'file') {
                    element.value = data[name] || '';
                    restored = true;
                }
            }

            // Disparar evento change para actualizar UI si es necesario
            if (restored) {
                const event = new Event('formRestored', { bubbles: true });
                form.dispatchEvent(event);
            }

            return restored;

        } catch (e) {
            console.warn('No se pudo restaurar los datos del formulario:', e);
            return false;
        }
    }

    // Limpiar datos guardados del formulario
    function clearFormData(form) {
        const key = getFormStorageKey(form);
        sessionStorage.removeItem(key);
    }

    // Limpiar datos expirados
    function cleanupExpiredData() {
        const keysToRemove = [];
        
        for (let i = 0; i < sessionStorage.length; i++) {
            const key = sessionStorage.key(i);
            if (key && key.startsWith(STORAGE_PREFIX)) {
                try {
                    const stored = sessionStorage.getItem(key);
                    const { timestamp } = JSON.parse(stored);
                    if (Date.now() - timestamp > STORAGE_DURATION) {
                        keysToRemove.push(key);
                    }
                } catch (e) {
                    keysToRemove.push(key);
                }
            }
        }

        keysToRemove.forEach(key => sessionStorage.removeItem(key));
    }

    // Inicializar persistencia para un formulario
    function initFormPersistence(form) {
        // Restaurar datos al cargar
        const restored = restoreFormData(form);
        
        if (restored) {
            // Mostrar notificación discreta
            const notification = document.createElement('div');
            notification.className = 'alert alert-info alert-dismissible fade show';
            notification.style.cssText = 'position: fixed; top: 80px; right: 20px; z-index: 9999; max-width: 300px;';
            notification.innerHTML = `
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                <small><i class="bi bi-arrow-counterclockwise"></i> Datos restaurados del último borrador</small>
            `;
            document.body.appendChild(notification);
            setTimeout(() => notification.remove(), 5000);
        }

        // Guardar automáticamente en cada cambio (con debounce)
        let saveTimeout;
        form.addEventListener('input', function() {
            clearTimeout(saveTimeout);
            saveTimeout = setTimeout(() => saveFormData(form), 500);
        });

        form.addEventListener('change', function() {
            saveFormData(form);
        });

        // Limpiar al enviar con éxito
        form.addEventListener('submit', function(e) {
            // Marcar que se está enviando
            form.dataset.submitting = 'true';
            
            // Limpiar después de un breve delay (dar tiempo al servidor)
            setTimeout(() => {
                if (form.dataset.submitting === 'true') {
                    clearFormData(form);
                }
            }, 1000);
        });

        // Opción manual de limpiar
        const clearButton = document.createElement('button');
        clearButton.type = 'button';
        clearButton.className = 'btn btn-sm btn-link text-muted';
        clearButton.innerHTML = '<i class="bi bi-trash"></i> Limpiar borrador';
        clearButton.style.cssText = 'position: absolute; top: 10px; right: 10px; font-size: 0.75rem;';
        clearButton.addEventListener('click', function() {
            if (confirm('¿Desea limpiar el borrador guardado?')) {
                clearFormData(form);
                form.reset();
                this.remove();
            }
        });

        // Agregar botón si hay datos restaurados
        if (restored && form.style.position !== 'relative') {
            form.style.position = 'relative';
            form.insertBefore(clearButton, form.firstChild);
        }
    }

    // Auto-inicializar todos los formularios al cargar
    document.addEventListener('DOMContentLoaded', function() {
        cleanupExpiredData();
        
        const forms = document.querySelectorAll('form[method="post"]');
        forms.forEach(function(form) {
            // Solo aplicar a formularios de creación/edición, no a búsquedas
            if (!form.classList.contains('form-search') && 
                !form.classList.contains('no-persistence')) {
                initFormPersistence(form);
            }
        });
    });

    // API pública
    window.FormPersistence = {
        save: saveFormData,
        restore: restoreFormData,
        clear: clearFormData,
        init: initFormPersistence
    };

})();

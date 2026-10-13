// Confirmaciones con SweetAlert2 (mismo patrón que TechStore).
// Los formularios con class="swal-save-form" o "swal-delete-form" piden confirmación antes de enviarse.
// El texto se puede personalizar con data-swal-title y data-swal-text.
$(function () {
    function confirmar(form, opciones) {
        // Si el formulario no supera la validación del cliente, no se pregunta nada.
        if ($.fn.valid && !$(form).valid()) {
            return;
        }
        // Sin internet el CDN de SweetAlert2 no carga: se usa el confirm nativo.
        if (!window.Swal) {
            if (window.confirm(form.dataset.swalTitle || opciones.title)) {
                form.submit();
            }
            return;
        }
        Swal.fire($.extend({
            showCancelButton: true,
            cancelButtonText: 'Cancelar',
            reverseButtons: true
        }, opciones, {
            title: form.dataset.swalTitle || opciones.title,
            text: form.dataset.swalText || opciones.text
        })).then(function (result) {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    }

    $(document).on('submit', 'form.swal-delete-form', function (e) {
        e.preventDefault();
        confirmar(this, {
            title: '¿Eliminar registro?',
            text: 'Esta acción no se puede deshacer.',
            icon: 'warning',
            confirmButtonColor: '#c4513d',
            confirmButtonText: 'Sí, eliminar'
        });
    });

    $(document).on('submit', 'form.swal-save-form', function (e) {
        e.preventDefault();
        confirmar(this, {
            title: '¿Guardar cambios?',
            text: 'Se actualizará la información en la base de datos.',
            icon: 'question',
            confirmButtonColor: '#13233f',
            confirmButtonText: 'Sí, guardar'
        });
    });
});

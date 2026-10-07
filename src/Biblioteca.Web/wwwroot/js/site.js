// JavaScript propio de la Web. Se usa JavaScript "normal" (sin jQuery) para que sea fácil de leer.

// --- Menú lateral: el botón hamburguesa lo muestra u oculta -------------------
document.addEventListener("DOMContentLoaded", function () {
    const menu = document.getElementById("menu-lateral");
    const boton = document.getElementById("boton-menu");

    function colapsarMenu(colapsar) {
        menu.classList.toggle("colapsado", colapsar);
        boton.setAttribute("aria-expanded", colapsar ? "false" : "true");
    }

    // En pantallas pequeñas el menú empieza oculto para no tapar el contenido.
    // 768px es el punto de corte "md" de Bootstrap.
    if (window.innerWidth < 768) {
        colapsarMenu(true);
    }

    boton.addEventListener("click", function () {
        colapsarMenu(!menu.classList.contains("colapsado"));
    });
});

// --- Mensajes de éxito y error (ver el div "mensajes" en _Layout.cshtml) ----------
document.addEventListener("DOMContentLoaded", function () {
    const mensajes = document.getElementById("mensajes");
    const exito = mensajes.dataset.exito;
    const error = mensajes.dataset.error;

    // Se usa "text" y no "html" para que SweetAlert2 no interprete etiquetas del mensaje.
    if (exito) {
        Swal.fire({ icon: "success", title: "Listo", text: exito, timer: 2500, showConfirmButton: false });
    } else if (error) {
        Swal.fire({ icon: "error", title: "No se pudo completar", text: error });
    }
});

// --- Confirmación antes de eliminar (RW-07) -----------------------------------
// Los formularios con la clase "form-eliminar" no se envían directamente: primero se pregunta
// con SweetAlert2 y solo se envían si el usuario confirma.
document.addEventListener("DOMContentLoaded", function () {
    const formularios = document.querySelectorAll("form.form-eliminar");

    formularios.forEach(function (formulario) {
        formulario.addEventListener("submit", function (evento) {
            // Se detiene el envío mientras el usuario decide.
            evento.preventDefault();

            Swal.fire({
                icon: "warning",
                title: "¿Eliminar este libro?",
                text: "Se eliminará \"" + formulario.dataset.titulo + "\". Esta acción no se puede deshacer.",
                showCancelButton: true,
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar",
                confirmButtonColor: "#dc3545",
                focusCancel: true // Si se pulsa Enter sin pensar, se cancela.
            }).then(function (resultado) {
                if (resultado.isConfirmed) {
                    // submit() no vuelve a lanzar el evento "submit", así que no se pregunta dos veces.
                    formulario.submit();
                }
            });
        });
    });
});

// --- Modal de crear y editar (RW-09) ------------------------------------------
// Los botones con data-url-formulario piden ese formulario al servidor y lo muestran en el
// modal de _ModalFormulario.cshtml. El formulario se envía con fetch para que, si hay errores,
// se vean dentro del modal sin cerrarlo. Qué significa cada respuesta: ver LibrosController.
document.addEventListener("DOMContentLoaded", function () {
    const modalElemento = document.getElementById("modal-formulario");
    if (!modalElemento) {
        return; // Esta página no tiene modal.
    }
    const contenido = document.getElementById("modal-formulario-contenido");
    const modal = bootstrap.Modal.getOrCreateInstance(modalElemento);

    function mostrarFormulario(html) {
        contenido.innerHTML = html;
        // La validación en el navegador solo conoce los formularios que había al cargar la
        // página: hay que avisarle del nuevo. (jQuery Validation es la única parte con jQuery.)
        $.validator.unobtrusive.parse(contenido.querySelector("form"));
    }

    function avisarErrorInesperado() {
        Swal.fire({ icon: "error", title: "Ocurrió un error inesperado", text: "Inténtalo de nuevo en un momento." });
    }

    async function abrirFormulario(url) {
        try {
            const respuesta = await fetch(url);
            if (respuesta.ok) {
                mostrarFormulario(await respuesta.text());
                modal.show();
            } else {
                // 404 o 503: el controller dejó el mensaje en TempData y al recargar se ve.
                window.location.reload();
            }
        } catch {
            avisarErrorInesperado(); // La propia Web no respondió.
        }
    }

    document.querySelectorAll("[data-url-formulario]").forEach(function (boton) {
        boton.addEventListener("click", function () {
            abrirFormulario(boton.dataset.urlFormulario);
        });
    });

    // Se escucha el "submit" en el contenido del modal (y no en el formulario) porque el
    // formulario se reemplaza cada vez que llega uno nuevo del servidor.
    contenido.addEventListener("submit", async function (evento) {
        evento.preventDefault(); // Se envía con fetch, no recargando la página.
        const formulario = evento.target;
        if (!$(formulario).valid()) {
            return; // Hay errores simples: jQuery Validation ya los está mostrando.
        }

        const botonGuardar = formulario.querySelector("button[type=submit]");
        botonGuardar.disabled = true; // Evita guardar dos veces con un doble clic.

        try {
            // FormData incluye todos los campos, también el token antiforgery.
            const respuesta = await fetch(formulario.action, { method: "POST", body: new FormData(formulario) });

            if (respuesta.status === 400) {
                mostrarFormulario(await respuesta.text()); // Formulario con errores.
            } else if (respuesta.ok || respuesta.status === 404 || respuesta.status === 503) {
                window.location.reload(); // Guardado, o libro borrado: el mensaje va en TempData.
            } else {
                botonGuardar.disabled = false;
                avisarErrorInesperado();
            }
        } catch {
            botonGuardar.disabled = false;
            avisarErrorInesperado();
        }
    });

    // El card "Nuevo libro" de la portada lleva a /Libros#nuevo: se abre el modal al cargar.
    // Se quita "#nuevo" de la dirección para que al recargar (tras guardar) no se abra otra vez.
    if (window.location.hash === "#nuevo") {
        history.replaceState(null, "", window.location.pathname);
        const botonNuevo = document.getElementById("boton-nuevo-libro");
        if (botonNuevo) {
            botonNuevo.click();
        }
    }
});

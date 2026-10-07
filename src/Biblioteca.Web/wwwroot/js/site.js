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

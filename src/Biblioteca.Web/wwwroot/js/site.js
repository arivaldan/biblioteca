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

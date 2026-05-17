document.querySelectorAll('.folder-card')
.forEach(card => {

    card.addEventListener('click', () => {

        card.classList.toggle('active');

    });

});
const modalOverlay =
    document.getElementById('modalOverlay');
const openModal =
    document.getElementById('openModal');
const closeModal =
    document.getElementById('closeModal');
const cancelModal =
    document.getElementById('cancelModal');
openModal.addEventListener('click', () => {
    modalOverlay.style.display = 'flex';
});
closeModal.addEventListener('click', () => {
    modalOverlay.style.display = 'none';
});
cancelModal.addEventListener('click', () => {
    modalOverlay.style.display = 'none';
});
modalOverlay.addEventListener('click', (e) => {
    if (e.target === modalOverlay) {
        modalOverlay.style.display = 'none';
    }
});

// Buscador
const buscador = document.querySelector('.search-box input');

buscador.addEventListener('input', function () {
    const texto = this.value.toLowerCase().trim();
    const tarjetas = document.querySelectorAll('.folder-card');

    // Quitar mensaje inmediatamente
    const noResultados = document.getElementById('no-resultados');
    if (noResultados) noResultados.remove();

    tarjetas.forEach(function (tarjeta) {
        const nombre = tarjeta.querySelector('h3').textContent.toLowerCase();
        if (nombre.includes(texto)) {
            tarjeta.classList.remove('oculto');
            tarjeta.style.display = '';
        } else {
            tarjeta.classList.add('oculto');
            setTimeout(function () {
                if (tarjeta.classList.contains('oculto')) {
                    tarjeta.style.display = 'none';
                }
            }, 300);
        }
    });

    // Esperar animación y verificar UNA sola vez
    clearTimeout(window.buscarTimeout);
    window.buscarTimeout = setTimeout(function () {
        // Verificar que no exista ya el mensaje
        if (document.getElementById('no-resultados')) return;

        const emptyState = document.querySelector('.empty-state');
        if (emptyState) return;

        let hayVisibles = [...document.querySelectorAll('.folder-card')]
            .some(t => t.style.display !== 'none');

        if (!hayVisibles) {
            const nuevo = document.createElement('div');
            nuevo.id = 'no-resultados';
            nuevo.className = 'empty-state';
            nuevo.innerHTML = '<div style="display:flex; align-items:center; gap:10px;"><i class="fa-solid fa-magnifying-glass"></i><div><h2>Sin resultados</h2><p>No se encontraron carpetas con ese nombre</p></div></div>';
            document.querySelector('.folders-grid').appendChild(nuevo);
        }
    }, 350);
});
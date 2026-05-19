// ── Toggle mostrar/ocultar contraseñas ──────────────────────────────
document.getElementById('toggleContraseña').addEventListener('click', function () {
    const input = document.getElementById('inputContraseña');
    const esPassword = input.type === 'password';
    input.type = esPassword ? 'text' : 'password';
    this.textContent = esPassword ? 'Ocultar' : 'Mostrar';
});

document.getElementById('toggleConfirmar').addEventListener('click', function () {
    const input = document.getElementById('inputConfirmar');
    const esPassword = input.type === 'password';
    input.type = esPassword ? 'text' : 'password';
    this.textContent = esPassword ? 'Ocultar' : 'Mostrar';
});

document.querySelectorAll('input').forEach(function (inp) {
    inp.addEventListener('input', function () {
        const span = this.closest('.input-group').querySelector('span[data-valmsg-for]');
        if (span) span.textContent = '';
    });
});

// ── Medidor de fuerza de contraseña ─────────────────────────────────
const inputContraseña = document.getElementById('inputContraseña');
const barraFuerza = document.getElementById('barraFuerza');
const textoFuerza = document.getElementById('textoFuerza');

inputContraseña.addEventListener('input', function () {
    const valor = this.value;
    const resultado = calcularFuerza(valor);

    // Actualiza las barras
    const segmentos = barraFuerza.querySelectorAll('.segmento');
    segmentos.forEach((seg, i) => {
        seg.className = 'segmento'; // reset
        if (i < resultado.nivel) {
            seg.classList.add(resultado.clase);
        }
    });

    // Actualiza el texto
    textoFuerza.textContent = valor.length === 0 ? '' : resultado.texto;
    textoFuerza.className = 'texto-fuerza ' + (valor.length === 0 ? '' : resultado.clase);
});

function calcularFuerza(password) {
    if (password.length === 0) return { nivel: 0, clase: '', texto: '' };

    let puntos = 0;

    if (password.length >= 8) puntos++;   // longitud mínima
    if (password.length >= 12) puntos++;   // longitud buena
    if (/[A-Z]/.test(password)) puntos++;  // mayúscula
    if (/[0-9]/.test(password)) puntos++;  // número
    if (/[!@#$%^&*()\[\]{}_+=<>?,.:`~|]/.test(password)) puntos++; // especial permitido

    if (puntos <= 2) return { nivel: 1, clase: 'debil', texto: 'Débil' };
    if (puntos <= 3) return { nivel: 2, clase: 'media', texto: 'Media' };
    if (puntos <= 4) return { nivel: 3, clase: 'fuerte', texto: 'Fuerte' };
    return { nivel: 3, clase: 'fuerte', texto: 'Fuerte' };
}

// ── Mantener el medidor al volver de un submit fallido ──────────────
window.addEventListener('DOMContentLoaded', function () {
    const input = document.getElementById('inputContraseña');
    if (input && input.value.length > 0) {
        input.dispatchEvent(new Event('input'));
    }
});

// ── Bloqueo de caracteres en tiempo real ────────────────────────────

// Solo letras y espacios para el nombre
document.querySelector('input[name="nombre"]').addEventListener('keypress', function (e) {
    const permitidos = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]$/;
    if (!permitidos.test(e.key)) {
        e.preventDefault();
    }
});

// Bloquear caracteres peligrosos SQL en correo
document.querySelector('input[name="correo"]').addEventListener('keypress', function (e) {
    const bloqueados = /['";\-\/\*\\]/;
    if (bloqueados.test(e.key)) {
        e.preventDefault();
    }
});

// Bloquear caracteres peligrosos SQL en contraseña (pero permitir especiales seguros)
document.getElementById('inputContraseña').addEventListener('keypress', function (e) {
    const bloqueados = /['";\-\/\*\\]/;
    if (bloqueados.test(e.key)) {
        e.preventDefault();
    }
});

// También bloquear si alguien pega texto con caracteres prohibidos
['input[name="nombre"]', 'input[name="correo"]', '#inputContraseña', '#inputConfirmar'].forEach(selector => {
    document.querySelector(selector)?.addEventListener('paste', function (e) {
        const textoPegado = (e.clipboardData || window.clipboardData).getData('text');
        const patronesSql = ['--', ';--', '/*', '*/', 'xp_', 'DROP', 'SELECT', 'INSERT', 'DELETE', 'UPDATE', 'EXEC', 'UNION'];
        const tienePeligroso = patronesSql.some(p => textoPegado.toUpperCase().includes(p.toUpperCase()));

        if (tienePeligroso) {
            e.preventDefault();
            alert('Se detectó contenido no permitido en el texto pegado.');
        }
    });
});
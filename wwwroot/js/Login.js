// Quitar error al escribir en cualquier campo
document.querySelectorAll('input').forEach(function (input) {
    input.addEventListener('input', function () {
        const errorDiv = document.querySelector('[style*="c0392b"]');
        if (errorDiv) errorDiv.remove();
    });
});

// toggle contraseña
const toggle = document.getElementById('toggleContraseña');
const input = document.getElementById('inputContraseña');

toggle.addEventListener('click', function () {
    if (input.type === 'password') {
        input.type = 'text';
        toggle.textContent = 'Ocultar';
    } else {
        input.type = 'password';
        toggle.textContent = 'Mostrar';
    }
});

// ── Bloqueo de caracteres en tiempo real ────────────────────────────

// Bloquear caracteres peligrosos en el correo
document.querySelector('input[name="correo"]').addEventListener('keypress', function (e) {
    const bloqueados = /['";\-\/\*\\]/;
    if (bloqueados.test(e.key)) {
        e.preventDefault();
    }
});

// Bloquear caracteres peligrosos en la contraseña
document.getElementById('inputContraseña').addEventListener('keypress', function (e) {
    const bloqueados = /['";\-\/\*\\]/;
    if (bloqueados.test(e.key)) {
        e.preventDefault();
    }
});

// ── Bloquear pegado con contenido peligroso ─────────────────────────
['input[name="correo"]', '#inputContraseña'].forEach(selector => {
    document.querySelector(selector)?.addEventListener('paste', function (e) {
        const textoPegado = (e.clipboardData || window.clipboardData).getData('text');
        const patronesSql = ['--', ';--', '/*', '*/', 'xp_', 'DROP', 'SELECT',
                             'INSERT', 'DELETE', 'UPDATE', 'EXEC', 'UNION'];

        const tienePeligroso = patronesSql.some(p =>
            textoPegado.toUpperCase().includes(p.toUpperCase())
        );

        if (tienePeligroso) {
            e.preventDefault();
            alert('Se detectó contenido no permitido en el texto pegado.');
        }
    });
});
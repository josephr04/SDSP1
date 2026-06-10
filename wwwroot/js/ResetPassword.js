// Quitar error al escribir en cualquier campo
document.querySelectorAll('input').forEach(function (input) {
    input.addEventListener('input', function () {
        const errorDiv = document.querySelector('[style*="c0392b"]');
        if (errorDiv) errorDiv.remove();
    });
});

// Toggle contraseña
const togglePassword = document.getElementById('toggleContraseña');
const inputPassword = document.getElementById('inputContraseña');

if (togglePassword && inputPassword) {
    togglePassword.addEventListener('click', function () {
        if (inputPassword.type === 'password') {
            inputPassword.type = 'text';
            togglePassword.textContent = 'Ocultar';
        } else {
            inputPassword.type = 'password';
            togglePassword.textContent = 'Mostrar';
        }
    });
}

// Toggle confirmar contraseña
const toggleConfirm = document.getElementById('toggleConfirmar');
const inputConfirm = document.getElementById('inputConfirmar');

if (toggleConfirm && inputConfirm) {
    toggleConfirm.addEventListener('click', function () {
        if (inputConfirm.type === 'password') {
            inputConfirm.type = 'text';
            toggleConfirm.textContent = 'Ocultar';
        } else {
            inputConfirm.type = 'password';
            toggleConfirm.textContent = 'Mostrar';
        }
    });
}

// ── Validación en tiempo real de coincidencia de contraseñas ────────

const passwordInput = document.getElementById('inputContraseña');
const confirmInput = document.getElementById('inputConfirmar');

if (passwordInput && confirmInput) {
    confirmInput.addEventListener('input', function () {
        if (confirmInput.value !== passwordInput.value && confirmInput.value !== '') {
            confirmInput.style.borderColor = '#c0392b';
            confirmInput.style.boxShadow = '0 0 8px rgba(192, 57, 43, 0.3)';
        } else {
            confirmInput.style.borderColor = '#e8f1f1';
            confirmInput.style.boxShadow = 'none';
        }
    });

    passwordInput.addEventListener('input', function () {
        if (confirmInput.value !== '') {
            if (confirmInput.value !== passwordInput.value) {
                confirmInput.style.borderColor = '#c0392b';
                confirmInput.style.boxShadow = '0 0 8px rgba(192, 57, 43, 0.3)';
            } else {
                confirmInput.style.borderColor = '#e8f1f1';
                confirmInput.style.boxShadow = 'none';
            }
        }
    });
}

// ── Bloqueo de caracteres peligrosos en tiempo real ──────────────────

// Bloquear caracteres peligrosos en la contraseña
[inputPassword, inputConfirm].forEach(input => {
    if (input) {
        input.addEventListener('keypress', function (e) {
            const bloqueados = /['";\-\/\*\\]/;
            if (bloqueados.test(e.key)) {
                e.preventDefault();
            }
        });

        // ── Bloquear pegado con contenido peligroso ────────────────────
        input.addEventListener('paste', function (e) {
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
    }
});

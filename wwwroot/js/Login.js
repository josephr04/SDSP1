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
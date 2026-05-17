const toggle = document.getElementById('toggleContraseña');
const input = document.getElementById('inputContraseña');
toggle.addEventListener('click', function () {
    if (input.type === 'password') { input.type = 'text'; toggle.textContent = 'Ocultar'; }
    else { input.type = 'password'; toggle.textContent = 'Mostrar'; }
});

const toggleConfirmar = document.getElementById('toggleConfirmar');
const inputConfirmar = document.getElementById('inputConfirmar');
toggleConfirmar.addEventListener('click', function () {
    if (inputConfirmar.type === 'password') { inputConfirmar.type = 'text'; toggleConfirmar.textContent = 'Ocultar'; }
    else { inputConfirmar.type = 'password'; toggleConfirmar.textContent = 'Mostrar'; }
});

document.querySelectorAll('input').forEach(function (inp) {
    inp.addEventListener('input', function () {
        const span = this.closest('.input-group').querySelector('span[data-valmsg-for]');
        if (span) span.textContent = '';
    });
});
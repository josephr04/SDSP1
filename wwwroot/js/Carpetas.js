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
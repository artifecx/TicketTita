var confirmationCallback;
var originalModalId;
function displayConfirmationModal(callback, message, originalModal) {
    confirmationCallback = callback;
    originalModalId = originalModal;
    document.getElementById('confirmationModalMessage').textContent = `${message}. Are you sure you want to proceed?`;
    $('#confirmationModal').modal('show');
}

$('#confirmationModal .btn-primary').on('click', function () {
    if (typeof confirmationCallback === 'function') {
        confirmationCallback();
    }
});

$('#confirmationModal').on('hidden.bs.modal', function () {
    $('#confirmationModal').modal('hide');
    $('#'+originalModalId).modal('show');
    confirmationCallback = null;
    originalModalId = null;
});

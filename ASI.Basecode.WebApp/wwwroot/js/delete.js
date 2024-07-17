var itemId;
function displayDeleteModal(id, name) {
    itemId = id;
    itemName = name;
    document.getElementById('deleteModalMessage').textContent = `Are you sure you want to delete ${itemName}?`;
    $('#deleteModal').modal('show');
}

$('#deleteModal').on('hidden.bs.modal', function () {
    toastr.info("Deletion cancelled, no changes were made");
});

$('#confirmDeleteBtn').on('click', function () {
    var deleteUrl = $('#deleteUrl').val();
    var baseUrl = $('#baseUrl').val();
    $.ajax({
        url: deleteUrl,
        type: 'POST',
        data: { id: itemId },
        success: function (response) {
            if (response.success) {
                window.location.href = baseUrl;
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        }
    });
});
$(document).ready(function () {
    var successMessage = $('#tempDataContainer').data('success');
    var errorMessage = $('#tempDataContainer').data('error');

    if (successMessage) {
        toastr.success(successMessage);
    } else if (errorMessage) {
        toastr.error(errorMessage);
    }
});

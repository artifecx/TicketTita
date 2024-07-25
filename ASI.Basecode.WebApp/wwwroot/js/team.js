$(document).ready(function () {
    var url = new URL(window.location);
    var showModal = url.searchParams.get('showModal');

    if (showModal === 'editTeam') {
        $('#editTeamModal').modal('show');
        url.searchParams.delete('showModal');
        window.history.replaceState(null, null, url);
    }
});

document.addEventListener('DOMContentLoaded', function () {
    var contentTextarea = document.getElementById('createTeamName');
    var remainingCharsSpan = document.getElementById('remainingNameChars');

    function updateRemainingChars() {
        var remaining = 100 - contentTextarea.value.length;
        remainingCharsSpan.textContent = remaining + ' characters remaining';
    }

    updateRemainingChars();

    contentTextarea.addEventListener('keyup', updateRemainingChars);
});

document.addEventListener('DOMContentLoaded', function () {
    var contentTextarea = document.getElementById('createTeamDescription');
    var remainingCharsSpan = document.getElementById('remainingDescriptionChars');

    function updateRemainingChars() {
        var remaining = 800 - contentTextarea.value.length;
        remainingCharsSpan.textContent = remaining + ' characters remaining';
    }

    updateRemainingChars();

    contentTextarea.addEventListener('keyup', updateRemainingChars);
});

document.addEventListener('DOMContentLoaded', function () {
    var contentTextarea = document.getElementById('editTeamName');
    var remainingCharsSpan = document.getElementById('remainingNameChars');

    function updateRemainingChars() {
        var remaining = 100 - contentTextarea.value.length;
        remainingCharsSpan.textContent = remaining + ' characters remaining';
    }

    updateRemainingChars();

    contentTextarea.addEventListener('keyup', updateRemainingChars);
});

document.addEventListener('DOMContentLoaded', function () {
    var contentTextarea = document.getElementById('editTeamDescription');
    var remainingCharsSpan = document.getElementById('remainingDescriptionChars');

    function updateRemainingChars() {
        var remaining = 800 - contentTextarea.value.length;
        remainingCharsSpan.textContent = remaining + ' characters remaining';
    }

    updateRemainingChars();

    contentTextarea.addEventListener('keyup', updateRemainingChars);
});

function displayAddModal(teamId) {
    $('#TeamId').val(teamId);
    $('#addAgentModal').modal('show');
}

$('#addAgentModal').on('hidden.bs.modal', function () {
    toastr.info("Add agent cancelled, no changes were made");
});

function submitAddAgent() {
    var form = $('#addAgentForm');

    form.validate();
    if (!form.valid()) {
        return;
    }

    var formData = form.serialize();

    $.ajax({
        url: '/Team/AssignAgent',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                location.reload();
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            toastr.error(errorMessage);
        }
    });
}

function displayReassignAgentModal(oldTeamId, agentId) {
    $('#oldTeamId').val(oldTeamId);
    $('#agentId').val(agentId);
    $('#reassignAgentModal').modal('show');
}

$('#reassignAgentModal').on('hidden.bs.modal', function () {
    toastr.info("Reassignment cancelled, no changes were made");
});

function submitReassignAgent() {
    var form = $('#reassignAgentForm');

    form.validate();
    if (!form.valid()) {
        return;
    }

    var formData = form.serialize();

    $.ajax({
        url: '/Team/ReassignAgent',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                location.reload();
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            toastr.error(errorMessage);
        }
    });
}

function displayCreateModal() {
    $('#createTeamModal').modal('show');
}

$('#createTeamModal').on('hidden.bs.modal', function () {
    toastr.info("Create cancelled, no changes were made");
});

function submitCreateTeam() {
    var form = $('#createTeamForm');

    form.validate();
    if (!form.valid()) {
        return;
    }

    var formData = form.serialize();

    $.ajax({
        url: '/Team/Create',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                location.reload();
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            toastr.error(errorMessage);
        }
    });
}

function displayEditModal(teamId, teamName, teamDescription) {
    $('#editTeamId').val(teamId);
    $('#editTeamName').val(teamName);
    $('#editTeamDescription').val(teamDescription);
    $('#editTeamModal').modal('show');
}

$('#editTeamModal').on('hidden.bs.modal', function () {
    toastr.info("Edit cancelled, no changes were made");
});

function submitEditTeam() {
    var form = $('#editTeamForm');

    form.validate();
    if (!form.valid()) {
        return;
    }

    var formData = form.serialize();

    $.ajax({
        url: '/Team/Update',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                location.reload();
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            toastr.error(errorMessage);
        }
    });
}

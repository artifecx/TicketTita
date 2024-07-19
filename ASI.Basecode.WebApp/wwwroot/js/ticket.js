$(document).ready(function () {
    var url = new URL(window.location);
    var showModal = url.searchParams.get('showModal');

    if (showModal === 'editTicket') {
        $('#editTicketModal').modal('show');
        url.searchParams.delete('showModal');
        window.history.replaceState(null, null, url);
    }
});

$('#createTicketModal').on('hidden.bs.modal', function () {
    toastr.info("Create cancelled, no changes were made");
});

function submitCreateTicket() {
    var form = $('#createTicketForm');

    form.validate();
    if (!form.valid()) {
        return;
    }

    var formData = new FormData(form[0]);

    $.ajax({
        url: '/Ticket/Create',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                $('#createTicketModal').modal('hide');
                location.reload();
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            toastr.error(errorMessage);
            $('#createTicketModal').modal('hide');
        }
    });
}

var attachmentRemoved = false;
document.getElementById('clearAttachment').addEventListener('click', function () {
    document.getElementById('attachmentDetails').style.display = 'none';
    document.getElementById('fileUpload').style.display = 'block';
    attachmentRemoved = true;
});

$('#editTicketModal').on('hidden.bs.modal', function () {
    $('#editTicketForm')[0].reset();
    document.getElementById('attachmentDetails').style.display = '@(Model.Attachment != null ? "block" : "none")';
    document.getElementById('fileUpload').style.display = '@(Model.Attachment == null ? "block" : "none")';

    document.querySelector('#attachmentDetails input').value = '@Model.Attachment?.Name';
    toastr.info("Edit cancelled, no changes were made");
});

function submitEditTicket() {
    var form = $('#editTicketForm');

    form.validate();
    if (!form.valid()) {
        return;
    }

    var formData = new FormData(form[0]);

    if (attachmentRemoved) {
        formData.delete('Attachment.AttachmentId');
    }

    $.ajax({
        url: '/Ticket/Edit',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                $('#editTicketModal').modal('hide');
                location.reload();
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            toastr.error(errorMessage);
            $('#editTicketModal').modal('hide');
        }
    });
}

var statusId;
function validateTracking() {
    statusId = $('#statusId').val();

    if (statusId === 'S3') {
        $('#updateTrackingModal').modal('hide');
        setTimeout(function () {
            var message = 'Setting this ticket as resolved is final and cannot be undone';
            displayConfirmationModal(saveTracking, message, 'updateTrackingModal');
        }, 250);
    }
    else {
        saveTracking();
    }
}

function reopenTicket() {
    statusId = 'S1';
    saveTracking();
}

function saveTracking() {
    var priorityId = $('#priorityId').val();
    var ticketId = $('#ticketId').val();

    $.ajax({
        url: '/Ticket/UpdateTracking',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            TicketId: ticketId,
            PriorityTypeId: priorityId,
            StatusTypeId: statusId
        }),
        success: function (response) {
            if (response.success) {
                statusId = null;
                $('#updateTrackingModal').modal('hide');
                location.reload();
            } else {
                statusId = null;
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            statusId = null;
            toastr.error(errorMessage);
            $('#updateTrackingModal').modal('hide');
        }
    });
}

$('#saveAssignmentBtn').click(function () {
    var teamId = $('#teamId').val();
    var agentId = $('#agentId').val();
    var ticketId = $('#ticketId').val();

    $.ajax({
        url: '/Ticket/UpdateAssignment',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            TicketId: ticketId,
            TeamId: teamId,
            AgentId: agentId,
        }),
        success: function (response) {
            if (response.success) {
                $('#updateAssignmentModal').modal('hide');
                location.reload();
            } else {
                var errorMessage = response.error || "An error occurred.";
                toastr.error(errorMessage);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error : "An unexpected error occurred.";
            toastr.error(errorMessage);
            $('#updateAssignmentModal').modal('hide');
        }
    });
});
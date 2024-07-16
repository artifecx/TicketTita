$(document).ready(function () {
    $("#new-comment-link").click(function (e) {
        e.preventDefault();
        $("#new-comment-section").show();
        $("#new-comment-link").hide();
    });

    $("#cancel-comment-btn").click(function () {
        $("#new-comment-section").hide();
        $("#new-comment-link").show();
    });

    $(document).on("click", ".edit-comment-link", function () {
        var id = $(this).data("id");
        $("#edit-comment-" + id).show();
    });

    $(document).on("click", ".cancel-edit", function () {
        $(this).closest(".edit-comment-form").hide();
    });

    $(document).on("click", ".reply-comment-link", function () {
        var id = $(this).data("id");
        $("#reply-comment-" + id).show();
    });

    $(document).on("click", ".cancel-reply", function () {
        $(this).closest(".reply-comment-form").hide();
    });

    $(document).on("click", ".save-edit", function () {
        var id = $(this).data("id");
        var content = $("#edit-comment-" + id).find("textarea").val();

        $.ajax({
            url: editCommentUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Content: content,
                CommentId: id,
                TicketId: ticketId,
            }),
            success: function (response) {
                location.reload();
            },
            error: function () {
                location.reload();
            }
        });
    });

    $(document).on("click", ".save-reply", function () {
        var id = $(this).data("id");
        var content = $("#reply-comment-" + id).find("textarea").val();

        $.ajax({
            url: addCommentUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Content: content,
                ParentId: id,
                TicketId: ticketId
            }),
            success: function (response) {
                location.reload();
            },
            error: function () {
                location.reload();
            }
        });
    });

    $("#post-comment-btn").click(function () {
        var content = $("#new-comment-content").val();

        $.ajax({
            url: addCommentUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Content: content,
                TicketId: ticketId
            }),
            success: function (response) {
                location.reload();
            },
            error: function () {
                location.reload();
            }
        });
    });

    var commentId;
    $(document).on("click", ".delete-comment-link", function (e) {
        e.preventDefault();
        commentId = $(this).data("id");
        $("#deleteModalMessage").text("Are you sure you want to delete this comment?");
        $("#deleteModal").modal("show");
    });

    $("#confirmDeleteBtn").click(function () {
        $.ajax({
            url: deleteCommentUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                Content: "delete",
                CommentId: commentId,
                TicketId: ticketId
            }),
            success: function (response) {
                location.reload();
            },
            error: function () {
                location.reload();
            }
        });
    });
});

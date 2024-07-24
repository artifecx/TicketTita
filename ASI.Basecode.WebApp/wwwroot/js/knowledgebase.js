document.addEventListener('DOMContentLoaded', function () {
    setupContentCharsCounter();
    setupEditModal();
});

function setupContentCharsCounter() {
    var contentTextarea = document.getElementById('content');
    var remainingContentCharsSpan = document.getElementById('remainingContentChars');
    var titleInput = document.getElementById('title');
    var remainingTitleCharsSpan = document.getElementById('remainingTitleChars');

    function updateRemainingContentChars() {
        var remaining = 800 - contentTextarea.value.length;
        remainingContentCharsSpan.textContent = remaining + ' characters remaining';
    }

    function updateRemainingTitleChars() {
        var remaining = 100 - titleInput.value.length;
        remainingTitleCharsSpan.textContent = remaining + ' characters remaining';
    }

    if (contentTextarea && remainingContentCharsSpan) {
        updateRemainingContentChars();
        contentTextarea.addEventListener('keyup', updateRemainingContentChars);
    } else {
        console.error('Content elements not found:', contentTextarea, remainingContentCharsSpan);
    }

    if (titleInput && remainingTitleCharsSpan) {
        updateRemainingTitleChars();
        titleInput.addEventListener('keyup', updateRemainingTitleChars);
    } else {
        console.error('Title elements not found:', titleInput, remainingTitleCharsSpan);
    }
}

function setupEditModal() {
    var contentTextarea = document.getElementById('editContent');
    var remainingContentCharsSpan = document.getElementById('editRemainingContentChars');
    var titleInput = document.getElementById('editTitle');
    var remainingTitleCharsSpan = document.getElementById('editRemainingTitleChars');

    function editUpdateRemainingContentChars() {
        var remaining = 800 - contentTextarea.value.length;
        remainingContentCharsSpan.textContent = remaining + ' characters remaining';
    }

    function editUpdateRemainingTitleChars() {
        var remaining = 100 - titleInput.value.length;
        remainingTitleCharsSpan.textContent = remaining + ' characters remaining';
    }

    if (contentTextarea && remainingContentCharsSpan) {
        editUpdateRemainingContentChars();
        contentTextarea.addEventListener('keyup', editUpdateRemainingContentChars);
    }

    if (titleInput && remainingTitleCharsSpan) {
        editUpdateRemainingTitleChars();
        titleInput.addEventListener('keyup', editUpdateRemainingTitleChars);
    }

    $('#editArticleModal').on('shown.bs.modal', function () {
        editUpdateRemainingContentChars();
        editUpdateRemainingTitleChars();
    });
}
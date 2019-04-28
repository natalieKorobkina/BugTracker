$(function () {
    $(".actionArchive").on('click', function (e) {
        e.preventDefault();
        var result = confirm("Are you sure you want to arcive this project? All project's tickets will be archived too.");
        if (result) {
            $(this).closest('form').submit();
        }
    });
});
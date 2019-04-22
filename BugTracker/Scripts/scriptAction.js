$(function () {
    $(".actionDelete").on('click', function (e) {
        e.preventDefault();
        var result = confirm("Are you sure you want to delete? This action is not reversible.");
        if (result) {
            $(this).closest('form').submit();
        }
    });
});
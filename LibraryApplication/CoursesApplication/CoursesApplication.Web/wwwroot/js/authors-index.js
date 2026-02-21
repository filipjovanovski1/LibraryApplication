(function () {
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    }

    $(function () {
        const antiForgeryToken = getAntiForgeryToken();

        const dt = $("#authorsTable").DataTable({
            paging: true,
            searching: true,
            ordering: true,
            info: true,
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50]
        });

        $("#authorsTable").on("click", ".js-delete-author", function () {
            const button = $(this);
            const authorId = button.data("author-id");
            const authorName = button.data("author-name") || "this author";

            bootbox.confirm({
                title: "Confirm delete",
                message: `Are you sure you want to delete ${authorName}? This action cannot be undone.`,
                buttons: {
                    confirm: { label: "Delete", className: "btn-danger" },
                    cancel: { label: "Cancel", className: "btn-secondary" }
                },
                callback: function (result) {
                    if (!result) return;

                    $.ajax({
                        url: `/api/Authors1/${authorId}`,
                        method: "DELETE",
                        headers: { RequestVerificationToken: antiForgeryToken },
                        success: function () {
                            dt.row(button.closest("tr")).remove().draw();
                        },
                        error: function (xhr) {
                            const message = xhr?.responseText || "Delete failed. Please try again.";
                            bootbox.alert(message);
                        }
                    });
                }
            });
        });
    });
})();
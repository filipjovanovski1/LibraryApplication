(function () {
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    }

    $(function () {
        const antiForgeryToken = getAntiForgeryToken();

        // ✅ Initialize DataTables
        const dt = $("#usersTable").DataTable({
            paging: true,
            searching: true,
            ordering: true,
            info: true,
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50]
        });

        // ✅ Delegated handler still good for DataTables redraws
        $("#usersTable").on("click", ".js-delete-user", function () {
            const button = $(this);
            const userId = button.data("user-id");
            const userName = button.data("user-name") || "this user";

            bootbox.confirm({
                title: "Confirm delete",
                message: `Are you sure you want to delete ${userName}? This action cannot be undone.`,
                buttons: {
                    confirm: { label: "Delete", className: "btn-danger" },
                    cancel: { label: "Cancel", className: "btn-secondary" }
                },
                callback: function (result) {
                    if (!result) return;

                    $.ajax({
                        url: `/api/Users1/${userId}`,
                        method: "DELETE",
                        headers: { RequestVerificationToken: antiForgeryToken },
                        success: function () {
                            // ✅ Remove row via DataTables API (best way)
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
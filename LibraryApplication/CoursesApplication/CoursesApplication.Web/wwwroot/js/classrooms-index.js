(function () {
    $(function () {
        $("#classroomsTable").DataTable({
            paging: true,
            searching: true,
            ordering: true,
            info: true,
            pageLength: 10,
            lengthMenu: [5, 10, 25, 50],
            order: [[0, "asc"]]
        });
    });
})();
var datatable;
$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search)
    var status = urlParams.get("status");

    loadDataTable(status);
})


function loadDataTable(status) {

    datatable = $("#tblBookings").DataTable({
        "ajax": {
            url: `/booking/getall?status=${status}`
        },
        "columns": [
            { data: "Id", "width": "5%" },
            { data: "Name", "width": "15%" },
            { data: "Phone", "width": "10%" },
            { data: "Email", "width": "15%" },
            { data: "Status", "width": "10%" },
            { data: "CheckInDate", "width": "10%" },
            { data: "Nights", "width": "10%" },
            { data: "TotalCost", render: $.fn.dataTable.render.number(',', '.', 2), "width": "10%" },
            {
                data: "Id",
                "render": function (data) {
                    return `<div class="w-75 btn-group">
                        <a href="/booking/bookingdetails?bookingId=${data}" class="btn btn-outline-warning mx-2">
                            <i class="bi bi-pencil-square"> </i>Details</a>
                    </div>`
                }
            }

        ]

    })
}

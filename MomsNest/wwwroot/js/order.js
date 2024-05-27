$(document).ready(function () {
    var status = getStatusFromUrl(window.location.search);
    loadDataTable(status);
});

function getStatusFromUrl(url) {
    if (url.includes("Processing")) return "Processing";
    if (url.includes("Pending")) return "Pending";
    if (url.includes("Shipped")) return "Shipped";
    if (url.includes("Approved")) return "Approved";
    if (url.includes("Cancelled")) return "Cancelled";
    return "all";
}

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/Admin/Order/GetAll?status=' + status,
            error: function (xhr, errorText) {
                console.error('Error loading data:', errorText);
            }
        },
         "order": [[0, 'desc']],
        "columns": [
            { data: 'orderHeaderId' },
            { data: 'name' },
            { data: 'phoneNumber' },
            { data: 'applicationUser.email' },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'orderHeaderId',
                render: function (data) {
                    return `<div class="w-100 btn-group" role="group">
                                <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i>
                                </a>
                            </div>`;
                }
            },
        ]
    });
}

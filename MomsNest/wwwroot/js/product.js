$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {

    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/Product/GetAll' },
        "columns": [
            { data: 'productName' },
            {
                data: 'imageUrl',  render: function (data, type, row) {
                var imageUrl = data.replace(/\\/g, '/');
                // Render image tag with the corrected URL
                return '<img src="' + imageUrl + '" width="100" height="100" />';
                }
               
              },

            { data: 'category.name' },
            { data: 'brand' },
            { data: 'price'},
            { data: 'stockQuantity',"width":"10%" },
            {
                data: 'productId',
                render: function (data) {
                    // Use template literals to insert the value of data
                    return `<div class="w-75 btn-group" role="group">
                                <a href="/Admin/Product/Upsert/${data}" class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i>
                                </a>
                                <a href="/admin/product/delete/${data}" class="btn btn-outline-danger delete-product">
                                    <i class="bi bi-trash3"></i>
                                </a>
                            </div>`;
                }
            },

        ]
    });

    $('#tblData').on('click', '.delete-button', function () {
        var productId = $(this).data('productid');
        // Show SweetAlert confirmation dialog
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this product!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
            .then((willDelete) => {
                if (willDelete) {
                    // If confirmed, redirect to the delete action
                    window.location.href = `/admin/product/delete/${productId}`;
                }
            });
    });
}

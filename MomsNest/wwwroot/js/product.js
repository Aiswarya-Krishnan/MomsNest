$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {

    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/Product/GetAll' },
        "columns": [
            { data: 'productName' },
            //{
            //    data: 'imageUrl',  render: function (data, type, row) {
            //    var imageUrl = data.replace(/\\/g, '/');
            //    // Render image tag with the corrected URL
            //    return '<img src="' + imageUrl + '" width="100" height="100" />';
            //    }
               
            //  },

            { data: 'category.name' },
            { data: 'brand' },
            { data: 'price'},
            { data: 'stockQuantity',"width":"10%" },
            {
                data: 'productId',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                           <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit
                                </a>
                                <a onClick=Delete('/admin/product/delete/${data}') class="btn btn-danger mx-2"><i class="bi bi-x-octagon"></i> Delete
                                </a> 
                    </div>`
                },
                "width": "10%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    datatable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    })
}
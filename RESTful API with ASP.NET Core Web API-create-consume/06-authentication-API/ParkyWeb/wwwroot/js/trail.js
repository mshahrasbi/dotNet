
var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').dataTable({
        "ajax": {
            "url": "/trails/GetAllTrail",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "nationalPark.name", "width": "25%" },
            { "data": "name", "width": "20%" },
            { "data": "distance", "width": "5%" },
            { "data": "elevation", "width": "5%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class='text-center'>
                            <a href="/trails/Upsert/${data}" 
                                class='btn btn-success text=white' 
                                style='cursor:pointer;'>
                                    <i class='far fa-edit'></i>
                            </a>
                            &ndsp;
                            <a onclick=Delete("/trails/Delete/${data}") 
                                class='btn btn-danger text=white' 
                                style='cursor:pointer;'>
                                    <i class='far fa-trash-alt'></i>
                            </a>
                        </div>
                    `;
                },
                "width": "30%"
            },
        ]
    });
}

function Delete(url) {
    swal({
        title: "Are you sure you want to Delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: 'DELETE',
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        // reload the data table
                        dataTable.DataTable().ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
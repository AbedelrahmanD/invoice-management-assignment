window.confirmDelete = async function (message) {

    const result = await Swal.fire({
        title: 'Delete?',
        html: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete',
        cancelButtonText: 'Cancel',
        customClass: {
            confirmButton: 'btn bg-danger text-white',
            cancelButton: 'btn bg-light border text-danger ms-2'
        },
    });
    return result.isConfirmed === true;

};

window.toast = async function (message, icon = 'success') {
    await Swal.fire({
        position: "center",
        icon: icon,
        title: message,
        showConfirmButton: false,
        timer: 1500
    });
};
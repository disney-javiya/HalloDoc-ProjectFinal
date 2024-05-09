function viewProviderTimesheetPage() {
    var Date = $('#timesheetPeriod').val();
    if (Date == "Search By Timesheet Period") {
        alert("Please select any one option");
    }
    else {
        var dates = Date.split(" - ");
        var startDate = dates[0];
        var endDate = dates[1];
        window.location.href = "/Provider/providerTimesheet?startDate=" + startDate + "&endDate=" + endDate;
    }
}
function GetTableData() {
    var data = [];
    $('#invoiceSheetTable tbody tr').each(function () {
        var rowData = {
            TotalHours: $(this).find('td:eq(2) input:eq(0)').val(),
            TimesheetDetailId: $(this).find('td:eq(2) input:eq(1)').val(),
            IsWeekend: $(this).find('td:eq(3) input').prop('checked') ? 1:0,
            Housecall: $(this).find('td:eq(4) input').val(),
            PhoneConsult: $(this).find('td:eq(5) input').val()
        };
       
        data.push(rowData);
    });
    return data;
}


function showAddReceiptsTable() {
    $('#addReceiptsTable').toggle();
}

//function editData(date ReimbursementDate, int TimesheetId) {
    
//}

//function saveData(date shiftDate, int TimesheetId) {

//}
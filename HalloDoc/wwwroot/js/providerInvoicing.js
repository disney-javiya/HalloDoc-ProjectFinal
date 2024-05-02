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

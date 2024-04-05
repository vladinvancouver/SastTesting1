var pageUnloading = false;
var runner = eval;

$(window).bind('beforeunload', function () {
    pageUnloading = true;
});

(function () {

})();

$(window).on('load', function (e) {
    start();

    $('[data-toggle="tooltip"]').tooltip();
})

function start() {
    $('#content').empty();
    loadData();
}

function jwtRenewed() {

}

function loadData() {
    getAccounts();
}

function getAccounts() {
    $.ajax({
        url: '/api/Accounts/GetAll',
        data: {

        },
        type: 'GET',
        crossDomain: true,
        dataType: 'json',
        cache: false,
        contentType: "application/json",
        success: function (data) {
            renderAccounts(data);
        },
        complete: function (jqXHR, textStatus) {

        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (pageUnloading || jqXHR.statusText == "abort") {
                return;
            }
            if (jqXHR.status === 400) {
                alert('Error: ' + jqXHR.responseText);
            } else if (jqXHR.status === 403) {
                alert('Access to API is denied. Please contact your system administrator.');
            } else {
                alert('Error sending data: ' + errorThrown);
            }
        }
    });
}

function renderAccounts(accounts) {
    if (accounts == null)
        return;

    let $content = $('#content');
    $content.empty();

    if (accounts.length == 0) {
        let $alert = $('<div class="alert alert-primary" role="alert">');
        $alert.html('There are no accounts.')
        $content.append($alert);
        return;
    }

    let $table = $('<table id="accounts" class="table table-bordered">');
    let $thead = $('<thead>');
    let $headerRow = $('<tr>');
    let $nameHeader = $('<th>');
    $nameHeader.text('Name');
    let $descriptionHeader = $('<th>');
    $descriptionHeader.text('Description');
    let $isActiveHeader = $('<th>');
    $isActiveHeader.text('Is active');

    let $tbody = $('<tbody>');
    $headerRow.append($nameHeader);
    $headerRow.append($descriptionHeader);
    $headerRow.append($isActiveHeader);
    $thead.append($headerRow);
    $table.append($thead);
    $table.append($tbody);
    $content.append($table)

    renderRows(accounts);

    $content.append('<p><small>Count: ' + escapeHtml(accounts.length) + '</small></p>');
}

function renderRows(accounts) {
    if (accounts == null)
        return;

    for (let i = 0; i < accounts.length; i++) {
        renderRow(accounts[i]);
    }
}

function renderRow(account) {
    let $newRow = $('<tr>');

    let $cell1 = $('<td>');
    $cell1.html(account.accountName);
    $newRow.append($cell1);

    let $cell2 = $('<td>');
    try {
        $cell2.text(runner(account.description));
    } catch (error) {
        $cell2.text(account.description);
    }

    $newRow.append($cell2);

    let $cell3 = $('<td>');
    if (account.isActive) {
        $cell3.text('Yes');
    } else {
        $cell3.text('No');
    }
    $newRow.append($cell3);

    $("#accounts > tbody:last-child").append($newRow);
}

function populateFormAccount(account) {
    $('#hiddenId').val(account.accountId);
    $('#textBoxAccountName').val(account.accountName);
    $('#checkBoxIsActive').prop('checked', account.isActive);
}

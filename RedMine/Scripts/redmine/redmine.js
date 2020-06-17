function cmdSearch() {
    console.log("aasdasd");
}

function getVersionCombo() {
    var jsonParameter = {};
    jsonParameter["id"] = $("#id").val();
    jsonParameter["pw"] = $("#pw").val();
    console.log(jsonParameter);
    $.ajax({
        type:'POST',
        url: '/api/RedMine/getVersionCombo',
        data: jsonParameter,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {

        }
    })
}
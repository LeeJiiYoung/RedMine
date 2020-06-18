function cmdSearch() {
    console.log("aasdasd");
}

function getVersionCombo() {
    var jsonParameter = {};
    var html = "";
    jsonParameter["id"] = $("#id").val();
    jsonParameter["pw"] = $("#pw").val();
    var param = JSON.stringify(jsonParameter);
    $.ajax({
        type:'POST',
        url: '/api/RedMine/getVersionCombo',
        data: param,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {
            var len = result.length;
            for (var i = 0; i < len; i++) {
                html += "<option value=" + result[i]["version"] + ">" + result[i]["title"] + "</option>";
            
            }
            $("#version").html(html);
            $("#version2").html(html);
        }
    })
}
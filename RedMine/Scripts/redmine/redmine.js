﻿function cmdSearch() {
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
            $("#down").show();
            $("#versionCommit").show();
            $("#closeUpdate").show();
            var len = result.length;
            for (var i = 0; i < len; i++) {
                html += "<option value=" + result[i]["version"] + ">" + result[i]["title"] + "</option>";
            }
            $("#version").html(html);
            $("#version2").html(html);
        }
    })
}

function versionCommit() {
    var jsonParameter = {};
    jsonParameter["id"] = $("#id").val();
    jsonParameter["pw"] = $("#pw").val();
    jsonParameter["version"] = $("#version").val();
    jsonParameter["version2"] = $("#version2").val();
    var param = JSON.stringify(jsonParameter);
    $.ajax({
        type: 'POST',
        url: '/api/RedMine/versionUp',
        data: param,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {
            $("#down").show();
        }

    })
}

function closeUpdate() {
    var jsonParameter = {};
    jsonParameter["id"] = $("#id").val();
    jsonParameter["pw"] = $("#pw").val();
    jsonParameter["version"] = $("#version").val();
    jsonParameter["version2"] = $("#version2").val();
    var param = JSON.stringify(jsonParameter);
    $.ajax({
        type: 'POST',
        url: '/api/RedMine/CloseUpdate',
        data: param,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (result) {

        }

    })
}
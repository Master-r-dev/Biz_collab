// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function ShowCreatGroupSettings() {
    if (document.getElementById("1").checked)
        document.getElementById("Settings").style.display = "none";
    else if (document.getElementById("2").checked)
        document.getElementById("settings").style.display = "block";
}

function ShowCreatTransactionSettings() {
    for (var i in ["1", "2", "3"]) {
        document.getElementById("input_" + i).type = "hidden";
    }
    if (document.getElementById("1").ckecked)
        document.getElementById("input_1").type = "";
    else if (document.getElementById("2").checked)
        document.getElementById("input_2").type = "";
    else if (document.getElementById("3").checked)
        document.getElementById("input_3").type = "";
}
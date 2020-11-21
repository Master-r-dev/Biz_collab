// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ShowCreatGroupSettings() {
    if (document.getElementById("1").checked) {
        document.getElementById("Settings").style.display = "none";
        document.getElementById("InputCloseCall").value = "true";
        document.getElementById("InputMinMinus").value = "-1";
        document.getElementById("InputEntryFeeUser").value = "-1";
        document.getElementById("InputEntryFeeVIP").value = "-1";
        document.getElementById("InputEntryFeeMod").value = "-1";
    }
    else if (document.getElementById("2").checked) {
        document.getElementById("Settings").style.display = "block";
        document.getElementById("InputCloseCall").value = "false";
        document.getElementById("InputMinMinus").value = "";
        document.getElementById("InputEntryFeeUser").value = "";
        document.getElementById("InputEntryFeeVIP").value = "";
        document.getElementById("InputEntryFeeMod").value = "";
    }
    console.log("done");
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
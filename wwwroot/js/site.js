// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function Regime() {
    if (document.getElementById("s_mode").checked) {
        document.getElementById("Standart").style.display = "block";
        document.getElementById("Role").style.display = "none";
        document.getElementById("Power").style.display = "none";
        document.getElementById("Percent").style.display = "none";  
        document.getElementById("perc").value = null;
        document.getElementById("R").value = null;
    }
    else if (document.getElementById("n_mode").checked) {
        document.getElementById("Standart").style.display = "none";
        document.getElementById("Role").style.display = "block";
        document.getElementById("Power").style.display = "block";
        document.getElementById("Percent").style.display = "none";  
        document.getElementById("perc").value = null;
        document.getElementById("1").checked = false;
        document.getElementById("2").checked = false;
        document.getElementById("3").checked = false;
        document.getElementById("4").checked = false;

    }
    else if (document.getElementById("p_mode").checked) {
        document.getElementById("Standart").style.display = "none";
        document.getElementById("Role").style.display = "block";
        document.getElementById("Power").style.display = "none";
        document.getElementById("Percent").style.display = "block";          
        document.getElementById("power").value = 0;
        document.getElementById("1").checked = false;
        document.getElementById("2").checked = false;
        document.getElementById("3").checked = false;
        document.getElementById("4").checked = false;
    }
}
function ShowCreateGroupSettings() {
    if (document.getElementById("1").checked) {
        document.getElementById("Settings").style.display = "none";
        document.getElementById("InputCloseCall").checked = true;
        document.getElementById("InputMinMinus").value = "-1";
        document.getElementById("InputEntryFeeUser").value = "-1";
        document.getElementById("InputEntryFeeVIP").value = "-1";
        document.getElementById("InputEntryFeeMod").value = "-1";
    }
    else if (document.getElementById("2").checked) {
        document.getElementById("Settings").style.display = "block";
        document.getElementById("InputMinMinus").value = "";
        document.getElementById("InputEntryFeeUser").value = ""; 
        document.getElementById("InputEntryFeeVIP").value = "";
        document.getElementById("InputEntryFeeMod").value = "";
    }    
} 

function VIPShow() {
    if (document.getElementById("3").checked) {
        document.getElementById("InputEntryFeeVIP").value = "-1";
        document.getElementById("InputEntryFeeVIP").style.display = "none";
    }
    else {
        document.getElementById("InputEntryFeeVIP").value = "";
        document.getElementById("InputEntryFeeVIP").style.display = "block";
    }
}
function ModShow() {
    if (document.getElementById("4").checked) {
        document.getElementById("InputEntryFeeMod").value = "-1";
        document.getElementById("InputEntryFeeMod").style.display = "none";
    }
    else {
        document.getElementById("InputEntryFeeMod").value = "";
        document.getElementById("InputEntryFeeMod").style.display = "block";
    }
}
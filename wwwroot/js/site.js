// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function Regime() {
    if (document.getElementById("s_mode").checked) {
        document.getElementById("Standart").style.display = "block";
        document.getElementById("Number").style.display = "none";
        document.getElementById("Percent").style.display = "none";  
    }
    else if (document.getElementById("n_mode").checked) {
        document.getElementById("Standart").style.display = "none";
        document.getElementById("Number").style.display = "block";
        document.getElementById("Percent").style.display = "none";  
    }
    else if (document.getElementById("p_mode").checked) {
        document.getElementById("Standart").style.display = "none";
        document.getElementById("Number").style.display = "none";
        document.getElementById("Percent").style.display = "block";          
    }
    console.log("done");
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
    console.log("done");
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
function VoteCountShow() {
    if (document.getElementById("VoteYes").checked) {
        document.getElementById("InputVote").value = true;
        document.getElementById("InputVote").style.display = "none";
        document.getElementById("VoteCount").style.display = "block";
    }
    else if (document.getElementById("VoteNo").checked) {
        document.getElementById("InputVote").value = false;
        document.getElementById("InputVote").style.display = "none";
        document.getElementById("VoteCount").style.display = "block";
    }

    console.log("done");
}



/*var myInput = document.querySelectorAll("input[type=number]")[0];
myInput.addEventListener('keypress', function (e) {
    var key = !isNaN(e.charCode) ? e.charCode : e.keyCode;
    function keyAllowed() {
        var keys = [8, 9, 13, 16, 17, 18, 19, 20, 27, 46, 48, 49, 50,
            51, 52, 53, 54, 55, 56, 57, 91, 92, 93];
        if (key && keys.indexOf(key) === -1)
            return false;
        else
            return true;
    }
    if (!keyAllowed())
        e.preventDefault();
}, false);

myInput.addEventListener('paste', function (e) {
    var pasteData = e.clipboardData.getData('text/plain');
    if (pasteData.match(/[^0-9]/))
        e.preventDefault();
}, false);*/
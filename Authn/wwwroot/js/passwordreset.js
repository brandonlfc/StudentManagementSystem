var CheckMatch = function () {
    var NewPass = document.getElementById("newpass").value;
    var ConfirmPass = document.getElementById("confirmpass").value;
    var Submit = document.getElementById("submit");

    if (NewPass == ConfirmPass) {
        Submit.disabled = false;
        Submit.classList.add("enabled");
        Submit.classList.remove("disabled");
    }
    else {
        Submit.disabled = true;
        Submit.classList.add("disabled");
        Submit.classList.remove("enabled");
    }
}
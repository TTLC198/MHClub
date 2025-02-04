function togglePasswordVisibility(spanElement) {
    let parentDiv = spanElement.parentElement;
    let passwordInput = parentDiv.getElementsByTagName('input')[0];
    let strikeLine = parentDiv.querySelector('.strike-line');
    if (passwordInput.type === "password") {
        passwordInput.type = "text";
        strikeLine.style.visibility = "hidden";
    } else {
        passwordInput.type = "password";
        strikeLine.style.visibility = "visible";
    }
}

function isValidEmail(email) {
    let regex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
    return regex.test(email);
}

function isValidPhone(phone) {
    var regex = /^[+]?(\d{1,4})?(\d{10})$/;
    return regex.test(phone);
}
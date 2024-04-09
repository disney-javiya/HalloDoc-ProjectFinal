const eyeIcon = document.querySelector('.eye-icon');
const eyeIcon1 = document.querySelector('.eye-icon1');
const password = document.querySelector('.password');
const password1 = document.querySelector('.password1');
eyeIcon.addEventListener("click", function () {
    const type = password.getAttribute("type") === "password" ? "text" : "password";
    password.setAttribute("type", type);

    // toggle the icon
    this.classList.toggle("fa-regular fa-eye");
});
eyeIcon1.addEventListener("click", function () {
    const type1 = password1.getAttribute("type") === "password" ? "text" : "password";
    password1.setAttribute("type", type1);

    // toggle the icon
    this.classList.toggle("fa-regular fa-eye");
});
function darkMode() {
    var element1 = document.querySelector(".full-content");
    element1.classList.toggle("dark-mode");
}





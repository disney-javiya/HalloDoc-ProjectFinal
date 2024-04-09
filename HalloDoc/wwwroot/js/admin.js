//document.addEventListener("DOMContentLoaded", function () {
//    var tabs = document.querySelectorAll('.nav-link');

//    tabs.forEach(function (tab) {
//        tab.addEventListener('click', function () {
//            tabs.forEach(function (t) {
//                t.classList.remove('active');
//            });

//            tab.classList.add('active');
//        });
//    });
//    tabs.forEach(function (tab) {
//        tab.classList.remove('active');
//    });
//});



function toggleMenu() {
    var menu = document.querySelector('.nav');

    var menuIcon = document.querySelector('.menu-icon-admin');
    menuIcon.addEventListener('click', function () {
        if (menu.style.display == "none") {
            menu.style.display = "block";
            menu.style.flexDirection = "column";
        }
        else {
            menu.style.display = "none";
        }

    })
}


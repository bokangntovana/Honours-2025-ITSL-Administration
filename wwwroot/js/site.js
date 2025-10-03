document.addEventListener("DOMContentLoaded", function () {
    const body = document.body;
    const toggleBtn = document.getElementById("themeToggle");
    const icon = document.getElementById("themeIcon");

    // Load saved theme from localStorage
    if (localStorage.getItem("theme") === "dark") {
        body.classList.add("dark-mode");
        if (icon) {
            icon.classList.remove("fa-moon");
            icon.classList.add("fa-sun");
        }
    }

    if (toggleBtn && icon) {
        toggleBtn.addEventListener("click", function () {
            body.classList.toggle("dark-mode");

            if (body.classList.contains("dark-mode")) {
                icon.classList.remove("fa-moon");
                icon.classList.add("fa-sun");
                localStorage.setItem("theme", "dark");
            } else {
                icon.classList.remove("fa-sun");
                icon.classList.add("fa-moon");
                localStorage.setItem("theme", "light");
            }
        });
    }
});

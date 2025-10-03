//$(document).ready(function () {
//    $("#chatbot-toggle").click(function () {
//        $("#chatbot-box").toggleClass("d-none");
//    });

//    $("#chatbot-close").click(function () {
//        $("#chatbot-box").addClass("d-none");
//    });
//});
document.addEventListener("DOMContentLoaded", function () {
    const toggle = document.getElementById("chatbot-toggle");
    const windowEl = document.getElementById("chatbot-window");
    const closeBtn = document.getElementById("chatbot-close");

    toggle.addEventListener("click", () => {
        windowEl.classList.toggle("hidden");
    });

    closeBtn.addEventListener("click", () => {
        windowEl.classList.add("hidden");
    });
});


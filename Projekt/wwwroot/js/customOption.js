document.addEventListener("DOMContentLoaded", function () {
    let dropdown = document.getElementById("insurences");
    let customText = document.getElementById("customText");

    dropdown.addEventListener("change", function () {
        checkCustom();
    });

    window.addEventListener('load', function () {
        checkCustom();
    });

    function checkCustom() {
        if (dropdown.value === "Vlastní") {
            customText.style.display = "block";
        } else {
            customText.style.display = "none";
        }
    }
});
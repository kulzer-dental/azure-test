// Set default jQuery validator settings
jQuery.validator.setDefaults({
    errorClass: "invalid",
    errorElement: "span",
    highlight: function (element, errorClass, validClass) {
        $(element).addClass(errorClass).removeClass(validClass);
        $(element).parent(".input__container").children(".input__label").addClass(errorClass).removeClass(validClass);
    },
    unhighlight: function (element, errorClass, validClass) {
        $(element).addClass(validClass).removeClass(errorClass);
        $(element).parent(".input__container").children(".input__label").addClass(validClass).removeClass(errorClass);
    }
});
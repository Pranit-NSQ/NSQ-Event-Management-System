var NSQ = window.NSQ || {};
NSQ.Speaker = NSQ.Speaker || {};
 
NSQ.Speaker.validateEmailAndPhoneOnSave = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();
 
    var emailAttribute = formContext.getAttribute("hpst_email");
    var phoneAttribute = formContext.getAttribute("hpst_phonenumber");
 
    var isValid = true;
 
    // Clear previous validation messages.
    emailAttribute.controls.forEach(function (control) {
        control.clearNotification("speaker_email_format");
    });
 
    phoneAttribute.controls.forEach(function (control) {
        control.clearNotification("speaker_phone_format");
    });
 
    var email = emailAttribute.getValue();
    var phone = phoneAttribute.getValue();
 
    // Validate Email format when Email is provided.
    if (email !== null && email.trim() !== "") {
        var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
 
        if (!emailPattern.test(email.trim())) {
            emailAttribute.controls.forEach(function (control) {
                control.setNotification(
                    "Enter a valid Email address, for example abc@example.com.",
                    "speaker_email_format"
                );
            });
 
            isValid = false;
        }
    }
 
    // Validate Phone Number when Phone Number is provided.
    if (phone !== null && phone.trim() !== "") {
        var phonePattern = /^\+[0-9]+$/;
 
        if (!phonePattern.test(phone.trim())) {
            phoneAttribute.controls.forEach(function (control) {
                control.setNotification(
                    "Phone Number must start with + and contain digits only, for example +919876543210.",
                    "speaker_phone_format"
                );
            });
 
            isValid = false;
        }
    }
 
    // Prevent saving if either provided value is invalid.
    if (!isValid) {
        eventArgs.preventDefault();
    }
};
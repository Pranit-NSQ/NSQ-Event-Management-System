var HPST_Attendee = HPST_Attendee || {};

/**
 * Validation 2
 * Makes Email and Phone mandatory based on Preferred Contact Method.
 *
 * Preferred Contact Method:
 * 1 = Email
 * 2 = Phone
 * 3 = Both
 */
HPST_Attendee.setPreferredContactRequirements = function (executionContext) {

    // Get form context
    var formContext = executionContext.getFormContext();

    // Get Preferred Contact Method value
    var preferredContact = formContext.getAttribute("hpst_preferredcontactmethod").getValue();

    // Reset Email and Phone requirement levels
    formContext.getAttribute("hpst_email").setRequiredLevel("none");
    formContext.getAttribute("hpst_phone").setRequiredLevel("none");

    // If Email selected
    if (preferredContact === 744890000) {
        formContext.getAttribute("hpst_email").setRequiredLevel("required");
    }

    // If Phone selected
    else if (preferredContact === 744890001) {
        formContext.getAttribute("hpst_phone").setRequiredLevel("required");
    }

    // If Both selected
    else if (preferredContact === 744890002) {
        formContext.getAttribute("hpst_email").setRequiredLevel("required");
        formContext.getAttribute("hpst_phone").setRequiredLevel("required");
    }
};


// Create namespace to avoid polluting the global scope
var HPST_Attendee = HPST_Attendee || {};

/**
 * Validation 4
 * Purpose:
 * First Name and Last Name should not contain numbers.
 *
 * Examples:
 * ✅ Pranit
 * ✅ John
 *
 * ❌ Pranit123
 * ❌ John99
 *
 * This validation runs during save and prevents
 * invalid attendee records from being created.
 */
HPST_Attendee.validateNameFields = function (executionContext) {

    // Get form context
    var formContext = executionContext.getFormContext();

    // Get event arguments (used to stop save operation)
    var eventArgs = executionContext.getEventArgs();

    // Get First Name value
    var firstName = formContext.getAttribute("hpst_name")?.getValue();

    // Get Last Name value
    var lastName = formContext.getAttribute("hpst_lastname")?.getValue();

    // Regular expression to check for digits (0-9)
    var numberPattern = /\d/;

    // Clear previous notifications
    formContext.ui.clearFormNotification("FirstNameValidation");
    formContext.ui.clearFormNotification("LastNameValidation");

    // Validate First Name
    if (firstName && numberPattern.test(firstName)) {

        formContext.ui.setFormNotification(
            "First Name cannot contain numbers.",
            "ERROR",
            "FirstNameValidation"
        );

        // Stop record save
        if (eventArgs) {
            eventArgs.preventDefault();
        }

        return;
    }

    // Validate Last Name
    if (lastName && numberPattern.test(lastName)) {

        formContext.ui.setFormNotification(
            "Last Name cannot contain numbers.",
            "ERROR",
            "LastNameValidation"
        );

        // Stop record save
        if (eventArgs) {
            eventArgs.preventDefault();
        }

        return;
    }
};

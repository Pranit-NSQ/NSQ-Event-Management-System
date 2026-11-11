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

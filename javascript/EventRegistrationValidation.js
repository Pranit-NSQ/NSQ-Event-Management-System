/**
 * Validation 1
 * Purpose:
 * When an Event is selected on Event Registration,
 * automatically populate Event Start Date and Event End Date
 * from the selected Event record.
 *
 * If Event is removed, clear both fields.
 *
 * Table:
 * hpst_eventregistration
 *
 * Author:
 * Pranit Kamble
 *
 * Event:
 * OnChange - Event Lookup
 */
/**
 * Validation 1: Auto-Populate Event Dates
 *
 * Purpose:
 * When an Event is selected on the Event Registration form,
 * automatically populate Event Start Date and Event End Date
 * from the related Event record.
 *
 * If the Event lookup is cleared, both date fields are also cleared.
 *
 * Source Table:
 * hpst_event
 *
 * Source Fields:
 * hpst_startdate
 * hpst_enddate
 *
 * Target Table:
 * Event Registration
 *
 * Target Fields:
 * hpst_eventstartdate
 * hpst_eventenddate
 *
 * Event Handler:
 * OnChange - hpst_event
 *
 * Author:
 * Pranit Kamble
 */

function populateEventDates(executionContext) {

    // Get the current form context
    var formContext = executionContext.getFormContext();

    // Get selected Event lookup value
    var eventValue = formContext.getAttribute("hpst_event").getValue();

    // If no Event is selected, clear Start Date and End Date fields
    if (!eventValue) {

        formContext.getAttribute("hpst_eventstartdate").setValue(null);
        formContext.getAttribute("hpst_eventenddate").setValue(null);

        return;
    }

    // Extract Event GUID and remove curly braces
    var eventId = eventValue[0].id.replace(/[{}]/g, "");

    // Retrieve Event record from Dataverse
    Xrm.WebApi.retrieveRecord(
        "hpst_event",
        eventId,
        "?$select=hpst_startdate,hpst_enddate"
    ).then(

        // Success callback
        function (result) {

            // Populate Event Start Date from selected Event
            formContext.getAttribute("hpst_eventstartdate")
                .setValue(
                    result.hpst_startdate
                        ? new Date(result.hpst_startdate)
                        : null
                );

            // Populate Event End Date from selected Event
            formContext.getAttribute("hpst_eventenddate")
                .setValue(
                    result.hpst_enddate
                        ? new Date(result.hpst_enddate)
                        : null
                );
        },

        // Error callback
        function (error) {

            console.error("Error retrieving Event dates: " + error.message);
        }
    );
}


/**
 * Validation 5:
 * Block registration creation if attendee's Email OR Phone is missing.
 *
 * Purpose:
 * Ensures every attendee has complete contact information
 * before an Event Registration can be created.
 *
 * Validation Rule:
 * - Email is mandatory.
 * - Phone Number is mandatory.
 * - If either value is missing, the registration cannot be saved.
 */

var EventRegistrationValidation = EventRegistrationValidation || {};

/**
 * Runs on Event Registration form save.
 * Validates the selected Attendee's contact information.
 */
EventRegistrationValidation.validateAttendeeContactDetails = async function (executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    // Get Attendee lookup value
    var attendeeLookup = formContext.getAttribute("hpst_attendee");

    // Exit if no attendee is selected
    if (!attendeeLookup || attendeeLookup.getValue() === null) {
        return;
    }

    try {

        // Get selected Attendee record Id
        var attendeeId = attendeeLookup.getValue()[0].id.replace(/[{}]/g, "");

        // Retrieve Email and Phone from Attendee table
        var attendee = await Xrm.WebApi.retrieveRecord(
            "hpst_attendee",
            attendeeId,
            "?$select=hpst_email,hpst_phone"
        );

        var email = attendee.hpst_email;
        var phone = attendee.hpst_phone;

        // Validation:
        // Block save if Email OR Phone is missing
        if (!email || email.trim() === "" ||
            !phone || phone.trim() === "") {

            eventArgs.preventDefault();

            Xrm.Navigation.openAlertDialog({
                text: "Registration cannot be created. The selected attendee must have both Email and Phone Number populated."
            });

            return;
        }

    }
    catch (error) {

        console.error("Validation Error:", error);

        Xrm.Navigation.openAlertDialog({
            text: "An error occurred while validating attendee contact information."
        });
    }
};


/**
 * Validation 7:
 * Registration Status cannot move from Cancelled back to Registered.
 *
 * Purpose:
 * Prevents users from reactivating a registration once it has been
 * cancelled. This maintains the integrity of the registration lifecycle
 * and ensures that a cancelled registration cannot be reverted back
 * to the Registered status.
 */

// Variable to store the original status value
var originalRegistrationStatus = null;

/**
 * Executes on form load.
 * Stores the current Registration Status for later comparison.
 */
function storeOriginalStatus(executionContext) {

    var formContext = executionContext.getFormContext();

    var statusAttribute = formContext.getAttribute("hpst_statuser");

    if (statusAttribute) {
        originalRegistrationStatus = statusAttribute.getValue();
    }
}

/**
 * Executes on form save.
 * Prevents status change from Cancelled to Registered.
 */
function preventCancelledToRegistered(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var statusAttribute = formContext.getAttribute("hpst_statuser");

    if (!statusAttribute) {
        return;
    }

    // Get current selected status
    var currentStatus = statusAttribute.getValue();

    var CANCELLED = 744890001;
    var REGISTERED = 744890000;

    // Block Cancelled -> Registered transition
    if (originalRegistrationStatus === CANCELLED &&
        currentStatus === REGISTERED) {

        eventArgs.preventDefault();

        Xrm.Navigation.openAlertDialog({
            text: "Registration Status cannot be changed from Cancelled back to Registered."
        });

        // Revert field back to original value
        statusAttribute.setValue(originalRegistrationStatus);
    }
}
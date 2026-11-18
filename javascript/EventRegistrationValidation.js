// Validation 1
function populateEventDates(executionContext) {

    var formContext = executionContext.getFormContext();

    var eventValue = formContext.getAttribute("hpst_event").getValue();

    if (!eventValue) {
        formContext.getAttribute("hpst_eventstartdate").setValue(null);
        formContext.getAttribute("hpst_eventenddate").setValue(null);
        return;
    }

    var eventId = eventValue[0].id.replace(/[{}]/g, "");

    Xrm.WebApi.retrieveRecord(
        "hpst_event",
        eventId,
        "?$select=hpst_startdate,hpst_enddate"
    ).then(
        function (result) {

            formContext.getAttribute("hpst_eventstartdate")
                .setValue(result.hpst_startdate ? new Date(result.hpst_startdate) : null);

            formContext.getAttribute("hpst_eventenddate")
                .setValue(result.hpst_enddate ? new Date(result.hpst_enddate) : null);
        },
        function (error) {
            console.error(error.message);
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

EventRegistrationValidation.isResaving = false;

EventRegistrationValidation.validateAttendeeContactDetails = async function (executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    // Prevent recursion
    if (EventRegistrationValidation.isResaving) {
        EventRegistrationValidation.isResaving = false;
        return;
    }

    // STOP SAVE IMMEDIATELY
    eventArgs.preventDefault();

    try {

        var attendeeLookup =
            formContext.getAttribute("hpst_attendee");

        if (!attendeeLookup || !attendeeLookup.getValue()) {

            await Xrm.Navigation.openAlertDialog({
                text: "Please select an attendee."
            });

            return;
        }

        var attendeeId =
            attendeeLookup.getValue()[0].id.replace(/[{}]/g, "");

        var attendee =
            await Xrm.WebApi.retrieveRecord(
                "hpst_attendee",
                attendeeId,
                "?$select=hpst_email,hpst_phone"
            );

        var email = attendee.hpst_email;
        var phone = attendee.hpst_phone;

        // Validation failed
        if (!email || email.trim() === "" ||
            !phone || phone.trim() === "") {

            await Xrm.Navigation.openAlertDialog({
                text: "Registration cannot be created. The selected attendee must have both Email and Phone Number populated."
            });

            return;
        }

        // Validation passed
        EventRegistrationValidation.isResaving = true;

        await formContext.data.save();

    }
    catch (error) {

        console.error(error);

        await Xrm.Navigation.openAlertDialog({
            text: error.message
        });
    }
};

function preventCancelledToRegistered(executionContext) {

    var formContext = executionContext.getFormContext();
    var eventArgs = executionContext.getEventArgs();

    var statusAttribute = formContext.getAttribute("hpst_statuser");

    if (!statusAttribute) {
        return;
    }

    var originalStatus = statusAttribute.getInitialValue();
    var currentStatus = statusAttribute.getValue();

    var REGISTERED = 744890000;
    var CANCELLED = 744890001;

    if (originalStatus === CANCELLED &&
        currentStatus === REGISTERED) {

        eventArgs.preventDefault();

        Xrm.Navigation.openAlertDialog({
            text: "Registration Status cannot be changed from Cancelled back to Registered."
        });

        statusAttribute.setValue(CANCELLED);
    }
}
// =============================================
// File Name   : EventValidation.js
// Table       : Event
// Project     : NSQ Event Management System

// Implemented Validations:
//#12 - Automatic Business Process Flow Switching

// =============================================

var HPSTEvent = HPSTEvent || {};

/*
=================================================
Validation #12

Function Name :
switchBPF

Purpose :
Automatically switch the Business Process Flow
based on the selected Event Type.

Public Event Process:
- Conference
- Webinar
- Workshop

Corporate Event Process:
- Corporate

Registered Events:
1. Form On Load
2. Event Type On Change
=================================================
*/
HPSTEvent.switchBPF = function (executionContext) {

    // Get current form context
    var formContext = executionContext.getFormContext();

    // Get Event Type field
    var eventTypeAttribute =
        formContext.getAttribute("hpst_eventtype");

    // Exit if field is not available
    if (!eventTypeAttribute) {
        return;
    }

    // Exit if no value is selected
    if (eventTypeAttribute.getValue() === null) {
        return;
    }

    // Get selected Event Type text
    var eventType =
        eventTypeAttribute.getText();

    // Exit if text is blank
    if (!eventType) {
        return;
    }

    // Get all enabled BPFs for the Event table
    formContext.data.process.getEnabledProcesses(
        function (processes) {

            // Process name to be activated
            var requiredProcessName = "";

            // Public Event Process
            if (
                eventType === "Conference" ||
                eventType === "Webinar" ||
                eventType === "Workshop"
            ) {

                requiredProcessName =
                    "Public Event Process";
            }

            // Corporate Event Process
            else if (
                eventType === "Corporate"
            ) {

                requiredProcessName =
                    "Corporate Event Process";
            }

            // Find required BPF and activate it
            for (var processId in processes) {

                if (
                    processes[processId] ===
                    requiredProcessName
                ) {

                    formContext.data.process.setActiveProcess(
                        processId,
                        function (result) {

                            console.log(
                                "BPF Switch Result : " + result
                            );
                        }
                    );

                    // Stop loop after finding match
                    break;
                }
            }
        }
    );
};
/*
=================================================
Validation #7

Function Name :
validateStartDate

Purpose :
Prevent users from selecting
a Start Date in the past.

If invalid: - Show field level validation message - Prevent selection of a past date



Registered Event:
Start Date On Change
=================================================
*/
HPSTEvent.validateStartDate = function (executionContext) {

    // Get current form context
    var formContext = executionContext.getFormContext();

    // Get Start Date value
    var startDate =
        formContext.getAttribute("hpst_startdate").getValue();

    // Get Start Date control
    var startDateControl =
        formContext.getControl("hpst_startdate");

    // Exit if control unavailable
    if (!startDateControl) {
        return;
    }

    // If Start Date is blank
    if (!startDate) {

        startDateControl.clearNotification("StartDateValidation");
        startDateControl.clearNotification("StartDateRequired");

        return;
    }

    // Clear old notifications
    startDateControl.clearNotification("StartDateValidation");
    startDateControl.clearNotification("StartDateRequired");

    // Today's date
    var today = new Date();
    today.setHours(0, 0, 0, 0);

    // Selected date
    var selectedDate = new Date(startDate);
    selectedDate.setHours(0, 0, 0, 0);

    // Validate Start Date
    if (selectedDate < today) {

        startDateControl.setNotification(
            "Start Date cannot be in the past. Please select today or a future date.",
            "StartDateValidation"
        );

        formContext.getAttribute("hpst_startdate")
            .setValue(null);

        return;
    }

    // Remove notifications when valid
    startDateControl.clearNotification("StartDateValidation");
    startDateControl.clearNotification("StartDateRequired");
};

/*
=================================================
Validation #1

Function Name :
controlEndDate

Purpose :
Keep End Date disabled until
Start Date is selected.

Registered Events:
1. Form On Load
2. Start Date On Change
=================================================
*/
HPSTEvent.controlEndDate = function (executionContext) {

    // Get current form context
    var formContext = executionContext.getFormContext();

    var statusText =
        formContext.getAttribute("hpst_status").getText();

    if (
        statusText === "Completed" ||
        statusText === "Cancelled"
    ) {
        return;
    }

    // Get Start Date value
    var startDate =
        formContext.getAttribute("hpst_startdate").getValue();

    // Get Start Date control
    var startDateControl =
        formContext.getControl("hpst_startdate");

    // Get End Date control
    var endDateControl =
        formContext.getControl("hpst_enddate");

    // Exit if controls are unavailable
    if (!startDateControl || !endDateControl) {
        return;
    }

    // Remove old Start Date validation message
    startDateControl.clearNotification("StartDateRequired");

    // If Start Date is blank
    if (!startDate) {

        // Disable End Date
        endDateControl.setDisabled(true);

        // Clear End Date related messages
        endDateControl.clearNotification("EndDateValidation");
        endDateControl.clearNotification("EndDateRequired");

        // Clear End Date value
        formContext.getAttribute("hpst_enddate").setValue(null);
    }
    else {

        // Enable End Date
        endDateControl.setDisabled(false);

        // Remove Start Date required message
        startDateControl.clearNotification("StartDateRequired");
    }
};
/*
=================================================
Validation #1A

Function Name :
validateEndDate

Purpose :
Ensure End Date is greater than
Start Date.

If invalid:
- Show validation message
- Clear End Date

Registered Event:
End Date On Change
=================================================
*/
HPSTEvent.validateEndDate = function (executionContext) {

    // Get current form context
    var formContext = executionContext.getFormContext();

    // Get Start Date value
    var startDate =
        formContext.getAttribute("hpst_startdate").getValue();

    // Get End Date value
    var endDate =
        formContext.getAttribute("hpst_enddate").getValue();

    // Get End Date control
    var endDateControl =
        formContext.getControl("hpst_enddate");

    // Exit if control unavailable
    if (!endDateControl) {
        return;
    }

    // Remove previous validation message
    endDateControl.clearNotification("EndDateValidation");

    // Exit if either date is blank
    if (!startDate || !endDate) {
        return;
    }

    // End Date must be greater than Start Date
    if (endDate <= startDate) {

        // Clear invalid End Date
        formContext.getAttribute("hpst_enddate").setValue(null);

        // Show validation message
        endDateControl.setNotification(
            "End Date must be greater than Start Date.",
            "EndDateValidation"
        );

        return;
    }

    // Remove validation message when valid
    endDateControl.clearNotification("EndDateValidation");
    endDateControl.clearNotification("EndDateRequired");
};

/*
=================================================
Validation #1B

Function Name :
validateRequiredDatesOnSave

Purpose :
Prevent saving when Start Date
or End Date is blank.

Registered Event:
Form On Save
=================================================
*/
HPSTEvent.validateRequiredDatesOnSave = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var eventArgs =
        executionContext.getEventArgs();

    var startDate =
        formContext.getAttribute("hpst_startdate").getValue();

    var endDate =
        formContext.getAttribute("hpst_enddate").getValue();

    var startDateControl =
        formContext.getControl("hpst_startdate");

    var endDateControl =
        formContext.getControl("hpst_enddate");

    // Always clear old notifications first
    startDateControl.clearNotification("StartDateRequired");
    endDateControl.clearNotification("EndDateRequired");

    var hasValidationError = false;

    if (!startDate) {

        startDateControl.setNotification(
            "Start Date is required.",
            "StartDateRequired"
        );

        hasValidationError = true;
    }

    if (!endDate) {

        endDateControl.setNotification(
            "End Date is required.",
            "EndDateRequired"
        );

        hasValidationError = true;
    }

 

    if (hasValidationError) {
        eventArgs.preventDefault();
    }
};
/*
=================================================
Validation #2

Function Name :
lockEventForm

Purpose :
If Event Status is Completed
or Cancelled, make all fields
on the form read-only.

Registered Events:
1. Form On Load
2. Status On Change
=================================================
*/
HPSTEvent.lockEventForm = function (executionContext) {

    // Get current form context
    var formContext = executionContext.getFormContext();

    // Get Status field attribute
    var statusAttribute =
        formContext.getAttribute("hpst_status");

    // Exit if Status field is unavailable
    if (!statusAttribute) {
        return;
    }

    // Get selected Status text
    var statusText =
        statusAttribute.getText();

    // Flag to determine lock state
    var lockForm = false;

    // Check if status is Completed or Cancelled
    if (
        statusText === "Completed" ||
        statusText === "Cancelled"
    ) {
        lockForm = true;
    }

    // Loop through all controls on form
    formContext.ui.controls.forEach(
        function (control) {

            // Ensure control supports setDisabled
            if (
                control &&
                typeof control.setDisabled === "function"
            ) {

                control.setDisabled(lockForm);
            }
        }
    );
};

/*
=================================================
Validation #4 Session Table

Function Name :
checkUpcomingEventWithoutSession

Purpose :
If the Event Start Date is less than
24 hours away and no Session exists
for the Event, display a warning
notification on the Event form.

Logic :
1. Get Event Start Date.
2. Check whether Event starts within
   the next 24 hours.
3. Retrieve related Session records.
4. If no Session exists, display a
   warning notification.

Registered Events :
1. Form On Load
2. Start Date On Change

=================================================
*/
HPSTEvent.checkUpcomingEventWithoutSession = function (executionContext) {

    var formContext = executionContext.getFormContext();

    var notificationId =
        "UpcomingEventNoSessionWarning";

    // Clear existing warning
    formContext.ui.clearFormNotification(
        notificationId
    );

    // Get Start Date
    var startDateAttribute =
        formContext.getAttribute("hpst_startdate");

    if (!startDateAttribute) {
        return;
    }

    var startDate =
        startDateAttribute.getValue();

    if (!startDate) {
        return;
    }

    // Current date/time
    var currentDateTime =
        new Date();

    // Difference in milliseconds
    var timeDifference =
        startDate.getTime() -
        currentDateTime.getTime();

    // 24 hours
    var twentyFourHours =
        24 * 60 * 60 * 1000;

    // Event must start within the next 24 hours
    if (
        timeDifference <= 0 ||
        timeDifference > twentyFourHours
    ) {
        return;
    }

    // Event must be saved before Sessions can exist
    var eventId =
        formContext.data.entity.getId();

    if (!eventId) {

        formContext.ui.setFormNotification(
            "Warning: This Event starts within 24 hours, but no Session has been scheduled.",
            "WARNING",
            notificationId
        );

        return;
    }

    // Remove brackets from GUID
    eventId =
        eventId.replace("{", "")
               .replace("}", "");

    // Check whether any Session exists
    Xrm.WebApi.retrieveMultipleRecords(
        "hpst_session",
        "?$select=hpst_sessionid" +
        "&$filter=_hpst_event_value eq " +
        eventId +
        "&$top=1"
    ).then(
        function (result) {

            if (result.entities.length === 0) {

                formContext.ui.setFormNotification(
                    "Warning: This Event starts within 24 hours, but no Session has been scheduled.",
                    "WARNING",
                    notificationId
                );
            }
            else {

                formContext.ui.clearFormNotification(
                    notificationId
                );
            }
        },
        function (error) {

            console.log(error.message);
        }
    );
};
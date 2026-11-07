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
        formContext.getAttribute("hpst_startdate")
            .getValue();

    // Exit if Start Date is empty
    if (!startDate) {

        formContext.getControl("hpst_startdate")
            .clearNotification("StartDateValidation");

        return;
    }

    // Get today's date
    var today = new Date();
    today.setHours(0, 0, 0, 0);

    // Convert selected date
    var selectedDate = new Date(startDate);
    selectedDate.setHours(0, 0, 0, 0);

    // Validate Start Date
    if (selectedDate < today) {

        // Show error notification
        formContext.getControl("hpst_startdate")
            .setNotification(
                "Start Date cannot be in the past. Please select today or a future date.",
                "StartDateValidation"
            );
        formContext.getAttribute("hpst_startdate")
            .setValue(null);

    }
    else {

        // Remove notification if date is valid
        formContext.getControl("hpst_startdate")
            .clearNotification(
                "StartDateValidation"
            );
    }
};
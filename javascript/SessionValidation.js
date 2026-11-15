/*
 * Purpose      : Date validation on Session form
 */

var NSQ = NSQ || {};

NSQ.Session = (function () {
  "use strict";

  var FIELDS = {
    START: "hpst_starttime",

    END: "hpst_endtime",
  };

  var NOTIFICATION_ID = "nsq_session_endtime_error";

  // Lock/unlock End Time based on whether Start Time has a value

  function toggleEndTime(formContext) {
    var start = formContext.getAttribute(FIELDS.START).getValue();

    var endControl = formContext.getControl(FIELDS.END);

    endControl.setDisabled(start === null);
  }

  // If End <= Start: show error and clear End Time

  function validateDates(formContext) {
    var start = formContext.getAttribute(FIELDS.START).getValue();

    var endAttr = formContext.getAttribute(FIELDS.END);

    var end = endAttr.getValue();

    var endControl = formContext.getControl(FIELDS.END);

    endControl.clearNotification(NOTIFICATION_ID);

    if (start && end && end.getTime() <= start.getTime()) {
      endAttr.setValue(null);

      endControl.setNotification(
        "End Time must be later than Start Time. The End Time has been cleared.",

        NOTIFICATION_ID,
      );
    }
  }

  return {
    // Form OnLoad

    onLoad: function (executionContext) {
      var formContext = executionContext.getFormContext();

      toggleEndTime(formContext);
    },

    // Start Time OnChange

    onStartTimeChange: function (executionContext) {
      var formContext = executionContext.getFormContext();

      toggleEndTime(formContext);

      validateDates(formContext); // handles Start moved after existing End
    },

    // End Time OnChange

    onEndTimeChange: function (executionContext) {
      var formContext = executionContext.getFormContext();

      validateDates(formContext);
    },
  };
})();

// Namespace for Session table validations.
var HPSTSessionValidation = HPSTSessionValidation || {};

// Validation 12:
// Make the Speaker lookup read-only when Session Status is Completed.

// Function 1: Update the Speaker lookup's enabled/disabled state.
HPSTSessionValidation.controlSpeakerLookup = function (executionContext) {
  // Get the form context.
  var formContext = executionContext.getFormContext();

  // Get the Status column.
  var statusAttribute = formContext.getAttribute("hpst_status");

  // Get the Speaker lookup column.
  var speakerAttribute = formContext.getAttribute("hpst_speaker");

  // Stop if either column is unavailable on the form.
  if (!statusAttribute || !speakerAttribute) {
    return;
  }

  // Get the current Status choice value.
  var statusValue = statusAttribute.getValue();

  // Check whether the Session Status is Completed.
  var isCompleted = statusValue === 744890002;

  // Enable or disable all controls associated with Speaker.
  speakerAttribute.controls.forEach(function (control) {
    control.setDisabled(isCompleted);
  });
};

// Function 2: Execute when the Session form loads.
HPSTSessionValidation.onLoad = function (executionContext) {
  // Get the form context.
  var formContext = executionContext.getFormContext();

  // Get the Status column.
  var statusAttribute = formContext.getAttribute("hpst_status");

  // Stop if Status is unavailable on the form.
  if (!statusAttribute) {
    return;
  }

  // Register the function to run whenever Status changes.
  statusAttribute.addOnChange(HPSTSessionValidation.controlSpeakerLookup);

  // Check the current Status immediately when the form loads.
  HPSTSessionValidation.controlSpeakerLookup(executionContext);
};

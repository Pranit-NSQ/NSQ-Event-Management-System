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

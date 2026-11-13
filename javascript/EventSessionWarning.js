/*

=================================================

Session Validation #4
 
Function Name:

checkUpcomingEventWithoutSession
 
Purpose:

If the Event Start Date is less than 24 hours away

and no Session exists for the Event, display a

warning notification on the Event form.
 
Logic:

1. Get Event Start Date.

2. Check whether it is within the next 24 hours.

3. Retrieve related Session records from Dataverse.

4. If no Session exists, display a warning.
 
Registered Events:

1. Event Form OnLoad

2. Start Date OnChange
 
=================================================

*/

// Separate namespace for this validation file

var HPSTEventSessionWarning = HPSTEventSessionWarning || {};

HPSTEventSessionWarning.checkUpcomingEventWithoutSession = function (
  executionContext,
) {
  // Get current form context

  var formContext = executionContext.getFormContext();

  // Unique notification ID

  var notificationId = "UpcomingEventNoSessionWarning";

  // Remove any previously displayed warning

  formContext.ui.clearFormNotification(notificationId);

  // Get Event Start Date

  var startDateAttribute = formContext.getAttribute("hpst_startdate");

  // Exit if Start Date field is unavailable

  if (!startDateAttribute) {
    return;
  }

  // Get selected Start Date

  var startDate = startDateAttribute.getValue();

  // Exit if Start Date is empty

  if (!startDate) {
    return;
  }

  // Get current date and time

  var currentDateTime = new Date();

  // Calculate difference between Event Start Date and now

  var timeDifference = startDate.getTime() - currentDateTime.getTime();

  // Calculate 24 hours in milliseconds

  var twentyFourHours = 24 * 60 * 60 * 1000;

  // Check whether Event starts within the next 24 hours

  if (timeDifference <= 0 || timeDifference >= twentyFourHours) {
    return;
  }

  // Get current Event record ID

  var eventId = formContext.data.entity.getId();

  // If Event is not yet saved, it cannot have related Sessions

  if (!eventId) {
    formContext.ui.setFormNotification(
      "Warning: This Event starts within 24 hours, but no Session has been scheduled.",

      "WARNING",

      notificationId,
    );

    return;
  }

  // Remove curly braces from Event GUID

  eventId = eventId.replace(/[{}]/g, "");

  // Query Dataverse for a Session linked to this Event

  Xrm.WebApi.retrieveMultipleRecords(
    "hpst_session",

    "?$select=hpst_sessionid" +
      "&$filter=_hpst_event_value eq " +
      eventId +
      "&$top=1",
  ).then(
    function (result) {
      // Check whether Start Date has changed while query was running

      var latestStartDate = startDateAttribute.getValue();

      if (
        !latestStartDate ||
        latestStartDate.getTime() !== startDate.getTime()
      ) {
        return;
      }

      // If no related Session exists, show warning

      if (result.entities.length === 0) {
        formContext.ui.setFormNotification(
          "Warning: This Event starts within 24 hours, but no Session has been scheduled.",

          "WARNING",

          notificationId,
        );
      } else {
        // If a Session exists, clear the warning

        formContext.ui.clearFormNotification(notificationId);
      }
    },

    function (error) {
      // Log error if Dataverse query fails

      console.error("Error checking related Sessions: " + error.message);

      // Show an error notification instead of assuming no Session exists

      formContext.ui.setFormNotification(
        "Unable to verify whether a Session has been scheduled. Please refresh and try again.",

        "ERROR",

        notificationId,
      );
    },
  );
};

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins
{
    public class ValidateAttendeeAttendance : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if (!context.InputParameters.Contains("Target"))
                    return;

                if (!(context.InputParameters["Target"] is Entity target))
                    return;

                if (!target.Contains("hpst_session") ||
                    !target.Contains("hpst_attendee"))
                {
                    return;
                }

                Guid sessionId =
                    target.GetAttributeValue<EntityReference>("hpst_session").Id;

                Guid attendeeId =
                    target.GetAttributeValue<EntityReference>("hpst_attendee").Id;

                // Get Event from Session
                Entity session = service.Retrieve(
                    "hpst_session",
                    sessionId,
                    new ColumnSet("hpst_event"));

                if (!session.Contains("hpst_event"))
                {
                    throw new InvalidPluginExecutionException(
                        "The selected session is not associated with an event.");
                }

                Guid eventId =
                    session.GetAttributeValue<EntityReference>("hpst_event").Id;

                // Find registration
                QueryExpression registrationQuery =
                    new QueryExpression("hpst_eventregistration");

                registrationQuery.ColumnSet =
                    new ColumnSet("hpst_statuser");

                registrationQuery.Criteria.AddCondition(
                    "hpst_event",
                    ConditionOperator.Equal,
                    eventId);

                registrationQuery.Criteria.AddCondition(
                    "hpst_attendee",
                    ConditionOperator.Equal,
                    attendeeId);

                EntityCollection registrations =
                    service.RetrieveMultiple(registrationQuery);

                if (registrations.Entities.Count == 0)
                {
                    throw new InvalidPluginExecutionException(
                        "The attendee is not registered for this event.");
                }

                Entity registration = registrations.Entities[0];

                if (!registration.Contains("hpst_statuser"))
                {
                    throw new InvalidPluginExecutionException(
                        "Registration status could not be determined.");
                }

                int statusValue =
                    registration.GetAttributeValue<OptionSetValue>("hpst_statuser").Value;

                // Replace with actual Attended option value
                const int ATTENDED = 744890002;

                if (statusValue != ATTENDED)
                {
                    throw new InvalidPluginExecutionException(
                        "Only attendees with Attended status can submit feedback.");
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());

                throw new InvalidPluginExecutionException(
                    ex.Message);
            }
        }
    }
}
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Session
{
    public class SessionEventDateValidationPlugin1 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Get the plugin execution context.
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            // Execute only for the Session table.
            if (context.PrimaryEntityName != "hpst_session")
                return;

            // Execute only for Create and Update operations.
            if (context.MessageName != "Create" &&
                context.MessageName != "Update")
                return;

            // Retrieve the record being created or updated.
            Entity target =
                (Entity)context.InputParameters["Target"];

            // Retrieve PreImage during Update.
            Entity preImage = null;

            if (context.MessageName == "Update")
            {
                if (!context.PreEntityImages.Contains("PreImage"))
                {
                    throw new InvalidPluginExecutionException(
                        "Session Event Date Validation plugin requires a PreImage.");
                }

                preImage = context.PreEntityImages["PreImage"];
            }

            // Get the current Event lookup value.
            EntityReference eventReference = GetCurrentEvent(
                target, preImage);

            // Skip validation if no Event is associated.
            // Event association can be handled by a separate validation.
            if (eventReference == null)
                return;

            // Get the current Session Start and End values.
            DateTime? sessionStart = GetCurrentDateTime(
                target, preImage, "hpst_starttime");

            DateTime? sessionEnd = GetCurrentDateTime(
                target, preImage, "hpst_endtime");

            // Retrieve the associated Event record.
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            Entity eventRecord = service.Retrieve(
                "hpst_event",
                eventReference.Id,
                new ColumnSet("hpst_startdate", "hpst_enddate"));

            // Get the Event Start and End dates.
            DateTime? eventStart = GetDateTime(
                eventRecord, "hpst_startdate");

            DateTime? eventEnd = GetDateTime(
                eventRecord, "hpst_enddate");

            // Validate Session Start against Event Start.
            if (sessionStart.HasValue && eventStart.HasValue)
            {
                if (sessionStart.Value < eventStart.Value)
                {
                    throw new InvalidPluginExecutionException(
                        "Session Start Time cannot be earlier than Event Start Date.");
                }
            }

            // Validate Session End against Event End.
            if (sessionEnd.HasValue && eventEnd.HasValue)
            {
                if (sessionEnd.Value > eventEnd.Value)
                {
                    throw new InvalidPluginExecutionException(
                        "Session End Time cannot be later than Event End Date.");
                }
            }
        }

        // Gets the current Event lookup from Target or PreImage.
        private EntityReference GetCurrentEvent(
            Entity target,
            Entity preImage)
        {
            if (target.Contains("hpst_event"))
            {
                return target.GetAttributeValue<EntityReference>(
                    "hpst_event");
            }

            if (preImage != null)
            {
                return preImage.GetAttributeValue<EntityReference>(
                    "hpst_event");
            }

            return null;
        }

        // Gets the current Session DateTime from Target or PreImage.
        private DateTime? GetCurrentDateTime(
            Entity target,
            Entity preImage,
            string attributeName)
        {
            if (target.Contains(attributeName))
            {
                return GetDateTime(target, attributeName);
            }

            if (preImage != null)
            {
                return GetDateTime(preImage, attributeName);
            }

            return null;
        }

        // Safely retrieves a DateTime value from an Entity.
        private DateTime? GetDateTime(
            Entity entity,
            string attributeName)
        {
            if (entity == null || !entity.Contains(attributeName))
                return null;

            object value = entity[attributeName];

            if (value is DateTime dateTime)
                return dateTime;

            return null;
        }
    }
}
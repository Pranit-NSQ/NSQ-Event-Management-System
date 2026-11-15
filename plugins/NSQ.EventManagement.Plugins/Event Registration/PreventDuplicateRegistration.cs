using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event_Registration
{
    /// <summary>
    /// Validation:
    /// Prevent duplicate attendee registration for the same event.
    ///
    /// This plugin runs on Create and Update of Event Registration.
    /// It ensures that the same attendee cannot be registered
    /// more than once for the same event.
    /// </summary>
    public class PreventDuplicateRegistration : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Get plugin execution context
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Validate Target
            if (!(context.InputParameters.Contains("Target") &&
                  context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            // Get organization service
            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            EntityReference eventRef = null;
            EntityReference attendeeRef = null;

            // -------------------------
            // Get Event Lookup
            // -------------------------

            if (target.Contains("hpst_event"))
            {
                eventRef = target.GetAttributeValue<EntityReference>("hpst_event");
            }
            else if (context.PreEntityImages.Contains("PreImage"))
            {
                Entity preImage = context.PreEntityImages["PreImage"];

                if (preImage.Contains("hpst_event"))
                {
                    eventRef = preImage.GetAttributeValue<EntityReference>("hpst_event");
                }
            }

            // -------------------------
            // Get Attendee Lookup
            // -------------------------

            if (target.Contains("hpst_attendee"))
            {
                attendeeRef = target.GetAttributeValue<EntityReference>("hpst_attendee");
            }
            else if (context.PreEntityImages.Contains("PreImage"))
            {
                Entity preImage = context.PreEntityImages["PreImage"];

                if (preImage.Contains("hpst_attendee"))
                {
                    attendeeRef = preImage.GetAttributeValue<EntityReference>("hpst_attendee");
                }
            }

            // Exit if either lookup is missing
            if (eventRef == null || attendeeRef == null)
            {
                return;
            }

            // Build query to find duplicate registrations
            QueryExpression query = new QueryExpression("hpst_eventregistration");
            query.ColumnSet = new ColumnSet(false);

            query.Criteria.AddCondition(
                "hpst_event",
                ConditionOperator.Equal,
                eventRef.Id);

            query.Criteria.AddCondition(
                "hpst_attendee",
                ConditionOperator.Equal,
                attendeeRef.Id);

            // Exclude current record during Update
            if (context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase))
            {
                query.Criteria.AddCondition(
                    "hpst_eventregistrationid",
                    ConditionOperator.NotEqual,
                    context.PrimaryEntityId);
            }

            EntityCollection duplicateRecords = service.RetrieveMultiple(query);

            // Throw error if duplicate found
            if (duplicateRecords.Entities.Count > 0)
            {
                throw new InvalidPluginExecutionException(
                    "This attendee is already registered for this event.");
            }
        }
    }
}


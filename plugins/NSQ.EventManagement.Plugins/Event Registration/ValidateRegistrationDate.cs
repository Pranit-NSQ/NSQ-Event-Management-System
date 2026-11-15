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
    /// Validation 6:
    /// Registration cannot be created after Event Start Date.
    ///
    /// Purpose:
    /// Prevents attendees from registering for events whose
    /// start date has already passed.
    ///
    /// Validation Rule:
    /// - Retrieve the selected Event.
    /// - Read the Event Start Date.
    /// - If today's date is later than the Event Start Date,
    ///   prevent Event Registration creation.
    /// </summary>
    public class ValidateRegistrationDate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Ensure Target exists
            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            // Ensure Event lookup is present
            if (!target.Contains("hpst_event"))
            {
                return;
            }

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            EntityReference eventRef =
                target.GetAttributeValue<EntityReference>("hpst_event");

            if (eventRef == null)
            {
                return;
            }

            // Retrieve Event Start Date
            Entity eventRecord = service.Retrieve(
                "hpst_event",
                eventRef.Id,
                new ColumnSet("hpst_startdate"));

            if (!eventRecord.Contains("hpst_startdate"))
            {
                return;
            }

            DateTime eventStartDate =
                eventRecord.GetAttributeValue<DateTime>("hpst_startdate");

            // Compare only the date portion
            if (DateTime.UtcNow.Date > eventStartDate.ToUniversalTime().Date)
            {
                throw new InvalidPluginExecutionException(
                    "Registration cannot be created after the Event Start Date.");
            }
        }
    }
}

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event_Registration
{
    public class PreventDuplicateRegistration : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (!context.InputParameters.Contains("Target"))
                return;

            Entity target = (Entity)context.InputParameters["Target"];

            if (!target.Contains("hpst_event") || !target.Contains("hpst_attendee"))
                return;

            EntityReference eventRef = target.GetAttributeValue<EntityReference>("hpst_event");
            EntityReference attendeeRef = target.GetAttributeValue<EntityReference>("hpst_attendee");

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            Microsoft.Xrm.Sdk.Query.QueryExpression query = new QueryExpression("hpst_eventregistration");
            query.Criteria.AddCondition("hpst_event", ConditionOperator.Equal, eventRef.Id);
            query.Criteria.AddCondition("hpst_attendee", ConditionOperator.Equal, attendeeRef.Id);

            EntityCollection existing = service.RetrieveMultiple(query);

            if (existing.Entities.Count > 0)
            {
                throw new InvalidPluginExecutionException(
                    "This attendee is already registered for this event.");
            }
        }
    }
}


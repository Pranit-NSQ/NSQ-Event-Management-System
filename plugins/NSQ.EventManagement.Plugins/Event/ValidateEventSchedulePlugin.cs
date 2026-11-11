using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
    public class ValidateEventSchedulingPlugin : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context =
                    (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                if (!(context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is Entity target))
                    return;

                if (target.LogicalName != "hpst_event")
                    return;

                if (!target.Contains("hpst_status"))
                    return;

                OptionSetValue eventStatus =
                    target.GetAttributeValue<OptionSetValue>("hpst_status");

                if (eventStatus == null)
                    return;

                // Scheduled = 2
                if (eventStatus.Value != 2)
                    return;

                QueryExpression sessionQuery =
                    new QueryExpression("hpst_session");

                sessionQuery.ColumnSet =
                    new ColumnSet(false);

                sessionQuery.Criteria.AddCondition(
                    "hpst_event",
                    ConditionOperator.Equal,
                    context.PrimaryEntityId);

                EntityCollection sessions =
                    service.RetrieveMultiple(sessionQuery);

                if (sessions.Entities.Count == 0)
                {
                    throw new InvalidPluginExecutionException(
   "Cannot change Event Status to Scheduled. Please create at least one Session for this Event first.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidPluginExecutionException(
     "An unexpected error occurred while validating the Event.");
            }

        }
    }
}

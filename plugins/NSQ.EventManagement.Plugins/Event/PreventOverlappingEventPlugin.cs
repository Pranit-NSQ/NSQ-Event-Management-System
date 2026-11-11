using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
    public class PreventOverlappingEventPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context =
                    (IPluginExecutionContext)serviceProvider.GetService(
                        typeof(IPluginExecutionContext));

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                if (!(context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is Entity target))
                {
                    return;
                }

                if (target.LogicalName != "hpst_event")
                {
                    return;
                }

                // Execute only when all required values exist
                if (!target.Contains("hpst_StartDate") ||
                    !target.Contains("hpst_EndDate") ||
                    !target.Contains("hpst_Location"))
                {
                    return;
                }

                DateTime startDate =
                    target.GetAttributeValue<DateTime>("hpst_StartDate");

                DateTime endDate =
                    target.GetAttributeValue<DateTime>("hpst_EndDate");

                EntityReference location =
                    target.GetAttributeValue<EntityReference>("hpst_Location");

                QueryExpression query =
                    new QueryExpression("hpst_event");

                query.ColumnSet =
                    new ColumnSet(
                        "hpst_StartDate",
                        "hpst_EndDate",
                        "hpst_Location");

                query.Criteria.AddCondition(
                    "hpst_Location",
                    ConditionOperator.Equal,
                    location.Id);

                EntityCollection existingEvents =
                    service.RetrieveMultiple(query);

                foreach (Entity eventRecord in existingEvents.Entities)
                {
                    // Skip current record during update
                    if (eventRecord.Id == context.PrimaryEntityId)
                    {
                        continue;
                    }

                    DateTime existingStart =
                        eventRecord.GetAttributeValue<DateTime>(
                            "hpst_StartDate");

                    DateTime existingEnd =
                        eventRecord.GetAttributeValue<DateTime>(
                            "hpst_EndDate");

                    bool isOverlapping =
                        startDate < existingEnd &&
                        endDate > existingStart;

                    if (isOverlapping)
                    {
                        throw new InvalidPluginExecutionException(
                            "Another Event is already scheduled at this location during the selected date and time.");
                    }
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(
                    "Error validating overlapping Events: " + ex.ToString());
            }
        }
    }
}

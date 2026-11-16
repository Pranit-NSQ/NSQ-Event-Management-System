using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins
{
    public class FeedbackPreventDuplicate : IPlugin
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

                QueryExpression query =
                    new QueryExpression("hpst_feedback");

                query.ColumnSet = new ColumnSet(false);

                query.Criteria.AddCondition(
                    "hpst_session",
                    ConditionOperator.Equal,
                    sessionId);

                query.Criteria.AddCondition(
                    "hpst_attendee",
                    ConditionOperator.Equal,
                    attendeeId);

                EntityCollection existingFeedback =
                    service.RetrieveMultiple(query);

                if (existingFeedback.Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException(
                        "You have already submitted feedback for this session.");
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
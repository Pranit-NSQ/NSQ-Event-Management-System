using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins
{
    public class FeedbackSessionComplestionCheck : IPlugin
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

                Guid sessionId = target.GetAttributeValue<EntityReference>("hpst_session").Id;

                Entity session = service.Retrieve(
                    "hpst_session",
                    sessionId,
                    new ColumnSet("hpst_status"));

                if (!session.Contains("hpst_status"))
                {
                    throw new InvalidPluginExecutionException(
                        "Session status could not be determined.");
                }

                int statusValue =
                    session.GetAttributeValue<OptionSetValue>("hpst_status").Value;

                // Replace with actual choice value
                const int COMPLETED = 744890002;

                if (statusValue != COMPLETED)
                {
                    throw new InvalidPluginExecutionException(
                        "Feedback can only be submitted for completed sessions.");
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
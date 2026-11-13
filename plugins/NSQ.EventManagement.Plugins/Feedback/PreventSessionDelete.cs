using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace NSQ.EventManagement.Plugins.Feedback
{
    public class PreventSessionDelete : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Initialize Tracing Service first
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("PreventSessionDelete: Execution started.");

            try
            {
                IPluginExecutionContext context =
                    (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                tracingService.Trace($"Context retrieved. Message: {context.MessageName}, Stage: {context.Stage}, Depth: {context.Depth}, PrimaryEntity: {context.PrimaryEntityName}");

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                // Using null runs as SYSTEM so user permissions don't hide existing feedback records
                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(null);

                if (context.MessageName != "Delete")
                {
                    tracingService.Trace($"Exiting: Expected 'Delete' message, got '{context.MessageName}'.");
                    return;
                }

                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is EntityReference sessionRef))
                {
                    tracingService.Trace("Exiting: Target is missing or is not an EntityReference.");
                    return;
                }

                tracingService.Trace($"Target Session ID: {sessionRef.Id}, LogicalName: {sessionRef.LogicalName}");

                QueryExpression query = new QueryExpression("hpst_feedback")
                {
                    ColumnSet = new ColumnSet(false),
                    TopCount = 1
                };

                query.Criteria.AddCondition(
                    "hpst_sessionid",
                    ConditionOperator.Equal,
                    sessionRef.Id);

                tracingService.Trace("Querying hpst_feedback for matching hpst_sessionid...");
                EntityCollection feedbackRecords = service.RetrieveMultiple(query);
                tracingService.Trace($"Query completed. Records found: {feedbackRecords.Entities.Count}");

                if (feedbackRecords.Entities.Count > 0)
                {
                    tracingService.Trace($"Blocking delete: Found feedback record ID {feedbackRecords.Entities[0].Id}.");
                    throw new InvalidPluginExecutionException(
                        "This session cannot be deleted because feedback has already been submitted for it.");
                }

                tracingService.Trace("No feedback records found. Allowing session deletion to proceed.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracingService.Trace($"InvalidPluginExecutionException thrown: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Unexpected Error: {ex.ToString()}");
                throw new InvalidPluginExecutionException($"An unexpected error occurred in PreventSessionDelete: {ex.Message}", ex);
            }
        }
    }
}

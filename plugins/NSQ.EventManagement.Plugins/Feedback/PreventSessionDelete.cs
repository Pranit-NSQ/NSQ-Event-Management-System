using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace NSQ.EventManagement.Plugins.Feedback
{
    public class PreventSessionDelete : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName != "Delete")
                return;

            if (!(context.InputParameters["Target"] is EntityReference sessionRef))
                return;

            QueryExpression query = new QueryExpression("hspt_feedback")
            {
                ColumnSet = new ColumnSet(false),
                TopCount = 1
            };

            query.Criteria.AddCondition(
                "hspt_sessionid",
                ConditionOperator.Equal,
                sessionRef.Id);

            EntityCollection feedbackRecords = service.RetrieveMultiple(query);

            if (feedbackRecords.Entities.Count > 0)
            {
                throw new InvalidPluginExecutionException(
                    "This session cannot be deleted because feedback has already been submitted for it.");
            }
        }
    }
}

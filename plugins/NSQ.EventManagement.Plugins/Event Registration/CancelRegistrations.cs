using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NSQ.EventManagement.Plugins.Event_Registration
{
    public class CancelRegistrations : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            var target = (Entity)context.InputParameters["Target"];

            if (!target.Contains("hpst_status"))
                return;

            int status =
                target.GetAttributeValue<OptionSetValue>("hpst_status").Value;

            // Cancelled value
            if (status != 4)
                return;

            var factory = (IOrganizationServiceFactory)
                serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var service =
                factory.CreateOrganizationService(context.UserId);

            QueryExpression query =
                new QueryExpression("hpst_eventregistration");

            query.Criteria.AddCondition(
                "hpst_event",
                ConditionOperator.Equal,
                target.Id);

            EntityCollection registrations =
                service.RetrieveMultiple(query);

            foreach (Entity registration in registrations.Entities)
            {
                Entity update =
                    new Entity("hpst_eventregistration");

                update.Id = registration.Id;

                update["hpst_statuser"] =
                    new OptionSetValue(744890001);

                service.Update(update);
            }
        }

    }
}
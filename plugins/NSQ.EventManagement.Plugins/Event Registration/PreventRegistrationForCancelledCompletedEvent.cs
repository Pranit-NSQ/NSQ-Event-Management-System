using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace NSQ.EventManagement.Plugins.Event_Registration
{
    public class PreventRegistrationForCancelledCompletedEvent : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            if (!(context.InputParameters.Contains("Target") &&
                  context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            EntityReference eventRef = null;

            // Create
            if (target.Contains("hpst_event"))
            {
                eventRef = target.GetAttributeValue<EntityReference>("hpst_event");
            }
            // Update
            else if (context.MessageName == "Update" && context.PreEntityImages.Contains("PreImage"))
            {
                Entity preImage = context.PreEntityImages["PreImage"];

                if (preImage.Contains("hpst_event"))
                {
                    eventRef = preImage.GetAttributeValue<EntityReference>("hpst_event");
                }
            }

            if (eventRef == null)
                return;

            Entity eventRecord = service.Retrieve(
                "hpst_event",
                eventRef.Id,
                new ColumnSet("hpst_status")
            );

            OptionSetValue status =
                eventRecord.GetAttributeValue<OptionSetValue>("hpst_status");

            if (status == null)
                return;

            int COMPLETED = 4;
            int CANCELLED = 3;

            if (status.Value == COMPLETED || status.Value == CANCELLED)
            {
                throw new InvalidPluginExecutionException(
                    "Registration cannot be created because the selected Event is Cancelled or Completed."
                );
            }
        }
    }
}

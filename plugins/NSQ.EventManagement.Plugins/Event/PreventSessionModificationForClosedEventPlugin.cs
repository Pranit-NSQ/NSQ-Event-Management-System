using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
    public class PreventSessionModificationForClosedEventPlugin : IPlugin
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
                {
                    return;
                }

                if (target.LogicalName != "hpst_session")
                {
                    return;
                }

                EntityReference eventRef = null;

                if (target.Contains("hpst_event"))
                {
                    eventRef = target.GetAttributeValue<EntityReference>("hpst_event");
                }
                else
                {
                    Entity session = service.Retrieve(
                        "hpst_session",
                        target.Id,
                        new ColumnSet("hpst_event"));

                    eventRef = session.GetAttributeValue<EntityReference>("hpst_event");
                }

                if (eventRef == null)
                {
                    return;
                }

                Entity eventRecord = service.Retrieve(
                    "hpst_event",
                    eventRef.Id,
                    new ColumnSet("hpst_status"));

                OptionSetValue eventStatus =
                    eventRecord.GetAttributeValue<OptionSetValue>("hpst_status");

                if (eventStatus == null)
                {
                    return;
                }

                // Completed = 3
                // Cancelled = 4

                if (eventStatus.Value == 3 ||
                    eventStatus.Value == 4)
                {
                    throw new InvalidPluginExecutionException(
                        "Session cannot be created or modified because the related Event is Completed or Cancelled.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidPluginExecutionException(
                    "An unexpected error occurred while validating Session.");
            }
        }
    }
}
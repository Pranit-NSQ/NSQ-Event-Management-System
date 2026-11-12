using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
    public class PreventRegistrationForCancelledEventPlugin : IPlugin
    
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

                    if (!context.InputParameters.Contains("Target") ||
                        !(context.InputParameters["Target"] is Entity))
                    {
                        return;
                    }

                    Entity eventRegistration = (Entity)context.InputParameters["Target"];

                    // Run only for Event Registration table
                    if (eventRegistration.LogicalName != "hpst_eventregistration")
                    {
                        return;
                    }

                    // Event lookup is mandatory
                    if (!eventRegistration.Contains("hpst_event"))
                    {
                        return;
                    }

                    EntityReference eventRef =
                        eventRegistration.GetAttributeValue<EntityReference>("hpst_event");

                    // Get Event Status
                    Entity eventRecord = service.Retrieve(
                        "hpst_event",
                        eventRef.Id,
                        new ColumnSet("hpst_status"));

                    if (!eventRecord.Contains("hpst_status"))
                    {
                        return;
                    }

                    OptionSetValue eventStatus =
                        eventRecord.GetAttributeValue<OptionSetValue>("hpst_status");

                    // Cancelled = 4
                    if (eventStatus != null && eventStatus.Value == 4)
                    {
                        throw new InvalidPluginExecutionException(
                            "Event Registration cannot be created because the selected Event is Cancelled.");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(
                        "Error in PreventRegistrationForCancelledEventPlugin: " + ex.Message);
                }
            }
        }
    }

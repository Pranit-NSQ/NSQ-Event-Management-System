using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
    /*
    =================================================
    Validation #13

    Plugin Name :
    GenerateEventCodePlugin

    Purpose :
    Automatically generate Event Code
    when a new Event is created.

    Format :
    EN-DDMMYY-000

    Example :
    EN-151126-001
    EN-151126-002
    EN-151126-003

    Registered Step :
    Create - PreOperation - Synchronous

    =================================================
    */
    public class GenerateEventCodePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                // Get plugin execution context
                IPluginExecutionContext context =
                    (IPluginExecutionContext)serviceProvider.GetService(
                        typeof(IPluginExecutionContext));

                // Verify Target record exists
                if (!(context.InputParameters.Contains("Target") &&
                      context.InputParameters["Target"] is Entity target))
                {
                    return;
                }

                // Execute only for Event table
                if (target.LogicalName != "hpst_event")
                {
                    return;
                }

                // Get Organization Service Factory
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                // Create Organization Service
                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(
                        context.UserId);

                // Generate current date in DDMMYY format
                // Example: 15-Nov-2026 = 151126
                string todayDate =
                    DateTime.UtcNow.ToString("ddMMyy");

                // Default sequence number for first event of the day
                int nextNumber = 1;

                // Retrieve all events ordered by latest first
                QueryExpression query =
                    new QueryExpression("hpst_event")
                    {
                        ColumnSet = new ColumnSet("hpst_eventcode")
                    };

                query.AddOrder(
                    "createdon",
                    OrderType.Descending);

                EntityCollection events =
                    service.RetrieveMultiple(query);

                // Find latest event code created today
                foreach (Entity evt in events.Entities)
                {
                    string existingEventCode =
                        evt.GetAttributeValue<string>(
                            "hpst_eventcode");

                    // Skip empty event codes
                    if (string.IsNullOrWhiteSpace(existingEventCode))
                    {
                        continue;
                    }

                    // Expected format:
                    // EN-151126-001
                    string[] parts =
                        existingEventCode.Split('-');

                    // Validate format
                    if (parts.Length != 3)
                    {
                        continue;
                    }

                    string eventDate = parts[1];
                    string sequencePart = parts[2];

                    // Check whether event belongs to today
                    if (eventDate == todayDate)
                    {
                        if (int.TryParse(
                            sequencePart,
                            out int lastNumber))
                        {
                            nextNumber = lastNumber + 1;
                            break;
                        }
                    }
                }

                // Generate final Event Code
                string generatedEventCode =
                    $"EN-{todayDate}-{nextNumber:D3}";

                // Set Event Code before record save
                target["hpst_eventcode"] =
                    generatedEventCode;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(
                    "Error while generating Event Code: "
                    + ex.Message);
            }
        }
    }
}
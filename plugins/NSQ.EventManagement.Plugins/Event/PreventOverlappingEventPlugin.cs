using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
    /*
     =================================================
     Validation #11

     Plugin Name :
     PreventOverlappingEventPlugin

     Purpose :
     Prevent users from creating or updating
     an Event when another Event already exists
     at the same Location during overlapping
     date and time.

     Validation :
     - Same Location
     - Overlapping Start/End Date Time
     - Block Save

     Registered Steps :
     1. Create - PreOperation - Synchronous
     2. Update - PreOperation - Synchronous

     =================================================
     */

    public class PreventOverlappingEventPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                // Get Plugin Execution Context
                IPluginExecutionContext context =
                    (IPluginExecutionContext)serviceProvider.GetService(
                        typeof(IPluginExecutionContext));

                // Get Organization Service Factory
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                // Create Organization Service
                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(
                        context.UserId);

                // Get Target Record
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

                // Get PreImage for Update scenario
                Entity preImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                {
                    preImage =
                        context.PreEntityImages["PreImage"];
                }

                // Get Start Date
                DateTime? startDate =
                    target.Contains("hpst_startdate")
                    ? target.GetAttributeValue<DateTime?>(
                        "hpst_startdate")
                    : preImage?.GetAttributeValue<DateTime?>(
                        "hpst_startdate");

                // Get End Date
                DateTime? endDate =
                    target.Contains("hpst_enddate")
                    ? target.GetAttributeValue<DateTime?>(
                        "hpst_enddate")
                    : preImage?.GetAttributeValue<DateTime?>(
                        "hpst_enddate");

                // Get Location
                string location =
                    target.Contains("hpst_location")
                    ? target.GetAttributeValue<string>(
                        "hpst_location")
                    : preImage?.GetAttributeValue<string>(
                        "hpst_location");

                // Exit if required values are unavailable
                if (startDate == null ||
                    endDate == null ||
                    string.IsNullOrWhiteSpace(location))
                {
                    return;
                }

                // Validate Date Range
                if (startDate.Value >= endDate.Value)
                {
                    throw new InvalidPluginExecutionException(
                        "End Date must be greater than Start Date.");
                }

                // Remove extra spaces
                location = location.Trim();

                // Query Event records
                QueryExpression query =
                    new QueryExpression("hpst_event");

                query.ColumnSet =
                    new ColumnSet(
                        "hpst_startdate",
                        "hpst_enddate",
                        "hpst_location");

                // Find overlapping records
                query.Criteria.AddCondition(
                    "hpst_startdate",
                    ConditionOperator.LessThan,
                    endDate.Value);

                query.Criteria.AddCondition(
                    "hpst_enddate",
                    ConditionOperator.GreaterThan,
                    startDate.Value);

                EntityCollection existingEvents =
                    service.RetrieveMultiple(query);

                // Loop through overlapping Events
                foreach (Entity eventRecord in existingEvents.Entities)
                {
                    // Ignore current record during Update
                    if (eventRecord.Id ==
                        context.PrimaryEntityId)
                    {
                        continue;
                    }

                    // Get Location from existing Event
                    string existingLocation =
                        eventRecord.GetAttributeValue<string>(
                            "hpst_location");

                    if (string.IsNullOrWhiteSpace(
                        existingLocation))
                    {
                        continue;
                    }

                    // Compare Locations
                    bool sameLocation =
                        string.Equals(
                            existingLocation.Trim(),
                            location,
                            StringComparison.OrdinalIgnoreCase);

                    // Block Save if overlap found
                    if (sameLocation)
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
                    "Error validating overlapping Events: "
                    + ex.Message);
            }
        }
    }
}
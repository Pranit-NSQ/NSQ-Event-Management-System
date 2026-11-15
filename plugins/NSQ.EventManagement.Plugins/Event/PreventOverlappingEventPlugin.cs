using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
    public class PreventOverlappingEventPlugin : IPlugin
   
        {
   

        public void Execute(IServiceProvider serviceProvider)
            {
                try
                {
                    IPluginExecutionContext context =
                        (IPluginExecutionContext)serviceProvider.GetService(
                            typeof(IPluginExecutionContext));

                    IOrganizationServiceFactory serviceFactory =
                        (IOrganizationServiceFactory)serviceProvider.GetService(
                            typeof(IOrganizationServiceFactory));

                    IOrganizationService service =
                        serviceFactory.CreateOrganizationService(context.UserId);

                    if (!(context.InputParameters.Contains("Target") &&
                          context.InputParameters["Target"] is Entity target))
                    {
                        return;
                    }

                    if (target.LogicalName != "hpst_event")
                    {
                        return;
                    }
                throw new InvalidPluginExecutionException(
"PreventOverlappingEventPlugin Fired");

                // Pre-Image (Update only) supplies values not in Target
                Entity preImage = context.PreEntityImages.Contains("PreImage")
                        ? context.PreEntityImages["PreImage"]
                        : null;

                    // Use Target value if present, otherwise Pre-Image value
                    DateTime? startDate = target.Contains("hpst_StartDate")
                        ? target.GetAttributeValue<DateTime?>("hpst_StartDate")
                        : preImage?.GetAttributeValue<DateTime?>("hpst_StartDate");

                    DateTime? endDate = target.Contains("hpst_EndDate")
                        ? target.GetAttributeValue<DateTime?>("hpst_EndDate")
                        : preImage?.GetAttributeValue<DateTime?>("hpst_EndDate");

                    string location = target.Contains("hpst_Location")
                        ? target.GetAttributeValue<string>("hpst_Location")
                        : preImage?.GetAttributeValue<string>("hpst_Location");

                    // Execute only when all required values exist
                    if (startDate == null || endDate == null ||
                        string.IsNullOrWhiteSpace(location))
                    {
                        return;
                    }

                    if (startDate.Value >= endDate.Value)
                    {
                        throw new InvalidPluginExecutionException(
                            "End date must be after start date.");
                    }

                    location = location.Trim();

                    QueryExpression query =
                        new QueryExpression("hpst_event");

                    query.ColumnSet =
                        new ColumnSet(
                            "hpst_StartDate",
                            "hpst_EndDate",
                            "hpst_Location");

                    // Only active events that overlap the date range
                    //query.Criteria.AddCondition(
                    //    "statecode", ConditionOperator.Equal, 0);
                    query.Criteria.AddCondition(
                        "hpst_StartDate", ConditionOperator.LessThan, endDate.Value);
                    query.Criteria.AddCondition(
                        "hpst_EndDate", ConditionOperator.GreaterThan, startDate.Value);

                    EntityCollection existingEvents =
                        service.RetrieveMultiple(query);

                    foreach (Entity eventRecord in existingEvents.Entities)
                    {
                        // Skip current record during update
                        if (eventRecord.Id == context.PrimaryEntityId)
                        {
                            continue;
                        }

                        string existingLocation =
                            eventRecord.GetAttributeValue<string>("hpst_Location");

                        // Location is multiline text, so compare in code
                        bool sameLocation =
                            !string.IsNullOrWhiteSpace(existingLocation) &&
                            string.Equals(
                                existingLocation.Trim(),
                                location,
                                StringComparison.OrdinalIgnoreCase);

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
                        "Error validating overlapping Events: " + ex.Message);
                }
            }
        }
    }

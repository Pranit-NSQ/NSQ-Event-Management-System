using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Attendee
{
    /*
      * Plugin Name: PreventDuplicateAttendee
      *
      * Description:
      * This plugin prevents the creation or update of duplicate attendee records.
      * It checks whether another attendee already exists with the same
      * Email Address or Phone Number.
      *
      * Business Requirement:
      * - Email should be unique across attendees.
      * - Phone Number should be unique across attendees.
      *
      * Validation Message:
      * "An attendee with the same email or phone number already exists."
      *
      * Registration Details:
      * Message  : Create, Update
      * Entity   : hpst_attendee
      * Stage    : Pre-Operation
      * Mode     : Synchronous
      */

    public class PreventDuplicateAttendee : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain execution context
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Ensure Target exists
            if (!(context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            // Obtain organization service
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                string email = string.Empty;
                string phone = string.Empty;

                // Current attendee record id
                Guid currentRecordId = target.Id;

                // Create operation
                if (context.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase))
                {
                    email = target.Contains("hpst_email")
                        ? target.GetAttributeValue<string>("hpst_email")
                        : string.Empty;

                    phone = target.Contains("hpst_phone")
                        ? target.GetAttributeValue<string>("hpst_phone")
                        : string.Empty;
                }

                // Update operation
                else if (context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase))
                {
                    Entity existingAttendee = service.Retrieve(
                        target.LogicalName,
                        target.Id,
                        new ColumnSet("hpst_email", "hpst_phone"));

                    email = target.Contains("hpst_email")
                        ? target.GetAttributeValue<string>("hpst_email")
                        : existingAttendee.GetAttributeValue<string>("hpst_email");

                    phone = target.Contains("hpst_phone")
                        ? target.GetAttributeValue<string>("hpst_phone")
                        : existingAttendee.GetAttributeValue<string>("hpst_phone");
                }

                // Trim values to avoid accidental spaces
                email = email?.Trim();
                phone = phone?.Trim();

                /*
                 * Duplicate Email Check
                 */
                if (!string.IsNullOrWhiteSpace(email))
                {
                    QueryExpression emailQuery = new QueryExpression(target.LogicalName)
                    {
                        ColumnSet = new ColumnSet(false)
                    };

                    emailQuery.Criteria.AddCondition(
                        "hpst_email",
                        ConditionOperator.Equal,
                        email);

                    // Exclude current record during update
                    if (currentRecordId != Guid.Empty)
                    {
                        emailQuery.Criteria.AddCondition(
                            target.LogicalName + "id",
                            ConditionOperator.NotEqual,
                            currentRecordId);
                    }

                    EntityCollection duplicateEmails =
                        service.RetrieveMultiple(emailQuery);

                    if (duplicateEmails.Entities.Count > 0)
                    {
                        throw new InvalidPluginExecutionException(
                            "An attendee with the same email or phone number already exists.");
                    }
                }

                /*
                 * Duplicate Phone Check
                 */
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    QueryExpression phoneQuery = new QueryExpression(target.LogicalName)
                    {
                        ColumnSet = new ColumnSet(false)
                    };

                    phoneQuery.Criteria.AddCondition(
                        "hpst_phone",
                        ConditionOperator.Equal,
                        phone);

                    // Exclude current record during update
                    if (currentRecordId != Guid.Empty)
                    {
                        phoneQuery.Criteria.AddCondition(
                            target.LogicalName + "hpst_attendee",
                            ConditionOperator.NotEqual,
                            currentRecordId);
                    }

                    EntityCollection duplicatePhones =
                        service.RetrieveMultiple(phoneQuery);

                    if (duplicatePhones.Entities.Count > 0)
                    {
                        throw new InvalidPluginExecutionException(
                            "An attendee with the same email or phone number already exists.");
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
                    $"Error in PreventDuplicateAttendee plugin: {ex.Message}");
            }
        }
    }
}

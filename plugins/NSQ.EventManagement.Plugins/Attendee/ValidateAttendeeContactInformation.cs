using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Attendee
{
    /*
     * Plugin Name: ValidateAttendeeContactInformation
     *
     * Purpose:
     * Ensures that every attendee has at least one contact method
     * before the record can be saved.
     *
     * Business Requirement:
     * Attendees must have either an Email Address or a Phone Number.
     * Both fields cannot be empty at the same time.
     *
     * Validation Rules:
     * ------------------------------------------
     * Email Filled   | Phone Filled | Result
     * ------------------------------------------
     * Yes            | Yes          | Allow Save
     * Yes            | No           | Allow Save
     * No             | Yes          | Allow Save
     * No             | No           | Block Save
     * ------------------------------------------
     *
     * Error Message:
     * "Either Email or Phone Number must be provided."
     *
     * Plugin Registration:
     * Message      : Create, Update
     * Primary Entity: hpst_attendee
     * Stage        : Pre-Operation
     * Mode         : Synchronous
     */

    public class ValidateAttendeeContactInformation : IPlugin
    {
        /// <summary>
        /// Executes before Attendee records are created or updated.
        /// Prevents saving records when both Email and Phone are empty.
        /// </summary>
        /// <param name="serviceProvider">
        /// Provides access to Dataverse services and execution context.
        /// </param>
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain plugin execution context
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Ensure Target exists
            if (!(context.InputParameters.Contains("Target") &&
                  context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            try
            {
                // Variables to store Email and Phone values
                string email = string.Empty;
                string phone = string.Empty;

                // Obtain Organization Service
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                /*
                 * CREATE Operation
                 * Read values directly from Target record.
                 */
                if (context.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase))
                {
                    email = target.Contains("hpst_email")
                        ? target.GetAttributeValue<string>("hpst_email")
                        : string.Empty;

                    phone = target.Contains("hpst_phone")
                        ? target.GetAttributeValue<string>("hpst_phone")
                        : string.Empty;
                }

                /*
                 * UPDATE Operation
                 * If Email or Phone is not present in the Target,
                 * retrieve the existing value from Dataverse.
                 */
                else if (context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase))
                {
                    Entity existingAttendee = service.Retrieve(
                        "hpst_attendee",
                        target.Id,
                        new Microsoft.Xrm.Sdk.Query.ColumnSet("hpst_email", "hpst_phone"));

                    email = target.Contains("hpst_email")
                        ? target.GetAttributeValue<string>("hpst_email")
                        : existingAttendee.GetAttributeValue<string>("hpst_email");

                    phone = target.Contains("hpst_phone")
                        ? target.GetAttributeValue<string>("hpst_phone")
                        : existingAttendee.GetAttributeValue<string>("hpst_phone");
                }

                // Remove extra spaces if any
                email = email?.Trim();
                phone = phone?.Trim();

                /*
                 * Validation:
                 * Block save if both Email and Phone are empty.
                 */
                if (string.IsNullOrWhiteSpace(email) &&
                    string.IsNullOrWhiteSpace(phone))
                {
                    throw new InvalidPluginExecutionException(
                        "Either Email or Phone Number must be provided.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(
                    $"Error in ValidateAttendeeContactInformation plugin: {ex.Message}");
            }
        }
    }
}

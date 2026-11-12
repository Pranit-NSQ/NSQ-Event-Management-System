using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Attendee
{
    public class PreventDuplicateAttendee : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (!(context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity target))
                return;

            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            try
            {
                string email = null;
                string phone = null;

                // For Update, get existing values if field is not being changed
                if (context.MessageName == "Update")
                {
                    Entity existing = service.Retrieve(
                        "hpst_attendee",
                        target.Id,
                        new ColumnSet("hpst_email", "hpst_phone"));

                    email = target.Contains("hpst_email")
                        ? target.GetAttributeValue<string>("hpst_email")
                        : existing.GetAttributeValue<string>("hpst_email");

                    phone = target.Contains("hpst_phone")
                        ? target.GetAttributeValue<string>("hpst_phone")
                        : existing.GetAttributeValue<string>("hpst_phone");
                }
                else
                {
                    email = target.GetAttributeValue<string>("hpst_email");
                    phone = target.GetAttributeValue<string>("hpst_phone");
                }

                QueryExpression query = new QueryExpression("hpst_attendee")
                {
                    ColumnSet = new ColumnSet(false)
                };

                FilterExpression filter =
                    new FilterExpression(LogicalOperator.Or);

                if (!string.IsNullOrWhiteSpace(email))
                {
                    filter.AddCondition(
                        "hpst_email",
                        ConditionOperator.Equal,
                        email.Trim());
                }

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    filter.AddCondition(
                        "hpst_phone",
                        ConditionOperator.Equal,
                        phone.Trim());
                }

                query.Criteria.AddFilter(filter);

                // Exclude current record during update
                if (target.Id != Guid.Empty)
                {
                    query.Criteria.AddCondition(
                        "hpst_attendeeid",
                        ConditionOperator.NotEqual,
                        target.Id);
                }

                if (service.RetrieveMultiple(query).Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException(
                        "An attendee with the same email or phone number already exists.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(
                    "Error validating attendee record: " + ex.Message);
            }
        }
    }
}
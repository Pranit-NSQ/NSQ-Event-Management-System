using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event_Registration
{
    public class GenerateConfirmationNumberPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (!context.InputParameters.Contains("Target"))
                return;

            Entity target = (Entity)context.InputParameters["Target"];

            if (!target.Contains("hpst_attendee"))
                return;

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            EntityReference attendeeRef =
                target.GetAttributeValue<EntityReference>("hpst_attendee");

            Entity attendee = service.Retrieve(
                attendeeRef.LogicalName,
                attendeeRef.Id,
                new ColumnSet("hpst_name", "hpst_lastname"));

            string firstName =
                attendee.GetAttributeValue<string>("hpst_name") ?? "";

            string lastName =
                attendee.GetAttributeValue<string>("hpst_lastname") ?? "";

            string firstInitial =
                !string.IsNullOrWhiteSpace(firstName)
                    ? firstName.Substring(0, 1).ToUpper()
                    : "X";

            string lastInitial =
                !string.IsNullOrWhiteSpace(lastName)
                    ? lastName.Substring(0, 1).ToUpper()
                    : "X";

            string initials = firstInitial + lastInitial;

            string datePart = DateTime.Now.ToString("ddMM");

            QueryExpression query =
                new QueryExpression("hpst_eventregistration");

            query.ColumnSet = new ColumnSet(false);

            int sequence =
                service.RetrieveMultiple(query).Entities.Count + 1;

            string runningNumber =
                sequence.ToString("D4");

            string confirmationNumber =
                $"{initials}-EN-{datePart}-{runningNumber}";

            target["hpst_confirmationnumber"] =
                confirmationNumber;
        }
    }
}

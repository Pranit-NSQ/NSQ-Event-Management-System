using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event
{
   
    public class GenerateEventCodePlugin : IPlugin
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

                if (target.LogicalName != "hpst_event")
                {
                    return;
                }

                string todayDate =
                    DateTime.Now.ToString("ddMMyy");

                QueryExpression query =
                    new QueryExpression("hpst_event");

                query.ColumnSet =
                    new ColumnSet("hpst_eventcode");

                query.AddOrder(
                    "createdon",
                    OrderType.Descending);

                EntityCollection events =
                    service.RetrieveMultiple(query);

                int nextNumber = 1;

                if (events.Entities.Count > 0)
                {
                    foreach (Entity eventRecord in events.Entities)
                    {
                        if (eventRecord.Contains("hpst_eventcode"))
                        {
                            string lastCode =
                                eventRecord.GetAttributeValue<string>("hpst_eventcode");

                            if (!string.IsNullOrEmpty(lastCode))
                            {
                                string[] parts =
                                    lastCode.Split('-');

                                if (parts.Length == 3)
                                {
                                    int lastNumber;

                                    if (int.TryParse(parts[2], out lastNumber))
                                    {
                                        nextNumber = lastNumber + 1;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                string eventCode =
                    "EN-" + todayDate + "-" + nextNumber.ToString("D3");

                target["hpst_eventcode"] =
                    eventCode;
            }
            catch (Exception)
            {
                throw new InvalidPluginExecutionException(
                    "Error while generating Event Code.");
            }
        }
    }
}
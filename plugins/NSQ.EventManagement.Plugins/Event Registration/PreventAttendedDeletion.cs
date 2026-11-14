using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Event_Registration
{
    public class PreventAttendedDeletion : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            Entity preImage =
                context.PreEntityImages["PreImage"];

            int status =
                preImage.GetAttributeValue<OptionSetValue>("hpst_statuser").Value;

            // Replace with your Attended option value
            if (status == 744890002)
            {
                throw new InvalidPluginExecutionException(
                    "Attended registrations cannot be deleted.");
            }
        }
    }
}

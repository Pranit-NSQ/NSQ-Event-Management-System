using Microsoft.Xrm.Sdk;
using System;
 
namespace NSQ.EventManagement.Plugins.Session
{
    public class SessionSpeakerRequiredPlugin : IPlugin
    {
        // Dataverse table logical name.
        private const string EntityName = "hpst_session";

        // Session column logical names.
        private const string StatusAttribute = "hpst_status";
        private const string SpeakerAttribute = "hpst_speaker";

        // Session Status choice values.
        private const int PlannedStatus = 744890001;
        private const int CompletedStatus = 744890002;

        public void Execute(IServiceProvider serviceProvider)
        {
            // Get the plugin execution context.
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            // Get the Target entity containing the current operation's data.
            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity))
            {
                return;
            }

            Entity target = (Entity)context.InputParameters["Target"];

            // Ensure this plugin is processing the Session table.
            if (target.LogicalName != EntityName)
            {
                return;
            }

            // Get the previous values for Update operations.
            Entity preImage = null;

            if (context.MessageName == "Update")
            {
                if (!context.PreEntityImages.Contains("PreImage"))
                {
                    throw new InvalidPluginExecutionException(
                        "Session Speaker Required Plugin: Pre Image is missing. " +
                        "Please configure the Pre Image with alias PreImage.");
                }

                preImage = context.PreEntityImages["PreImage"];
            }

            // Get the resulting Status value.
            // On Update, use Target if Status was changed.
            // Otherwise, use the previous Status from Pre Image.
            OptionSetValue status =
                target.Contains(StatusAttribute)
                    ? target.GetAttributeValue<OptionSetValue>(StatusAttribute)
                    : (preImage != null
                        ? preImage.GetAttributeValue<OptionSetValue>(StatusAttribute)
                        : null);

            // Get the resulting Speaker lookup value.
            // On Update, use Target if Speaker was changed.
            // Otherwise, use the previous Speaker from Pre Image.
            EntityReference speaker =
                target.Contains(SpeakerAttribute)
                    ? target.GetAttributeValue<EntityReference>(SpeakerAttribute)
                    : (preImage != null
                        ? preImage.GetAttributeValue<EntityReference>(SpeakerAttribute)
                        : null);

            // Check whether the Session Status is Planned or Completed.
            bool requiresSpeaker =
                status != null &&
                (status.Value == PlannedStatus ||
                 status.Value == CompletedStatus);

            // Block saving if the required Speaker is missing.
            if (requiresSpeaker && speaker == null)
            {
                throw new InvalidPluginExecutionException(
                    "A Speaker must be assigned before a Session can be marked as Planned or Completed.");
            }
        }
    }
}

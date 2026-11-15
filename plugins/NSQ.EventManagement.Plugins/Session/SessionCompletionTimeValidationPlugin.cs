using Microsoft.Xrm.Sdk;
using System;
using System.Security.Policy;

namespace NSQ.EventManagement.Plugins.Session
{
    public class SessionCompletionTimeValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                if (!(context.InputParameters["Target"] is Entity target)) return;

                Entity preImage = context.PreEntityImages.Contains("PreImage")
                    ? context.PreEntityImages["PreImage"] : null;

                // On Update, Target holds only changed columns, so fall back to the pre-image
                var status = PluginHelper.GetValue<OptionSetValue>(target, preImage, SessionConstants.StatusField);
                var end = PluginHelper.GetValue<DateTime?>(target, preImage, SessionConstants.EndTimeField);

                if (status == null || end == null) return;
                if (status.Value != SessionConstants.StatusCompleted) return;

                tracing.Trace("Completion check. UtcNow: " + DateTime.UtcNow + " | End: " + end.Value);

                // Dataverse DateTime values reach plugins in UTC
                if (DateTime.UtcNow < end.Value)
                {
                    throw new InvalidPluginExecutionException(
                        "A session cannot be marked as Completed before its End Time.");
                }
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                tracing.Trace("SessionCompletionTimeValidationPlugin error: " + ex);
                throw new InvalidPluginExecutionException("An error occurred while validating session completion.", ex);
            }
        }
    }
}
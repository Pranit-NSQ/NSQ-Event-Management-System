using System;
using Microsoft.Xrm.Sdk;

namespace NSQ.EventManagement.Plugins.Speaker

{
    public class SpeakerContactDetailRequiredPlugin : IPlugin

    {
        public void Execute(IServiceProvider serviceProvider)

        {
            // Get the plugin execution context.

            IPluginExecutionContext context =

                (IPluginExecutionContext)serviceProvider.GetService(

                    typeof(IPluginExecutionContext));

            // Execute only for the Speaker table.

            if (context.PrimaryEntityName != "hpst_speaker")

                return;

            // Execute only for Create and Update operations.

            if (context.MessageName != "Create" &&

                context.MessageName != "Update")

                return;

            // Retrieve the record being created or updated.

            Entity target =

                (Entity)context.InputParameters["Target"];

            // PreImage is required during Update to retrieve

            // contact values that were not included in Target.

            Entity preImage = null;

            if (context.MessageName == "Update")

            {
                if (!context.PreEntityImages.Contains("PreImage"))

                {
                    throw new InvalidPluginExecutionException(

                        "Speaker Contact Detail plugin requires a PreImage.");
                }

                preImage = context.PreEntityImages["PreImage"];
            }

            // Get the effective Email and Phone values.

            // Target values take priority over PreImage values.

            string email = GetCurrentValue(

                target, preImage, "hpst_email");

            string phone = GetCurrentValue(

                target, preImage, "hpst_phonenumber");

            // Check whether both contact details are empty.

            bool isEmailEmpty =

                string.IsNullOrWhiteSpace(email);

            bool isPhoneEmpty =

                string.IsNullOrWhiteSpace(phone);

            // Prevent saving if both Email and Phone are missing.

            if (isEmailEmpty && isPhoneEmpty)

            {
                throw new InvalidPluginExecutionException(

                    "Please provide at least one contact detail: Email or Phone Number.");
            }
        }

        // Retrieves the latest value from Target or PreImage.

        private string GetCurrentValue(

            Entity target,

            Entity preImage,

            string attributeName)

        {
            // If the attribute is included in Target,

            // use its current value.

            if (target.Contains(attributeName))

            {
                return target.GetAttributeValue<string>(attributeName);
            }

            // Otherwise, retrieve the existing value from PreImage.

            if (preImage != null)

            {
                return preImage.GetAttributeValue<string>(attributeName);
            }

            // Return null if the attribute has no value.

            return null;
        }
    }
}
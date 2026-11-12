using System;

using Microsoft.Xrm.Sdk;

using Microsoft.Xrm.Sdk.Query;

namespace NSQ.EventManagement.Plugins.Speaker

{
    public class SpeakerLinkedInUniquenessPlugin5 : IPlugin

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

            // Get the organization service.

            IOrganizationServiceFactory serviceFactory =

                (IOrganizationServiceFactory)serviceProvider.GetService(

                    typeof(IOrganizationServiceFactory));

            IOrganizationService service =

                serviceFactory.CreateOrganizationService(context.UserId);

            // Retrieve the previous LinkedIn value during Update.

            Entity preImage = null;

            if (context.MessageName == "Update")

            {
                if (!context.PreEntityImages.Contains("PreImage"))

                {
                    throw new InvalidPluginExecutionException(

                        "Speaker LinkedIn Uniqueness plugin requires a PreImage.");
                }

                preImage = context.PreEntityImages["PreImage"];
            }

            // Get the effective LinkedIn Profile value.

            // Target takes priority over PreImage.

            string linkedInProfile = GetCurrentValue(

                target, preImage, "hpst_linkedinprofile");

            // An empty LinkedIn Profile is allowed.

            if (string.IsNullOrWhiteSpace(linkedInProfile))

                return;

            // Check whether another Speaker already uses this URL.

            if (IsDuplicateLinkedInProfile(

                service,

                linkedInProfile,

                context.PrimaryEntityId,

                context.MessageName))

            {
                throw new InvalidPluginExecutionException(

                    "This LinkedIn Profile is already associated with another Speaker.");
            }
        }

        // Retrieves the latest value from Target or PreImage.

        private string GetCurrentValue(

            Entity target,

            Entity preImage,

            string attributeName)

        {
            // If the attribute is present in Target,

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

            return null;
        }

        // Checks whether another Speaker uses the same LinkedIn URL.

        private bool IsDuplicateLinkedInProfile(

            IOrganizationService service,

            string linkedInProfile,

            Guid currentSpeakerId,

            string messageName)

        {
            // Create a query for the Speaker table.

            QueryExpression query =

                new QueryExpression("hpst_speaker");

            query.ColumnSet = new ColumnSet(false);

            query.TopCount = 1;

            // Find a Speaker with the same LinkedIn Profile.

            query.Criteria.AddCondition(

                "hpst_linkedinprofile",

                ConditionOperator.Equal,

                linkedInProfile);

            // During Update, exclude the current Speaker record.

            if (messageName == "Update")

            {
                query.Criteria.AddCondition(

                    "hpst_speakerid",

                    ConditionOperator.NotEqual,

                    currentSpeakerId);
            }

            // Retrieve matching Speaker records.

            EntityCollection results =

                service.RetrieveMultiple(query);

            // Return true if another Speaker already has this URL.

            return results.Entities.Count > 0;
        }
    }
}
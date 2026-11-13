using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace NSQ.EventManagement.Plugins.Speaker
{

    public class SpeakerDuplicateEmailPhonePlugin1 : IPlugin

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

            // Retrieve the Target record.

            Entity target = (Entity)context.InputParameters["Target"];

            // Get the Dataverse organization service.

            IOrganizationServiceFactory serviceFactory =

                (IOrganizationServiceFactory)serviceProvider.GetService(

                    typeof(IOrganizationServiceFactory));

            IOrganizationService service =

                serviceFactory.CreateOrganizationService(context.UserId);

            // Retrieve the previous values during Update.

            Entity preImage = null;

            if (context.MessageName == "Update")

            {

                if (!context.PreEntityImages.Contains("PreImage"))

                {

                    throw new InvalidPluginExecutionException(

                        "Speaker plugin requires a PreImage.");

                }

                preImage = context.PreEntityImages["PreImage"];

            }

            // Get the current Email and Phone values.

            string email = GetCurrentValue(

                target, preImage, "hpst_email");

            string phone = GetCurrentValue(

                target, preImage, "hpst_phonenumber");

            // Get the current Speaker ID to exclude it during Update.

            Guid currentSpeakerId = target.Id;

            // Check whether another Speaker has the same Email.

            if (!string.IsNullOrWhiteSpace(email))

            {

                if (IsDuplicate(

                    service, "hpst_email", email,

                    currentSpeakerId, context.MessageName))

                {

                    throw new InvalidPluginExecutionException(

                        "A Speaker with the same Email already exists.");

                }

            }

            // Check whether another Speaker has the same Phone Number.

            if (!string.IsNullOrWhiteSpace(phone))

            {

                if (IsDuplicate(

                    service, "hpst_phonenumber", phone,

                    currentSpeakerId, context.MessageName))

                {

                    throw new InvalidPluginExecutionException(

                        "A Speaker with the same Phone Number already exists.");

                }

            }

        }

        // Gets the latest value from Target or PreImage.

        private string GetCurrentValue(

            Entity target,

            Entity preImage,

            string attributeName)

        {

            if (target.Contains(attributeName))

            {

                return target.GetAttributeValue<string>(attributeName);

            }

            if (preImage != null)

            {

                return preImage.GetAttributeValue<string>(attributeName);

            }

            return null;

        }

        // Checks whether another Speaker already has this value.

        private bool IsDuplicate(

            IOrganizationService service,

            string attributeName,

            string value,

            Guid currentSpeakerId,

            string messageName)

        {

            QueryExpression query =

                new QueryExpression("hpst_speaker");

            query.ColumnSet = new ColumnSet(false);

            query.TopCount = 1;

            // Match the Email or Phone Number.

            query.Criteria.AddCondition(

                attributeName,

                ConditionOperator.Equal,

                value);

            // During Update, exclude the current Speaker record.

            if (messageName == "Update")

            {

                query.Criteria.AddCondition(

                    "hpst_speakerid",

                    ConditionOperator.NotEqual,

                    currentSpeakerId);

            }

            EntityCollection results =

                service.RetrieveMultiple(query);

            return results.Entities.Count > 0;

        }

    }

}


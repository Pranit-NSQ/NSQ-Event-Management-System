using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace NSQ.EventManagement.Plugins.Session
{
    public class SessionOverlapValidationPlugin : IPlugin

    {
        private const string SessionEntity = "hpst_session";

        public void Execute(IServiceProvider serviceProvider)

        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var service = factory.CreateOrganizationService(context.UserId);

            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try

            {
                if (!(context.InputParameters["Target"] is Entity target)) return;

                Entity preImage = context.PreEntityImages.Contains("PreImage")

                    ? context.PreEntityImages["PreImage"] : null;

                // On Update, Target holds only changed columns, so fall back to the pre-image

                var eventRef = GetValue<EntityReference>(target, preImage, "hpst_event");

                var start = GetValue<DateTime?>(target, preImage, "hpst_starttime");

                var end = GetValue<DateTime?>(target, preImage, "hpst_endtime");

                if (eventRef == null || start == null || end == null) return;

                var query = new QueryExpression(SessionEntity)

                {
                    ColumnSet = new ColumnSet("hpst_sessiontitle"),

                    TopCount = 1
                };

                query.Criteria.AddCondition("hpst_event", ConditionOperator.Equal, eventRef.Id);

                query.Criteria.AddCondition("hpst_starttime", ConditionOperator.LessThan, end.Value);

                query.Criteria.AddCondition("hpst_endtime", ConditionOperator.GreaterThan, start.Value);

                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                if (context.MessageName == "Update")

                    query.Criteria.AddCondition("hpst_sessionid", ConditionOperator.NotEqual, context.PrimaryEntityId);

                var result = service.RetrieveMultiple(query);

                if (result.Entities.Count > 0)

                {
                    string title = result.Entities[0].GetAttributeValue<string>("hpst_sessiontitle");

                    throw new InvalidPluginExecutionException(

                        $"This session overlaps with the session '{title}' in the same event. Please choose a different time.");
                }
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)

            {
                tracing.Trace("SessionOverlapValidationPlugin error: " + ex);

                throw new InvalidPluginExecutionException("An error occurred while validating session timing.", ex);
            }
        }

        private static T GetValue<T>(Entity target, Entity image, string attribute)

        {
            if (target.Contains(attribute)) return target.GetAttributeValue<T>(attribute);

            if (image != null && image.Contains(attribute)) return image.GetAttributeValue<T>(attribute);

            return default(T);
        }
    }
}
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NSQ.EventManagement.Plugins.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQ.EventManagement.Plugins.Session
{
    public class SessionSpeakerConflictValidationPlugin : IPlugin

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

                var speakerRef = PluginHelper.GetValue<EntityReference>(target, preImage, "hpst_speaker");

                var start = PluginHelper.GetValue<DateTime?>(target, preImage, "hpst_starttime");

                var end = PluginHelper.GetValue<DateTime?>(target, preImage, "hpst_endtime");

                // Nothing to check without a speaker or complete timing

                if (speakerRef == null || start == null || end == null) return;

                var query = new QueryExpression(SessionEntity)

                {
                    ColumnSet = new ColumnSet("hpst_sessiontitle"),

                    TopCount = 1
                };

                query.Criteria.AddCondition("hpst_speaker", ConditionOperator.Equal, speakerRef.Id);

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

                        $"This speaker is already assigned to the overlapping session '{title}'. Please choose a different speaker or time.");
                }
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)

            {
                tracing.Trace("SessionSpeakerConflictValidationPlugin error: " + ex);

                throw new InvalidPluginExecutionException("An error occurred while validating speaker availability.", ex);
            }
        }
    }
}
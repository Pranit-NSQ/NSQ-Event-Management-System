using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NSQ.EventManagement.Plugins.Session
{
    internal static class PluginHelper

    {
        // On Update, Target holds only the changed columns, so fall back to the pre-image

        public static T GetValue<T>(Entity target, Entity image, string attribute)

        {
            if (target.Contains(attribute)) return target.GetAttributeValue<T>(attribute);

            if (image != null && image.Contains(attribute)) return image.GetAttributeValue<T>(attribute);

            return default(T);
        }
    }
}
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace ContosoUniversity.Infrastructure.Services
{
    public class CustomMartenContractResolver : DefaultContractResolver
    {
        private readonly IList<string> _propsToIgnore = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propsToIgnore">e.g. "UncommitedEvents".</param>
        public CustomMartenContractResolver(string[] propsToIgnore)
        {
            _propsToIgnore = propsToIgnore;
        }

        public CustomMartenContractResolver()
        {
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            // Make property member with private set method writable, so it can be deserialized.
            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    bool shouldIgnoreProperty = _propsToIgnore.IndexOf(property.Name) != -1;
                    if (shouldIgnoreProperty)
                    {
                        prop.Writable = false;
                        prop.Readable = false;
                    }
                    else
                    {
                        bool hasPrivateSetter = property.GetSetMethod(true) != null;
                        prop.Writable = hasPrivateSetter;
                    }
                }
            }

            return prop;
        }
    }

}

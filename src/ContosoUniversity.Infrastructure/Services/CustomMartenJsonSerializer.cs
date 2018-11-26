using Marten;
using Marten.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ContosoUniversity.Infrastructure.Services
{
    /// <summary>
    /// Json serializer used when serializing objects to Marten.
    /// </summary>
    public class CustomMartenJsonSerializer : JsonNetSerializer
    {
        public static readonly EnumStorage DefaultEnumStorage = EnumStorage.AsInteger;

        /// <summary>
        /// Uses HarcMartenContractResolver and ignores 'DomainEvents' property name from serialization.
        /// </summary>
        public CustomMartenJsonSerializer() : this(new CustomMartenContractResolver(new[] { "DomainEvents" }))
        {
        }

        /// <summary>
        /// Inject custom IContractResolver. Default is MartenContractResolver.
        /// </summary>
        /// <param name="customContractResolver"></param>
        public CustomMartenJsonSerializer(IContractResolver customContractResolver)
        {
            Customize(config =>
            {
                config.ContractResolver = customContractResolver;
                config.TypeNameHandling = TypeNameHandling.Auto;
                config.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                config.DateParseHandling = DateParseHandling.None;
                config.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
                config.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
            });
            EnumStorage = DefaultEnumStorage;
        }
    }
}



using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonPact.NewtonSoft {

    /// <inheritdoc />
    public class JsonOptions : JsonSerializerSettings {

        /// <summary>
        /// <inheritdoc cref="JsonSerializerSettings"/>
        /// </summary>
        /// <param name="casing"><see cref="JsonPactCase"/></param>
        /// <param name="settings"><see cref="JsonSerializerSettings"/></param>
        /// <exception cref="ArgumentException">Occurs on attempted override of required setting value.</exception>
        internal JsonOptions(JsonPactCase casing, JsonSerializerSettings settings) {
            // Set immutable required defaults.
            base.NullValueHandling = NullValueHandling.Ignore;
            base.DefaultValueHandling = DefaultValueHandling.Populate;
            base.ContractResolver = new JsonPactAttributesResolver {
                NamingStrategy = casing.IntoNamingStrategy()
            };

            // Copy over the remaining customizable values.
            Context = settings.Context;
            Converters = settings.Converters;
            Culture = settings.Culture;
            Error = settings.Error;
            Formatting = settings.Formatting;
            ConstructorHandling = settings.ConstructorHandling;
            EqualityComparer = settings.EqualityComparer;
            MaxDepth = settings.MaxDepth;
            SerializationBinder = settings.SerializationBinder;
            TraceWriter = settings.TraceWriter;
            CheckAdditionalContent = settings.CheckAdditionalContent;
            DateFormatHandling = settings.DateFormatHandling;
            DateFormatString = settings.DateFormatString;
            DateParseHandling = settings.DateParseHandling;
            FloatFormatHandling = settings.FloatFormatHandling;
            FloatParseHandling = settings.FloatParseHandling;
            MetadataPropertyHandling = settings.MetadataPropertyHandling;
            MissingMemberHandling = settings.MissingMemberHandling;
            ObjectCreationHandling = settings.ObjectCreationHandling;
            PreserveReferencesHandling = settings.PreserveReferencesHandling;
            ReferenceLoopHandling = settings.ReferenceLoopHandling;
            ReferenceResolverProvider = settings.ReferenceResolverProvider;
            StringEscapeHandling = settings.StringEscapeHandling;
            TypeNameHandling = settings.TypeNameHandling;
            DateTimeZoneHandling = settings.DateTimeZoneHandling;
            TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
        }

        /// <inheritdoc cref="JsonSerializerSettings.NullValueHandling"/>
        public new NullValueHandling NullValueHandling => base.NullValueHandling;

        /// <inheritdoc cref="JsonSerializerSettings.DefaultValueHandling"/>
        public new DefaultValueHandling DefaultValueHandling => base.DefaultValueHandling;

        /// <inheritdoc cref="JsonSerializerSettings.ContractResolver"/>
        public new IContractResolver? ContractResolver => base.ContractResolver;

        /// <summary>
        /// Default <see cref="JsonSerializerSettings"/> for JsonPact.
        /// </summary>
        /// <param name="casing"><see cref="JsonPactCase"/></param>
        /// <returns><see cref="JsonOptions"/></returns>
        public static JsonOptions Default(JsonPactCase casing) => new(casing, new JsonSerializerSettings());

        /// <summary>
        /// Converts Newtonsoft settings into an <see cref="IJsonPact"/>.
        /// </summary>
        /// <returns><see cref="IJsonPact"/></returns>
        public IJsonPact IntoJsonPact() => new Serializer(this);
    }
}

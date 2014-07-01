/* Copyright (c) BeyondTheDuck 2014 */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Fibber
{
    /// <summary>
    /// FibberTypeConfiguration.
    /// </summary>
    public class FibberTypeConfiguration
    {
        internal Dictionary<Type, FibberConfiguration> TypeConfigurations;

        /// <summary>
        /// Obsolete.
        /// </summary>
        [Obsolete("Please use the Build class to configure an instance of FibberEngine.", true)]
        public FibberTypeConfiguration()
        {
            this.TypeConfigurations = new Dictionary<Type, FibberConfiguration>();
        }

        /// <summary>
        /// Register a type.
        /// </summary>
        /// <typeparam name="T">Type to register</typeparam>
        /// <param name="configuration">Action for configuring an instance of FibberConfiguration.</param>
        /// <returns>The configured instance of FibberConfiguration.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Reviewed.")]
        public FibberConfiguration ForType<T>(Action<FibberConfiguration> configuration) where T : class
        {
            if (configuration == null) { throw new ArgumentNullException("configuration"); }

            TypeConfigurations.Add(typeof(T), Create<FibberConfiguration>(typeof(T)));

            configuration(TypeConfigurations[typeof(T)]);

            return Create<FibberConfiguration>(typeof(T));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Reviewed.")]
        private T Create<T>(Type type) where T : FibberConfiguration, new()
        {
            var returnValue = new T()
            {
                Type = type
            };

            return returnValue;
        }
    }
}
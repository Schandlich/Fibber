/* Copyright (c) BeyondTheDuck 2014 */
using System;
using System.Reflection;

namespace Fibber
{
    /// <summary>
    /// Used to fill classes with random data based on the types and generators configured.
    /// </summary>
    public class FibberEngine
    {
        private FibberTypeConfiguration _fibberTypeConfiguration;

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Please use the Build class to configure an instance of FibberEngine.", true)]
        public FibberEngine()
        {
            this._fibberTypeConfiguration = CreateConfig<FibberTypeConfiguration>();
        }

        /// <summary>
        /// Configure type configurations.
        /// </summary>
        /// <param name="configuration">Action for configuring an instance of Fibber.</param>
        /// <returns>A configured instance of Fibber.</returns>
        public FibberEngine Configure(Action<FibberTypeConfiguration> configuration)
        {
            if (configuration == null) { throw new ArgumentNullException("configuration"); }

            configuration(_fibberTypeConfiguration);

            return Create<FibberEngine>();
        }

        /// <summary>
        /// Specify a seed for the random in the Generators class. 
        /// </summary>
        /// <param name="seedValue">Seed for the random in the Generators class.</param>
        /// <returns></returns>
        public FibberEngine Seed(int seedValue)
        {
            Generators.SeedValue = seedValue;

            return Create<FibberEngine>();
        }

        private T Create<T>() where T : FibberEngine, new()
        {
            var returnValue = new T()
            {
                _fibberTypeConfiguration = this._fibberTypeConfiguration,
            };

            return returnValue;
        }

        private static T CreateConfig<T>() where T : FibberTypeConfiguration, new()
        {
            var returnValue = new T();

            return returnValue;
        }

        /// <summary>
        /// Insert data for the type defined according to the generators registered.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="item">The current instance of the item.</param>
        /// <returns>The instance of the item.</returns>
        public T Fib<T>(T item)
        {
            if (_fibberTypeConfiguration.TypeConfigurations.ContainsKey(typeof(T)))
            {
                var t = _fibberTypeConfiguration.TypeConfigurations[typeof(T)];

                if (t != null)
                {
                    foreach (PropertyInfo property in typeof(T).GetProperties())
                    {
                        Type propertyType = property.PropertyType;

                        if (t.TypeGenerators.ContainsKey(propertyType))
                        {
                            dynamic func = t.TypeGenerators[propertyType];

                            if (func != null)
                            {
                                if (func.Generator.GetType() == propertyType)
                                {
                                    property.SetValue(item, func.Generator, null);
                                }
                                else
                                {
                                    var constructedMethod = func.Generator.Method as MethodInfo;

                                    if (constructedMethod != null)
                                    {
                                        if (constructedMethod.GetParameters().Length == 0)
                                        {
                                            property.SetValue(item, constructedMethod.Invoke(null, null), null);
                                        }
                                        else if (constructedMethod.GetParameters().Length == 1)
                                        {
                                            property.SetValue(item, constructedMethod.Invoke(null, new object[] { property.GetValue(item, null) }), null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return item;
        }
    }
}
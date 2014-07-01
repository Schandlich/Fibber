/* Copyright (c) BeyondTheDuck 2014 */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Fibber
{
    /// <summary>
    /// Used to fill classes with random data based on the types and generators configured.
    /// </summary>
    public class FibberEngine : IDisposable
    {
        private FibberTypeConfiguration _fibberTypeConfiguration;
        private static int? _seedValue;
        private static Int16 _maxDepth = 5;

        private ThreadLocal<Random> _randomGen = new ThreadLocal<Random>(() => _seedValue.HasValue ? new Random(_seedValue.Value) : new Random());

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
            _seedValue = seedValue;

            return Create<FibberEngine>();
        }

        /// <summary>
        /// Max depth for classes that have properties with a hierarchical relationship.
        /// </summary>
        /// <param name="depth">Max number of instances per type that will be fibbed.</param>
        /// <returns></returns>
        public FibberEngine MaxDepth(Int16 depth)
        {
            if (depth < 1 || depth > 10) { throw new ArgumentException("depth must be greater than or equal to 1 and less than or equal to 10.", "depth"); }

            _maxDepth = depth;

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
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed.")]
        public T Fib<T>(T item)
        {
            var foundTypeGenerators = new Dictionary<Type, dynamic>();
            var currentDepth = 0;
            var actualDepth = 0;

            if (_fibberTypeConfiguration.TypeConfigurations.ContainsKey(typeof(T)))
            {
                foundTypeGenerators = _fibberTypeConfiguration.TypeConfigurations[typeof(T)].TypeGenerators;
            }
            else
            {
                var newTypeGenerators = new Dictionary<Type, dynamic>();

                dynamic boolExpando = new ExpandoObject();
                boolExpando.Generator = RandGen.Bool;
                newTypeGenerators.Add(typeof(bool), boolExpando);

                dynamic byteExpando = new ExpandoObject();
                byteExpando.Generator = RandGen.Byte;
                newTypeGenerators.Add(typeof(byte), byteExpando);

                dynamic byteArrayExpando = new ExpandoObject();
                byteArrayExpando.Generator = RandGen.ByteArray;
                newTypeGenerators.Add(typeof(byte[]), byteArrayExpando);

                dynamic decimalExpando = new ExpandoObject();
                decimalExpando.Generator = RandGen.Decimal;
                newTypeGenerators.Add(typeof(decimal), decimalExpando);

                dynamic floatExpando = new ExpandoObject();
                floatExpando.Generator = RandGen.Float;
                newTypeGenerators.Add(typeof(float), floatExpando);

                dynamic int16Expando = new ExpandoObject();
                int16Expando.Generator = RandGen.Int16;
                newTypeGenerators.Add(typeof(Int16), int16Expando);

                dynamic int32Expando = new ExpandoObject();
                int32Expando.Generator = RandGen.Int32;
                newTypeGenerators.Add(typeof(Int32), int32Expando);

                dynamic int64Expando = new ExpandoObject();
                int64Expando.Generator = RandGen.Int64;
                newTypeGenerators.Add(typeof(Int64), int64Expando);

                dynamic stringExpando = new ExpandoObject();
                stringExpando.Generator = RandGen.String;
                newTypeGenerators.Add(typeof(string), stringExpando);

                _fibberTypeConfiguration.TypeConfigurations.Add(typeof(T), new FibberConfiguration(typeof(T), newTypeGenerators));

                foundTypeGenerators = _fibberTypeConfiguration.TypeConfigurations[typeof(T)].TypeGenerators;
            }

            if (foundTypeGenerators != null)
            {
                foreach (PropertyInfo property in typeof(T).GetProperties())
                {
                    Type propertyType = property.PropertyType;

                    if (foundTypeGenerators.ContainsKey(propertyType))
                    {
                        dynamic func = foundTypeGenerators[propertyType];

                        if (func != null)
                        {
                            if (func.Generator.GetType() == propertyType)
                            {
                                if (property.CanWrite)
                                {
                                    property.SetValue(item, func.Generator, null);
                                }
                            }
                            else if (func.Generator.GetType() == typeof(RandGen))
                            {
                                RandGen randoms = Enum.ToObject(typeof(RandGen), func.Generator);

                                GenerateRandomValue<T>(item, property, randoms);
                            }
                            else
                            {
                                var constructedMethod = func.Generator.Method as MethodInfo;

                                if (constructedMethod != null)
                                {
                                    if (property.CanWrite)
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
                    else if (propertyType.IsPublic && propertyType.IsEnum)
                    {
                        var enumValues = propertyType.GetEnumValues();
                        var randEnum = enumValues.GetValue(_randomGen.Value.Next(0, enumValues.Length));

                        if (property.CanWrite)
                        {
                            property.SetValue(item, randEnum, null);
                        }
                    }
                    else if (propertyType.IsPublic && !propertyType.IsPrimitive && propertyType.IsClass && !propertyType.IsValueType && !propertyType.IsAbstract)
                    {
                        if (currentDepth >= _maxDepth) { return item; }

                        var construtor = propertyType.GetConstructor(Type.EmptyTypes);

                        if (construtor != null)
                        {
                            var instance = construtor.Invoke(null);

                            if (property.CanWrite)
                            {
                                if (propertyType == typeof(T))
                                {
                                    var result = Fib(instance, propertyType, currentDepth, out actualDepth);

                                    currentDepth = actualDepth;
                                    property.SetValue(item, result, null);
                                }
                                else
                                {
                                    var result = Fib(instance, propertyType, 0, out actualDepth);

                                    property.SetValue(item, result, null);
                                }
                            }
                        }
                    }
                }
            }

            return item;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed.")]
        private T Fib<T>(T item, Type type, int currentDepth, out int actualDepth)
        {
            currentDepth++;
            actualDepth = currentDepth;

            var foundTypeGenerators = new Dictionary<Type, dynamic>();

            if (_fibberTypeConfiguration.TypeConfigurations.ContainsKey(type))
            {
                foundTypeGenerators = _fibberTypeConfiguration.TypeConfigurations[type].TypeGenerators;
            }
            else
            {
                var newTypeGenerators = new Dictionary<Type, dynamic>();

                dynamic boolExpando = new ExpandoObject();
                boolExpando.Generator = RandGen.Bool;
                newTypeGenerators.Add(typeof(bool), boolExpando);

                dynamic byteExpando = new ExpandoObject();
                byteExpando.Generator = RandGen.Byte;
                newTypeGenerators.Add(typeof(byte), byteExpando);

                dynamic byteArrayExpando = new ExpandoObject();
                byteArrayExpando.Generator = RandGen.ByteArray;
                newTypeGenerators.Add(typeof(byte[]), byteArrayExpando);

                dynamic dateTimeExpando = new ExpandoObject();
                dateTimeExpando.Generator = RandGen.DateTime;
                newTypeGenerators.Add(typeof(DateTime), dateTimeExpando);

                dynamic dateTimeOffsetExpando = new ExpandoObject();
                dateTimeOffsetExpando.Generator = RandGen.DateTimeOffset;
                newTypeGenerators.Add(typeof(DateTimeOffset), dateTimeOffsetExpando);

                dynamic decimalExpando = new ExpandoObject();
                decimalExpando.Generator = RandGen.Decimal;
                newTypeGenerators.Add(typeof(decimal), decimalExpando);

                dynamic floatExpando = new ExpandoObject();
                floatExpando.Generator = RandGen.Float;
                newTypeGenerators.Add(typeof(float), floatExpando);

                dynamic int16Expando = new ExpandoObject();
                int16Expando.Generator = RandGen.Int16;
                newTypeGenerators.Add(typeof(Int16), int16Expando);

                dynamic int32Expando = new ExpandoObject();
                int32Expando.Generator = RandGen.Int32;
                newTypeGenerators.Add(typeof(Int32), int32Expando);

                dynamic int64Expando = new ExpandoObject();
                int64Expando.Generator = RandGen.Int64;
                newTypeGenerators.Add(typeof(Int64), int64Expando);

                dynamic stringExpando = new ExpandoObject();
                stringExpando.Generator = RandGen.String;
                newTypeGenerators.Add(typeof(string), stringExpando);

                _fibberTypeConfiguration.TypeConfigurations.Add(type, new FibberConfiguration(type, newTypeGenerators));

                foundTypeGenerators = _fibberTypeConfiguration.TypeConfigurations[type].TypeGenerators;
            }

            if (foundTypeGenerators != null)
            {
                foreach (PropertyInfo property in type.GetProperties())
                {
                    Type propertyType = property.PropertyType;

                    if (foundTypeGenerators.ContainsKey(propertyType))
                    {
                        dynamic func = foundTypeGenerators[propertyType];

                        if (func != null)
                        {
                            if (func.Generator.GetType() == propertyType)
                            {
                                if (property.CanWrite)
                                {
                                    property.SetValue(item, func.Generator, null);
                                }
                            }
                            else if (func.Generator.GetType() == typeof(RandGen))
                            {
                                RandGen randoms = Enum.ToObject(typeof(RandGen), func.Generator);

                                GenerateRandomValue<T>(item, property, randoms);
                            }
                            else
                            {
                                var constructedMethod = func.Generator.Method as MethodInfo;

                                if (constructedMethod != null)
                                {
                                    if (property.CanWrite)
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
                    else if (propertyType.IsPublic && !propertyType.IsPrimitive && propertyType.IsClass && !propertyType.IsValueType && !propertyType.IsAbstract)
                    {
                        if (currentDepth >= _maxDepth) { return item; }

                        var construtor = propertyType.GetConstructor(Type.EmptyTypes);

                        if (construtor != null)
                        {
                            var instance = construtor.Invoke(null);

                            var result = Fib(instance, propertyType, currentDepth, out actualDepth);

                            if (property.CanWrite)
                            {
                                property.SetValue(item, result, null);
                            }
                        }
                    }
                }
            }

            return item;
        }

        private void GenerateRandomValue<T>(T item, PropertyInfo property, RandGen randoms)
        {
            if (property.CanWrite)
            {
                switch (randoms)
                {
                    case RandGen.Bool:
                        {
                            var value = this.GenerateBool();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.Byte:
                        {
                            var value = this.GenerateByte();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.ByteArray:
                        {
                            var value = this.GenerateByteArray();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.DateTime:
                        {
                            var value = this.GenerateDateTime();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.DateTimeOffset:
                        {
                            var value = this.GenerateDateTimeOffset();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.Decimal:
                        {
                            var value = this.GenerateDecimal();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.Float:
                        {
                            var value = this.GenerateFloat();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.Int16:
                        {
                            var value = this.GenerateInt16();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.Int32:
                        {
                            var value = this.GenerateInt32();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.Int64:
                        {
                            var value = this.GenerateInt64();
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.ShortString:
                        {
                            var value = this.GenerateString(1, 31, true);
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.ShortStringNoSpaces:
                        {
                            var value = this.GenerateString(1, 31, false);
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.String:
                        {
                            var value = this.GenerateString(31, 61, true);
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.StringNoSpaces:
                        {
                            var value = this.GenerateString(31, 61, false);
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.LargeString:
                        {
                            var value = this.GenerateString(61, 256, true);
                            property.SetValue(item, value, null);
                        }

                        break;
                    case RandGen.LargeStringNoSpaces:
                        {
                            var value = this.GenerateString(61, 256, false);
                            property.SetValue(item, value, null);
                        }

                        break;
                }
            }
        }

        private bool GenerateBool()
        {
            return Convert.ToBoolean(_randomGen.Value.Next(2));
        }

        private byte GenerateByte()
        {
            var returnValue = new byte[1];

            _randomGen.Value.NextBytes(returnValue);

            return returnValue[0];
        }

        private byte[] GenerateByteArray()
        {
            var size = _randomGen.Value.Next(1, 128);
            var returnValue = new byte[size];

            _randomGen.Value.NextBytes(returnValue);

            return returnValue;
        }

        private DateTime GenerateDateTime()
        {
            var returnValue = DateTime.Now;

            var seconds = _randomGen.Value.Next(-17776000, 17776000);

            return returnValue.AddSeconds(seconds);
        }

        private DateTimeOffset GenerateDateTimeOffset()
        {
            var returnValue = DateTimeOffset.Now;

            var seconds = _randomGen.Value.Next(-17776000, 17776000);

            return returnValue.AddSeconds(seconds);
        }

        private decimal GenerateDecimal()
        {
            byte scale = (byte)_randomGen.Value.Next(29);
            bool sign = _randomGen.Value.Next(2) == 1;
            var returnValue = new decimal(_randomGen.Value.Next(), _randomGen.Value.Next(), _randomGen.Value.Next(), sign, scale);

            return Math.Abs(returnValue);
        }

        private float GenerateFloat()
        {
            var returnValue = ((float)_randomGen.Value.Next() / 2147483648);

            return returnValue;
        }

        private Int16 GenerateInt16()
        {
            var result = new byte[sizeof(Int16)];

            _randomGen.Value.NextBytes(result);

            var returnValue = Math.Abs(BitConverter.ToInt16(result, 0));

            return returnValue;
        }

        private Int32 GenerateInt32()
        {
            return _randomGen.Value.Next();
        }

        private Int64 GenerateInt64()
        {
            var result = new byte[sizeof(Int64)];

            _randomGen.Value.NextBytes(result);

            var returnValue = Math.Abs(BitConverter.ToInt64(result, 0));

            return returnValue;
        }

        private string GenerateString(int minLength, int maxLength, bool includeSpaces)
        {
            var length = _randomGen.Value.Next(minLength, maxLength);
            var returnValue = new byte[length];

            if (includeSpaces)
            {
                for (int i = 0; i < length; i++)
                {
                    var randomByte = (byte)_randomGen.Value.Next(54, 123);

                    if (randomByte < 65)
                    {
                        returnValue[i] = (byte)32;
                    }
                    else
                    {
                        returnValue[i] = randomByte;
                    }
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    returnValue[i] = (byte)_randomGen.Value.Next(65, 123);
                }
            }

            var encoding = new ASCIIEncoding();

            return encoding.GetString(returnValue);
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposing.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_randomGen != null && _randomGen.IsValueCreated && _randomGen.Value != null)
            {
                _randomGen.Dispose();
            }
        }

        #endregion
    }
}
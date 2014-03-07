/* Copyright (c) BeyondTheDuck 2014 */
using System;
using System.Diagnostics.CodeAnalysis;
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

        private ThreadLocal<Random> _randomGen = new ThreadLocal<Random>(() => _seedValue.HasValue ? new Random(_seedValue.Value) : new Random());

        private Lazy<string[]> _alphabet = new Lazy<string[]>(() =>
        {
            return new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " " };
        });

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

        private void GenerateRandomValue<T>(T item, PropertyInfo property, RandGen randoms)
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
            var returnValue = new StringBuilder();
            var alphabetRange = includeSpaces ? 64 : 52;

            var length = _randomGen.Value.Next(minLength, maxLength);

            for (int i = 0; i < length; i++)
            {
                returnValue.Append(_alphabet.Value[Convert.ToInt32(Math.Floor(alphabetRange * _randomGen.Value.NextDouble()))]);
            }

            return returnValue.ToString();
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
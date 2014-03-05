/* Copyright (c) BeyondTheDuck 2014 */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace Fibber
{
    /// <summary>
    /// FibberConfiguration
    /// </summary>
    public class FibberConfiguration
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Reviewed.")]
        internal Type Type;
        internal Dictionary<Type, dynamic> TypeGenerators = new Dictionary<Type, dynamic>();

        /// <summary>
        /// Obsolete.
        /// </summary>
        [Obsolete("Please use the FibberConfiguration class to configure an instance of Fibber.", true)]
        public FibberConfiguration() { }

        /// <summary>
        /// Register a generator for T.
        /// </summary>
        /// <typeparam name="T">The type the generator is registered to.</typeparam>
        /// <param name="func">Func to be executed when generating values for properties of type T. T1 is the currently existing value.</param>
        /// <returns></returns>
        public FibberConfiguration For<T>(Func<T, T> func)
        {
            if (TypeGenerators.ContainsKey(typeof(T))) { throw new ArgumentException(string.Format("There is already a generator registered for type: {0}.", typeof(T).ToString())); }

            dynamic expando = new ExpandoObject();
            expando.Generator = func;

            TypeGenerators.Add(typeof(T), expando);

            return this;
        }

        /// <summary>
        /// Register a generator for T.
        /// </summary>
        /// <typeparam name="T">The type the generator is registered to.</typeparam>
        /// <param name="func">Func to be executed when generating values for properties of T.</param>
        /// <returns></returns>
        public FibberConfiguration For<T>(Func<T> func)
        {
            if (TypeGenerators.ContainsKey(typeof(T))) { throw new ArgumentException(string.Format("There is already a generator registered for type: {0}.", typeof(T).ToString())); }

            dynamic expando = new ExpandoObject();
            expando.Generator = func;

            TypeGenerators.Add(typeof(T), expando);

            return this;
        }

        /// <summary>
        /// Register a generator for T.
        /// </summary>
        /// <typeparam name="T">The type the generator is registered to.</typeparam>
        /// <param name="value">The static value that will be set when generating values for properties of T.</param>
        /// <returns></returns>
        public FibberConfiguration For<T>(T value)
        {
            if (TypeGenerators.ContainsKey(typeof(T))) { throw new ArgumentException(string.Format("There is already a generator registered for type: {0}.", typeof(T).ToString())); }

            dynamic expando = new ExpandoObject();
            expando.Generator = value;

            TypeGenerators.Add(typeof(T), expando);

            return this;
        }

        /// <summary>
        /// Uses the built in Generators methods for all properties of type bool, byte, byte[], decimal, float, int, int64, int16, and string.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reviewed.")]
        public void UseDefaults()
        {
            dynamic boolExpando = new ExpandoObject();
            boolExpando.Generator = new Func<bool>(() => Generators.Current.Bool());
            TypeGenerators.Add(typeof(bool), boolExpando);

            dynamic byteExpando = new ExpandoObject();
            byteExpando.Generator = new Func<byte>(() => Generators.Current.Byte());
            TypeGenerators.Add(typeof(byte), byteExpando);

            dynamic byteArrayExpando = new ExpandoObject();
            byteArrayExpando.Generator = new Func<byte[]>(() => Generators.Current.ByteArray());
            TypeGenerators.Add(typeof(byte[]), byteArrayExpando);

            dynamic decimalExpando = new ExpandoObject();
            decimalExpando.Generator = new Func<decimal>(() => Generators.Current.Decimal());
            TypeGenerators.Add(typeof(decimal), decimalExpando);

            dynamic floatExpando = new ExpandoObject();
            floatExpando.Generator = new Func<float>(() => Generators.Current.Float());
            TypeGenerators.Add(typeof(float), floatExpando);

            dynamic intExpando = new ExpandoObject();
            intExpando.Generator = new Func<int>(() => Generators.Current.Int());
            TypeGenerators.Add(typeof(int), intExpando);

            dynamic int64Expando = new ExpandoObject();
            int64Expando.Generator = new Func<Int64>(() => Generators.Current.Int64());
            TypeGenerators.Add(typeof(Int64), int64Expando);

            dynamic int16Expando = new ExpandoObject();
            int16Expando.Generator = new Func<Int16>(() => Generators.Current.Int16());
            TypeGenerators.Add(typeof(Int16), int16Expando);

            dynamic stringExpando = new ExpandoObject();
            stringExpando.Generator = new Func<string>(() => Generators.Current.String());
            TypeGenerators.Add(typeof(string), stringExpando);
        }
    }
}

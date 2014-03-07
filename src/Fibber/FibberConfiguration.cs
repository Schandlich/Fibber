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
        /// Register a generator for T.
        /// </summary>
        /// <typeparam name="T">The type the generator is registered to.</typeparam>
        /// <param name="randomGenerator">Use a random generator to generate a value from one of the listed types described in the enumeration.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Reviewed.")]
        public FibberConfiguration For<T>(RandGen randomGenerator)
        {
            if (TypeGenerators.ContainsKey(typeof(T))) { throw new ArgumentException(string.Format("There is already a generator registered for type: {0}.", typeof(T).ToString())); }

            dynamic expando = new ExpandoObject();

            expando.Generator = randomGenerator;

            TypeGenerators.Add(typeof(T), expando);

            return this;
        }

        /// <summary>
        /// Uses the built in Generators methods for all properties of type bool, byte, byte[], decimal, float, Int16, Int32, Int64, and string (RandGen.String).
        /// </summary>
        public void UseDefaults()
        {
            dynamic boolExpando = new ExpandoObject();
            boolExpando.Generator = RandGen.Bool;
            TypeGenerators.Add(typeof(bool), boolExpando);

            dynamic byteExpando = new ExpandoObject();
            byteExpando.Generator = RandGen.Byte;
            TypeGenerators.Add(typeof(byte), byteExpando);

            dynamic byteArrayExpando = new ExpandoObject();
            byteArrayExpando.Generator = RandGen.ByteArray;
            TypeGenerators.Add(typeof(byte[]), byteArrayExpando);

            dynamic decimalExpando = new ExpandoObject();
            decimalExpando.Generator = RandGen.Decimal;
            TypeGenerators.Add(typeof(decimal), decimalExpando);

            dynamic floatExpando = new ExpandoObject();
            floatExpando.Generator = RandGen.Float;
            TypeGenerators.Add(typeof(float), floatExpando);

            dynamic int16Expando = new ExpandoObject();
            int16Expando.Generator = RandGen.Int16;
            TypeGenerators.Add(typeof(Int16), int16Expando);

            dynamic int32Expando = new ExpandoObject();
            int32Expando.Generator = RandGen.Int32;
            TypeGenerators.Add(typeof(Int32), int32Expando);

            dynamic int64Expando = new ExpandoObject();
            int64Expando.Generator = RandGen.Int64;
            TypeGenerators.Add(typeof(Int64), int64Expando);

            dynamic stringExpando = new ExpandoObject();
            stringExpando.Generator = RandGen.String;
            TypeGenerators.Add(typeof(string), stringExpando);
        }
    }
}

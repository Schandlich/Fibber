/* Copyright (c) BeyondTheDuck 2014 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fibber
{
    /// <summary>
    /// List of random generators
    /// </summary>
    public enum RandGen
    {
        /// <summary>
        /// Generate a random bool.
        /// </summary>
        Bool,

        /// <summary>
        /// Generate a random byte.
        /// </summary>
        Byte,

        /// <summary>
        /// Generate a random byte array with a size between and including 1 and 128.
        /// </summary>
        ByteArray,

        /// <summary>
        /// Generate a random decimal.
        /// </summary>
        Decimal,

        /// <summary>
        /// Generate a random float.
        /// </summary>
        Float,

        /// <summary>
        /// Generate a random, positive Int16,
        /// </summary>
        Int16,

        /// <summary>
        /// Generate a random, positive Int32,
        /// </summary>
        Int32,

        /// <summary>
        /// Generate a random, positive Int64,
        /// </summary>
        Int64,

        /// <summary>
        /// Generate a random string with a length between and including 1 and 30. Including spaces.
        /// </summary>
        ShortString,

        /// <summary>
        /// Generate a random string with a length between and including 1 and 30.
        /// </summary>
        ShortStringNoSpaces,

        /// <summary>
        /// Generate a random string with a length between and including 31 and 60. Including spaces.
        /// </summary>
        String,

        /// <summary>
        /// Generate a random string with a length between and including 31 and 60.
        /// </summary>
        StringNoSpaces,

        /// <summary>
        /// Generate a random string with a length between and including 61 and 255. Including spaces.
        /// </summary>
        LargeString,

        /// <summary>
        /// Generate a random string with a length between and including 61 and 255.
        /// </summary>
        LargeStringNoSpaces
    }
}

/* Copyright (c) BeyondTheDuck 2014 */

namespace Fibber
{
    /// <summary>
    /// Used to create an instance of the Fibber class.
    /// </summary>
    public static class Build
    {
        /// <summary>
        /// Build an instance of the Fibber class.
        /// </summary>
        /// <returns></returns>
        public static FibberEngine NewFibber()
        {
            return Create<FibberEngine>();
        }

        private static T Create<T>() where T : FibberEngine, new()
        {
            var returnValue = new T()
            {
            };

            return returnValue;
        }
    }
}

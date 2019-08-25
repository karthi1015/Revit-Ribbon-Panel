// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParent.cs" company="PMTech">
//   PMTech
// </copyright>
// <summary>
//   Defines the IParent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AddFamilyParameters.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// The Parent interface.
    /// </summary>
    /// <typeparam name="T">
    /// Generic type T
    /// </typeparam>
    public interface IParent<out T>
    {
        /// <summary>
        /// The get children.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        IEnumerable<T> GetChildren();
    }
}
using System;
using System.Collections.Generic;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// A flow, allows to specify several terms as located near each other.
  /// </summary>
  public interface IWeightedTermConstructionFlow
  {
    /// <summary>
    /// Constructed operands.
    /// </summary>
    IDictionary<IWeighableTerm, float?> WeightedOperands { get; }

    /// <summary>
    /// Adds <see cref="ISimpleTerm"/> to collection of operands with no specified weight.
    /// </summary>
    /// <param name="term">Exact word or phrase.</param>
    /// <returns>Instance, ready to keep on constructing operands.</returns>
    IWeightedTermConstructionFlow AndSimpleTerm(string term);

    /// <summary>
    /// Adds <see cref="ISimpleTerm"/> to collection of operands with certain weight.
    /// </summary>
    /// <param name="term">Exact word or phrase.</param>
    /// <param name="weight">Weight of term between 0.0 and 1.0.</param>
    /// <returns>Instance, ready to keep on constructing operands./</returns>
    IWeightedTermConstructionFlow AndSimpleTerm(string term, float weight);

    /// <summary>
    /// Adds <see cref="IPrefixTerm"/> to collection of operands with no specified weight.
    /// </summary>
    /// <param name="prefix">Prefix</param>
    /// <returns>Instance, ready to keep on constructing operands.</returns>
    IWeightedTermConstructionFlow AndPrefixTerm(string prefix);

    /// <summary>
    /// Adds <see cref="IPrefixTerm"/> to collection of operands with certain weight.
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="weight">Weight of term between 0.0 and 1.0.</param>
    /// <returns>Instance, ready to keep on constructing operands.</returns>
    IWeightedTermConstructionFlow AndPrefixTerm(string prefix, float weight);

    /// <summary>
    ///  Adds <see cref="IGenerationTerm"/> to collection of operands with no specified weight.
    /// </summary>
    /// <param name="generationType">Type of variants generation.</param>
    /// <param name="terms">Basis terms for variants generation.</param>
    /// <returns>Instance, ready to keep on constructing operands.</returns>
    IWeightedTermConstructionFlow AndGenerationTerm(GenerationType generationType, ICollection<string> terms);

    /// <summary>
    /// Adds <see cref="IGenerationTerm"/> to collection of operands with certain weight.
    /// </summary>
    /// <param name="generationType">Type of variants generation.</param>
    /// <param name="terms">Basis terms for variants generation.</param>
    /// <param name="weight">Weight of term between 0.0 and 1.0.</param>
    /// <returns>Instance, ready to keep on constructing operands.</returns>
    IWeightedTermConstructionFlow AndGenerationTerm(GenerationType generationType, ICollection<string> terms, float weight);

    /// <summary>
    ///  Adds <see cref="IProximityTerm"/> to collection of operands with no specified weight.
    /// </summary>
    /// <param name="proximityOperandComposer"></param>
    /// <returns>Instance, ready to keep on constructing operands.</returns>
    IWeightedTermConstructionFlow AndProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityOperandComposer);

    /// <summary>
    /// Adds <see cref="IProximityTerm"/> to collection of operands with certain weight.
    /// </summary>
    /// <param name="proximityOperandComposer"></param>
    /// <param name="weight">Weight of term between 0.0 and 1.0.</param>
    /// <returns>Instance, ready to keep on constructing operands.</returns>
    IWeightedTermConstructionFlow AndProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityOperandComposer, float weight);
  }
}
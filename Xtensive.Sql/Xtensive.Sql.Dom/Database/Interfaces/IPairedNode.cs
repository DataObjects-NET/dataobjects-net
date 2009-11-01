// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Describes an object that is paired with some <see cref="PairedNodeCollection{TOwner,TNode}"/>.
  /// </summary>
  public interface IPairedNode<TOwner> where TOwner: Node
  {
    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void UpdatePairedProperty(string property, TOwner value);
  }
}

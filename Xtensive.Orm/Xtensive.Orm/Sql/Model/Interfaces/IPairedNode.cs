// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Model
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

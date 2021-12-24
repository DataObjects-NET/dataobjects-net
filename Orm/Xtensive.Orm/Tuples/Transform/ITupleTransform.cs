// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.04.30

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Interface implemented by any of the tuple transforms.
  /// </summary>
  public interface ITupleTransform
  {
    /// <summary>
    /// Gets <see cref="TupleDescriptor"/> describing the tuples
    /// this transform may produce.
    /// </summary>
    TupleDescriptor Descriptor { get; }

    /// <summary>
    /// Indicates whether transform always produces read-only tuples or not.
    /// </summary>
    bool IsReadOnly { get; }
  }
}
// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;

namespace Xtensive.Tuples.Packed
{
  [Serializable]
  internal struct PackedFieldDescriptor
  {
    public int DataPosition;
    public int StatePosition;

    [NonSerialized]
    public PackedFieldAccessor Accessor;
  }
}
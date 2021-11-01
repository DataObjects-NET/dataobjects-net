// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Collections.Generic;
using Xtensive.Tuples.Transform;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal readonly struct TypeMapping
  {
    public readonly TypeInfo Type;
    public readonly MapTransform KeyTransform;
    public readonly IReadOnlyList<int> KeyIndexes;
    public readonly MapTransform Transform;


    // Constructors

    public TypeMapping(TypeInfo type, MapTransform keyTransform, MapTransform transform, IReadOnlyList<int> keyIndexes)
    {
      Type = type;
      KeyTransform = keyTransform;
      Transform = transform;
      KeyIndexes = keyIndexes;
    }
  }
}
// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  internal readonly struct TypeMetadata
  {
    public int Id { get; }

    public string Name { get; }

    public override string ToString() =>
      string.Format(Strings.MetadataTypeFormat, Name, Id);

    // Constructors

    public TypeMetadata(int id, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");

      Id = id;
      Name = name;
    }

    public TypeMetadata(int id, System.Type type)
    {
      Id = id;
      Name = type.GetFullName();
    }
  }
}

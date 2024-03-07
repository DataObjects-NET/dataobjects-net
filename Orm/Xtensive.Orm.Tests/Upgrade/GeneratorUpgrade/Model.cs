// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.12.10

using System.Text;

namespace Xtensive.Orm.Tests.Upgrade.GeneratorUpgrade
{
  namespace ReferenceModel.Part1
  {
    [HierarchyRoot]
    public class ShortKeyEntityPart1 : Entity
    {
      [Field, Key]
      public short Id { get; private set; }
    }

    [HierarchyRoot]
    public class IntKeyEntityPart1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class LongKeyEntityPart1 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedShortKeyEntityPart1")]
    public class NamedShortKeyEntityPart1 : Entity
    {
      [Field, Key]
      public short Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedIntKeyEntityPart1")]
    public class NamedIntKeyEntityPart1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedLongKeyEntityPart1")]
    public class NamedLongKeyEntityPart1 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }
  }

  namespace ReferenceModel.Part2
  {
    [HierarchyRoot]
    public class ShortKeyEntityPart2 : Entity
    {
      [Field, Key]
      public short Id { get; private set; }
    }

    [HierarchyRoot]
    public class IntKeyEntityPart2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class LongKeyEntityPart2 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedShortKeyEntityPart2")]
    public class NamedShortKeyEntityPart2 : Entity
    {
      [Field, Key]
      public short Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedIntKeyEntityPart2")]
    public class NamedIntKeyEntityPart2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedLongKeyEntityPart2")]
    public class NamedLongKeyEntityPart2 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }
  }

  namespace LessGenerators.Part1
  {
    [HierarchyRoot]
    public class IntKeyEntityPart1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class LongKeyEntityPart1 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedIntKeyEntityPart1")]
    public class NamedIntKeyEntityPart1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedLongKeyEntityPart1")]
    public class NamedLongKeyEntityPart1 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }
  }

  namespace LessGenerators.Part2
  {
    [HierarchyRoot]
    public class IntKeyEntityPart2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class LongKeyEntityPart2 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedIntKeyEntityPart2")]
    public class NamedIntKeyEntityPart2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default, Name = "NamedLongKeyEntityPart2")]
    public class NamedLongKeyEntityPart2 : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }
  }
}
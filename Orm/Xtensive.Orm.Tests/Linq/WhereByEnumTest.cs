// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.WhereByEnumTestModel;

namespace Xtensive.Orm.Tests.Linq.WhereByEnumTestModel
{
  public enum ByteEnum : byte
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = byte.MaxValue
  }

  public enum SByteEnum : sbyte
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = sbyte.MaxValue
  }

  public enum ShortEnum : short
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = short.MaxValue
  }

  public enum UShortEnum : ushort
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = ushort.MaxValue
  }

  public enum IntEnum : int
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = int.MaxValue
  }

  public enum UIntEnum : uint
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = uint.MaxValue
  }

  public enum LongEnum : long
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = long.MaxValue
  }

  public enum ULongEnum : ulong
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Max = ulong.MaxValue
  }

  [HierarchyRoot]
  [Index(nameof(EnumContainer.ByteEnumField))]
  [Index(nameof(EnumContainer.SByteEnumField))]
  [Index(nameof(EnumContainer.ShortEnumField))]
  [Index(nameof(EnumContainer.UShortEnumField))]
  [Index(nameof(EnumContainer.IntEnumField))]
  [Index(nameof(EnumContainer.UIntEnumField))]
  [Index(nameof(EnumContainer.LongEnumField))]
  [Index(nameof(EnumContainer.ULongEnumField))]
  public class EnumContainer : Entity
  {
    public const string Zero = "Zero";
    public const string One = "One";
    public const string Two = "Two";
    public const string Three = "Three";
    public const string Four = "Four";
    public const string Max = "Max";

    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public ByteEnum ByteEnumField { get; set; }

    [Field]
    public SByteEnum SByteEnumField { get; set; }

    [Field]
    public ShortEnum ShortEnumField { get; set; }

    [Field]
    public UShortEnum UShortEnumField { get; set; }

    [Field]
    public IntEnum IntEnumField { get; set; }

    [Field]
    public UIntEnum UIntEnumField { get; set; }

    [Field]
    public LongEnum LongEnumField { get; set; }

    [Field]
    public ULongEnum ULongEnumField { get; set; }

    public EnumContainer(Session session, string enumValue)
      : base(session)
    {
      ByteEnumField = (ByteEnum) Enum.Parse(typeof(ByteEnum), enumValue);
      SByteEnumField = (SByteEnum) Enum.Parse(typeof(SByteEnum), enumValue);
      ShortEnumField = (ShortEnum) Enum.Parse(typeof(ShortEnum), enumValue);
      UShortEnumField = (UShortEnum) Enum.Parse(typeof(UShortEnum), enumValue);
      IntEnumField = (IntEnum) Enum.Parse(typeof(IntEnum), enumValue);
      UIntEnumField = (UIntEnum) Enum.Parse(typeof(UIntEnum), enumValue);
      LongEnumField = (LongEnum) Enum.Parse(typeof(LongEnum), enumValue);
      ULongEnumField = (ULongEnum) Enum.Parse(typeof(ULongEnum), enumValue);
    }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  public class WhereByEnumTest : AutoBuildTest
  {
    private Session SharedSession;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
      SharedSession = Session.Current;
    }

    public override void TestFixtureTearDown()
    {
      SharedSession = null;
      base.TestFixtureTearDown();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(EnumContainer));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new EnumContainer(session, EnumContainer.Zero);
        _ = new EnumContainer(session, EnumContainer.One);
        _ = new EnumContainer(session, EnumContainer.Two);
        _ = new EnumContainer(session, EnumContainer.Three);
        _ = new EnumContainer(session, EnumContainer.Four);
        _ = new EnumContainer(session, EnumContainer.Max);
        tx.Complete();
      }
    }

    [Test]
    public void ByteTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField == ByteEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField == ByteEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField == ByteEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField == ByteEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField == ByteEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ByteEnumField == ByteEnum.Max).ToArray(1);
      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    [Test]
    public void SByteTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField == SByteEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField == SByteEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField == SByteEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField == SByteEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField == SByteEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.SByteEnumField == SByteEnum.Max).ToArray(1);
      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    [Test]
    public void ShortTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ShortEnumField == ShortEnum.Max).ToArray(1);
      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    [Test]
    public void UShortTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;

      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UShortEnumField == UShortEnum.Max).ToArray(1);

      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    [Test]
    public void IntTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;

      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField == IntEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField == IntEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField == IntEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField == IntEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField == IntEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.IntEnumField == IntEnum.Max).ToArray(1);

      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    [Test]
    public void UIntTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;

      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField == UIntEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField == UIntEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField == UIntEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField == UIntEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField == UIntEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.UIntEnumField == UIntEnum.Max).ToArray(1);

      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    [Test]
    public void LongTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;

      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField == LongEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField == LongEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField == LongEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField == LongEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField == LongEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.LongEnumField == LongEnum.Max).ToArray(1);

      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    [Test]
    public void ULontTest()
    {
      SharedSession.Events.DbCommandExecuting += PrintCommandToConsole;

      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField == ULongEnum.Zero).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField == ULongEnum.One).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField == ULongEnum.Two).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField == ULongEnum.Three).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField == ULongEnum.Four).ToArray(1);
      _ = SharedSession.Query.All<EnumContainer>().Where(e => e.ULongEnumField == ULongEnum.Max).ToArray(1);

      SharedSession.Events.DbCommandExecuting -= PrintCommandToConsole;
    }

    private static void PrintCommandToConsole(object sender, DbCommandEventArgs args) => Console.WriteLine(args.Command.CommandText);
  }
}

// Copyright (C) 2020-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.03.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Issues.IssueJira0786_AggregatesProblem
{
  public abstract class AggregatesProblemTestBase : AutoBuildTest
  {
    protected decimal FloatValueAccuracy { get; private set; } = 0.000001m;
    protected decimal DoubleValueAccuracy { get; private set; } = 0.00000000000001m;
    protected decimal DecimalValueAccuracy { get; private set; } = 0.00000000000000001m;

    protected override bool InitGlobalSession => true;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(TestEntity).Assembly, typeof(TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql);

    protected override void PopulateData()
    {
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.PostgreSql)) {
        // We have to change some scales of result values of average in SQL
        // because PostgreSQL does not provide ways to read raw value and reduce it
        // to fit into .NET decimal value, as we reduce scale for values in MS SQL Server provider.

        FloatValueAccuracy = 0.00001m;
        DoubleValueAccuracy = 0.00001m;
        DecimalValueAccuracy = 0.00001m;
      }

      _ = new TestEntity(GlobalSession) {
        ByteValue = 2,
        SByteValue = 4,
        ShortValue = 8,
        UShortValue = 9,
        IntValue = 10,
        UIntValue = 11,
        LongValue = 20,
        ULongValue = 25,
        FloatValue = 0.1f,
        DecimalValue = 1.2m,
        DoubleValue1 = 2.0,
        DoubleValue2 = 3.0,
        NullableByteValue = 2,
        NullableSByteValue = 4,
        NullableShortValue = 8,
        NullableUShortValue = 9,
        NullableIntValue = 30,
        NullableUIntValue = 31,
        NullableLongValue = 40,
        NullableULongValue = 45,
        NullableFloatValue = 0.4f,
        NullableDecimalValue = 4.2m,
        NullableDoubleValue1 = 5.0,
        NullableDoubleValue2 = 6.0
      };
      _ = new TestEntity(GlobalSession) {
        ByteValue = 3,
        SByteValue = 5,
        ShortValue = 9,
        UShortValue = 10,
        IntValue = 11,
        UIntValue = 12,
        LongValue = 21,
        ULongValue = 26,
        FloatValue = 0.2f,
        DecimalValue = 1.3m,
        DoubleValue1 = 2.1,
        DoubleValue2 = 3.1,
        NullableByteValue = 3,
        NullableSByteValue = 5,
        NullableShortValue = 9,
        NullableUShortValue = 10,
        NullableIntValue = 31,
        NullableUIntValue = 32,
        NullableLongValue = 41,
        NullableULongValue = 46,
        NullableFloatValue = 0.5f,
        NullableDecimalValue = 4.3m,
        NullableDoubleValue1 = 5.1,
        NullableDoubleValue2 = 6.1
      };
      _ = new TestEntity(GlobalSession) {
        ByteValue = 4,
        SByteValue = 6,
        ShortValue = 10,
        UShortValue = 11,
        IntValue = 12,
        UIntValue = 13,
        LongValue = 22,
        ULongValue = 27,
        FloatValue = 0.3f,
        DecimalValue = 1.4m,
        DoubleValue1 = 2.3,
        DoubleValue2 = 3.3,
        NullableByteValue = 4,
        NullableSByteValue = 6,
        NullableShortValue = 10,
        NullableUShortValue = 11,
        NullableIntValue = 32,
        NullableUIntValue = 33,
        NullableLongValue = 42,
        NullableULongValue = 47,
        NullableFloatValue = 0.6f,
        NullableDecimalValue = 4.4m,
        NullableDoubleValue1 = 5.3,
        NullableDoubleValue2 = 6.3
      };

      GlobalSession.SaveChanges();
    }
  }
}
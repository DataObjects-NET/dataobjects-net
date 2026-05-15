// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Object cloning helper.
  /// </summary>
  public static class Cloner
  {
    public static class SerializationDomains
    {
      public static readonly Lazy<IReadOnlyList<Type>> System = new Lazy<IReadOnlyList<Type>>(
        () => [
          typeof(DateOnly),       typeof(DateOnly?),       typeof(DateOnly[]),       typeof(DateOnly?[]),
          typeof(TimeOnly),       typeof(TimeOnly?),       typeof(TimeOnly[]),       typeof(TimeOnly?[]),
          typeof(DateTime),       typeof(DateTime?),       typeof(DateTime[]),       typeof(DateTime?[]),
          typeof(TimeSpan),       typeof(TimeSpan?),       typeof(TimeSpan[]),       typeof(TimeSpan?[]),
          typeof(DateTimeOffset), typeof(DateTimeOffset?), typeof(DateTimeOffset[]), typeof(DateTimeOffset?[]),
          typeof(Type),            typeof(Type[]),
          typeof(MethodInfo),      typeof(MethodInfo[]),
          typeof(MemberInfo),      typeof(MemberInfo[]),
          typeof(ConstructorInfo), typeof(ConstructorInfo[]),
          typeof(bool?),   typeof(bool[]),   typeof(bool?[]),
          typeof(byte?),   typeof(byte[]),   typeof(byte?[]),
          typeof(sbyte?),  typeof(sbyte[]),  typeof(sbyte?[]),
          typeof(short?),  typeof(short[]),  typeof(short?[]),
          typeof(ushort?), typeof(ushort[]), typeof(ushort?[]),
          typeof(int?),     typeof(int[]),     typeof(int?[]),
          typeof(uint?),    typeof(uint[]),    typeof(uint?[]),
          typeof(long?),    typeof(long[]),    typeof(long?[]),
          typeof(ulong?),   typeof(ulong[]),   typeof(ulong?[]),
          typeof(float?),   typeof(float[]),   typeof(float?[]),
          typeof(double?),  typeof(double[]),  typeof(double?[]),
          typeof(decimal?), typeof(decimal[]), typeof(decimal?[]),
          typeof(string[]),
        ]);

      public static readonly Lazy<IReadOnlyList<Type>> Sql = new Lazy<IReadOnlyList<Type>>(
        () => [
          typeof(Xtensive.Sql.SqlNode),
          typeof(Xtensive.Sql.SqlNodeType),
          typeof(Xtensive.Sql.SqlStatement),
          typeof(Xtensive.Sql.SqlValueType),
          typeof(Xtensive.Sql.SqlType),
          typeof(Xtensive.Sql.Action),
          typeof(Xtensive.Sql.BoundaryType),
          typeof(Xtensive.Sql.CheckOptions),
          typeof(Xtensive.Sql.ReferentialAction),
        ]);

      public static readonly Lazy<IReadOnlyList<Type>> SqlModel = new Lazy<IReadOnlyList<Type>>(
        () => [
          typeof(Xtensive.Sql.Model.Node),
          typeof(Xtensive.Sql.Model.SchemaNode),
          typeof(Xtensive.Sql.Model.CatalogNode),
          typeof(Xtensive.Sql.Model.Schema),
          typeof(Xtensive.Sql.Model.Catalog),

          //Constraints
          typeof(Xtensive.Sql.Model.Assertion),
          typeof(Xtensive.Sql.Model.Constraint),
          typeof(Xtensive.Sql.Model.CheckConstraint),
          typeof(Xtensive.Sql.Model.DefaultConstraint),
          typeof(Xtensive.Sql.Model.DomainConstraint),
          typeof(Xtensive.Sql.Model.TableConstraint),
          typeof(Xtensive.Sql.Model.UniqueConstraint),
          typeof(Xtensive.Sql.Model.PrimaryKey),
          typeof(Xtensive.Sql.Model.ForeignKey),
          //Partitioning
          typeof(Xtensive.Sql.Model.Partition),
          typeof(Xtensive.Sql.Model.PartitionDescriptor),
          typeof(Xtensive.Sql.Model.PartitionFunction),
          typeof(Xtensive.Sql.Model.PartitionMethod),
          typeof(Xtensive.Sql.Model.PartitionSchema),
          typeof(Xtensive.Sql.Model.HashPartition),
          typeof(Xtensive.Sql.Model.ListPartition),
          typeof(Xtensive.Sql.Model.RangePartition),
          // general types
          typeof(Xtensive.Sql.Model.ChangeTrackingMode),
          typeof(Xtensive.Sql.Model.Collation),
          typeof(Xtensive.Sql.Model.DataTable),
          typeof(Xtensive.Sql.Model.DataTableColumn),
          typeof(Xtensive.Sql.Model.DataTableNode),
          typeof(Xtensive.Sql.Model.Domain),
          typeof(Xtensive.Sql.Model.FullTextIndex),
          typeof(Xtensive.Sql.Model.Index),
          typeof(Xtensive.Sql.Model.Language),
          typeof(Xtensive.Sql.Model.PartitionMethod),
          typeof(Xtensive.Sql.Model.Sequence),
          typeof(Xtensive.Sql.Model.SequenceDescriptor),
          typeof(Xtensive.Sql.Model.SpatialIndex),
          typeof(Xtensive.Sql.Model.Table),
          typeof(Xtensive.Sql.Model.TableColumn),
          typeof(Xtensive.Sql.Model.TemporaryTable),
          typeof(Xtensive.Sql.Model.Translation),
          typeof(Xtensive.Sql.Model.View),
          typeof(Xtensive.Sql.Model.ViewColumn),
          //collections
          typeof(Xtensive.Sql.Model.NodeCollection<>),
          typeof(Xtensive.Sql.Model.NodeCollection<Sql.Model.Language>),
          typeof(Xtensive.Sql.Model.NodeCollection<Sql.Model.DataTableColumn>),
          typeof(Xtensive.Sql.Model.PairedNodeCollection<,>),
          typeof(Xtensive.Sql.Model.PairedNodeCollection<Sql.Model.View, Sql.Model.ViewColumn>),
          typeof(Xtensive.Sql.Model.PairedNodeCollection<Sql.Model.Table, Sql.Model.TableColumn>),
          typeof(Xtensive.Sql.Model.PairedNodeCollection<Sql.Model.Table, Sql.Model.TableConstraint>),
          typeof(Xtensive.Sql.Model.PairedNodeCollection<Sql.Model.Index, Sql.Model.IndexColumn>),
          typeof(Xtensive.Sql.Model.PairedNodeCollection<Sql.Model.Domain, Sql.Model.DomainConstraint>),
        ]);

      public static readonly Lazy<IReadOnlyList<Type>> SqlInfo = new Lazy<IReadOnlyList<Type>>(
        () => [
          //constraints
          typeof(Xtensive.Sql.Info.AssertConstraintInfo),
          typeof(Xtensive.Sql.Info.CheckConstraintFeatures),
          typeof(Xtensive.Sql.Info.CheckConstraintInfo),
          typeof(Xtensive.Sql.Info.ForeignKeyConstraintActions),
          typeof(Xtensive.Sql.Info.ForeignKeyConstraintFeatures),
          typeof(Xtensive.Sql.Info.ForeignKeyConstraintInfo),
          typeof(Xtensive.Sql.Info.PrimaryKeyConstraintFeatures),
          typeof(Xtensive.Sql.Info.PrimaryKeyConstraintInfo),
          typeof(Xtensive.Sql.Info.UniqueConstraintFeatures),
          typeof(Xtensive.Sql.Info.UniqueConstraintInfo),

          typeof(Xtensive.Sql.Info.ColumnFeatures),
          typeof(Xtensive.Sql.Info.ColumnInfo),
          typeof(Xtensive.Sql.Info.CoreServerInfo),
          typeof(Xtensive.Sql.Info.DataTypeCollection),
          typeof(Xtensive.Sql.Info.DataTypeFeatures),
          typeof(Xtensive.Sql.Info.DataTypeInfo),
          typeof(Xtensive.Sql.Info.DdlStatements),
          typeof(Xtensive.Sql.Info.DefaultSchemaInfo),
          typeof(Xtensive.Sql.Info.EntityInfo),
          typeof(Xtensive.Sql.Info.FullTextSearchFeatures),
          typeof(Xtensive.Sql.Info.FullTextSearchInfo),
          typeof(Xtensive.Sql.Info.IdentityFeatures),
          typeof(Xtensive.Sql.Info.IdentityInfo),
          typeof(Xtensive.Sql.Info.IndexFeatures),
          typeof(Xtensive.Sql.Info.ColumnFeatures),
          typeof(Xtensive.Sql.Info.IndexInfo),
          typeof(Xtensive.Sql.Info.IsolationLevels),
          typeof(Xtensive.Sql.Info.PartitionMethods),
          typeof(Xtensive.Sql.Info.QueryFeatures),
          typeof(Xtensive.Sql.Info.QueryInfo),
          typeof(Xtensive.Sql.Info.SequenceFeatures),
          typeof(Xtensive.Sql.Info.SequenceInfo),
          typeof(Xtensive.Sql.Info.ServerFeatures),
          typeof(Xtensive.Sql.Info.ServerInfo),
          typeof(Xtensive.Sql.Info.TableInfo),
          typeof(Xtensive.Sql.Info.TemporaryTableFeatures),
          typeof(Xtensive.Sql.Info.TemporaryTableInfo),
          typeof(Xtensive.Sql.Info.ValueRange<>),
          typeof(Xtensive.Sql.Info.ValueRange<bool>),
          typeof(Xtensive.Sql.Info.ValueRange<char>),
          typeof(Xtensive.Sql.Info.ValueRange<sbyte>),
          typeof(Xtensive.Sql.Info.ValueRange<short>),
          typeof(Xtensive.Sql.Info.ValueRange<int>),
          typeof(Xtensive.Sql.Info.ValueRange<long>),
          typeof(Xtensive.Sql.Info.ValueRange<byte>),
          typeof(Xtensive.Sql.Info.ValueRange<ushort>),
          typeof(Xtensive.Sql.Info.ValueRange<uint>),
          typeof(Xtensive.Sql.Info.ValueRange<ulong>),
          typeof(Xtensive.Sql.Info.ValueRange<float>),
          typeof(Xtensive.Sql.Info.ValueRange<double>),
          typeof(Xtensive.Sql.Info.ValueRange<decimal>),
          typeof(Xtensive.Sql.Info.ValueRange<DateTime>),
          typeof(Xtensive.Sql.Info.ValueRange<DateTimeOffset>),
          typeof(Xtensive.Sql.Info.ValueRange<TimeSpan>),
          typeof(Xtensive.Sql.Info.ValueRange<DateOnly>),
          typeof(Xtensive.Sql.Info.ValueRange<TimeOnly>),
          typeof(Xtensive.Sql.Info.ViewFeatures),
          ]);

      public static readonly Lazy<IReadOnlyList<Type>> SqlDml = new Lazy<IReadOnlyList<Type>>(
        () => [
          // collections
          typeof(Xtensive.Sql.Dml.SqlColumnCollection),
          typeof(Xtensive.Sql.Dml.Collections.SqlInsertValuesCollection),
          typeof(Xtensive.Sql.Dml.SqlOrderCollection),
          typeof(Xtensive.Sql.Dml.SqlTableColumnCollection),

          //expressions
          typeof(Xtensive.Sql.Dml.SqlAggregate),
          typeof(Xtensive.Sql.Dml.SqlArray),
          typeof(Xtensive.Sql.Dml.SqlArray<>),
          typeof(Xtensive.Sql.Dml.SqlArray<bool>),
          typeof(Xtensive.Sql.Dml.SqlArray<char>),
          typeof(Xtensive.Sql.Dml.SqlArray<sbyte>),
          typeof(Xtensive.Sql.Dml.SqlArray<short>),
          typeof(Xtensive.Sql.Dml.SqlArray<int>),
          typeof(Xtensive.Sql.Dml.SqlArray<long>),
          typeof(Xtensive.Sql.Dml.SqlArray<byte>),
          typeof(Xtensive.Sql.Dml.SqlArray<ushort>),
          typeof(Xtensive.Sql.Dml.SqlArray<uint>),
          typeof(Xtensive.Sql.Dml.SqlArray<ulong>),
          typeof(Xtensive.Sql.Dml.SqlArray<float>),
          typeof(Xtensive.Sql.Dml.SqlArray<double>),
          typeof(Xtensive.Sql.Dml.SqlArray<decimal>),
          typeof(Xtensive.Sql.Dml.SqlArray<DateTime>),
          typeof(Xtensive.Sql.Dml.SqlArray<DateTimeOffset>),
          typeof(Xtensive.Sql.Dml.SqlArray<TimeSpan>),
          typeof(Xtensive.Sql.Dml.SqlArray<DateOnly>),
          typeof(Xtensive.Sql.Dml.SqlArray<TimeOnly>),
          typeof(Xtensive.Sql.Dml.SqlArray<Guid>),
          typeof(Xtensive.Sql.Dml.SqlArray<string>),
          typeof(Xtensive.Sql.Dml.SqlBetween),
          typeof(Xtensive.Sql.Dml.SqlBinary),
          typeof(Xtensive.Sql.Dml.SqlCase),
          typeof(Xtensive.Sql.Dml.SqlCast),
          typeof(Xtensive.Sql.Dml.SqlCollate),
          typeof(Xtensive.Sql.Dml.SqlColumn),
          typeof(Xtensive.Sql.Dml.SqlColumnRef),
          typeof(Xtensive.Sql.Dml.SqlColumnStub),
          typeof(Xtensive.Sql.Dml.SqlComment),
          typeof(Xtensive.Sql.Dml.SqlConcat),
          typeof(Xtensive.Sql.Dml.SqlContainer),
          typeof(Xtensive.Sql.Dml.SqlCursor),
          typeof(Xtensive.Sql.Dml.SqlCustomFunctionCall),
          typeof(Xtensive.Sql.Dml.SqlDefaultValue),
          typeof(Xtensive.Sql.Dml.SqlDynamicFilter),
          typeof(Xtensive.Sql.Dml.SqlExpression),
          typeof(Xtensive.Sql.Dml.SqlExpressionList),
          typeof(Xtensive.Sql.Dml.SqlExtract),
          typeof(Xtensive.Sql.Dml.SqlFunctionCall),
          typeof(Xtensive.Sql.Dml.SqlFunctionCallBase),
          typeof(Xtensive.Sql.Dml.SqlLike),
          typeof(Xtensive.Sql.Dml.SqlLiteral),
          typeof(Xtensive.Sql.Dml.SqlLiteral<bool>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<sbyte>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<byte>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<short>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<ushort>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<int>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<uint>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<long>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<ulong>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<float>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<double>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<decimal>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<char>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<string>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<DateTime>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<DateTimeOffset>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<DateOnly>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<TimeOnly>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<TimeSpan>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<byte[]>),
          typeof(Xtensive.Sql.Dml.SqlLiteral<Guid>),
          typeof(Xtensive.Sql.Dml.SqlMatch),
          typeof(Xtensive.Sql.Dml.SqlMetadata),
          typeof(Xtensive.Sql.Dml.SqlNative),
          typeof(Xtensive.Sql.Dml.SqlNextValue),
          typeof(Xtensive.Sql.Dml.SqlNull),
          typeof(Xtensive.Sql.Dml.SqlParameterRef),
          typeof(Xtensive.Sql.Dml.SqlPlaceholder),
          typeof(Xtensive.Sql.Dml.SqlRound),
          typeof(Xtensive.Sql.Dml.SqlRow),
          typeof(Xtensive.Sql.Dml.SqlRowNumber),
          typeof(Xtensive.Sql.Dml.SqlSubQuery),
          typeof(Xtensive.Sql.Dml.SqlTableColumn),
          typeof(Xtensive.Sql.Dml.SqlTrim),
          typeof(Xtensive.Sql.Dml.SqlUnary),
          typeof(Xtensive.Sql.Dml.SqlUserColumn),
          typeof(Xtensive.Sql.Dml.SqlUserFunctionCall),
          typeof(Xtensive.Sql.Dml.SqlVariable),
          typeof(Xtensive.Sql.Dml.SqlVariant),
          // hints
          typeof(Xtensive.Sql.Dml.SqlHint),
          typeof(Xtensive.Sql.Dml.SqlFastFirstRowsHint),
          typeof(Xtensive.Sql.Dml.SqlForceJoinOrderHint),
          typeof(Xtensive.Sql.Dml.SqlJoinHint),
          typeof(Xtensive.Sql.Dml.SqlJoinMethod),
          typeof(Xtensive.Sql.Dml.SqlNativeHint),
          // statements
          typeof(Xtensive.Sql.Dml.SqlAssignment),
          typeof(Xtensive.Sql.Dml.SqlBatch),
          typeof(Xtensive.Sql.Dml.SqlBreak),
          typeof(Xtensive.Sql.Dml.SqlCloseCursor),
          typeof(Xtensive.Sql.Dml.SqlContinue),
          typeof(Xtensive.Sql.Dml.SqlDeclareCursor),
          typeof(Xtensive.Sql.Dml.SqlDeclareVariable),
          typeof(Xtensive.Sql.Dml.SqlDelete),
          typeof(Xtensive.Sql.Dml.SqlFetch),
          typeof(Xtensive.Sql.Dml.SqlFetchOption),
          typeof(Xtensive.Sql.Dml.SqlIf),
          typeof(Xtensive.Sql.Dml.SqlInsert),
          typeof(Xtensive.Sql.Dml.SqlOpenCursor),
          typeof(Xtensive.Sql.Dml.SqlQueryExpression),
          typeof(Xtensive.Sql.Dml.SqlQueryStatement),
          typeof(Xtensive.Sql.Dml.SqlSelect),
          typeof(Xtensive.Sql.Dml.SqlStatementBlock),
          typeof(Xtensive.Sql.Dml.SqlUpdate),
          typeof(Xtensive.Sql.Dml.SqlWhile),
          //
          typeof(Xtensive.Sql.Dml.SqlContainsTable),
          typeof(Xtensive.Sql.Dml.SqlCustomFunctionType),
          typeof(Xtensive.Sql.Dml.SqlDatePart),
          typeof(Xtensive.Sql.Dml.SqlDateTimeOffsetPart),
          typeof(Xtensive.Sql.Dml.SqlDateTimePart),
          typeof(Xtensive.Sql.Dml.SqlFetchOption),
          typeof(Xtensive.Sql.Dml.SqlFragment),
          typeof(Xtensive.Sql.Dml.SqlFreeTextTable),
          typeof(Xtensive.Sql.Dml.SqlFunctionType),
          typeof(Xtensive.Sql.Dml.SqlIntervalPart),
          typeof(Xtensive.Sql.Dml.SqlJoinedTable),
          typeof(Xtensive.Sql.Dml.SqlJoinExpression),
          typeof(Xtensive.Sql.Dml.SqlJoinType),
          typeof(Xtensive.Sql.Dml.SqlLockType),
          typeof(Xtensive.Sql.Dml.SqlMatchType),
          typeof(Xtensive.Sql.Dml.SqlOrder),
          typeof(Xtensive.Sql.Dml.SqlQueryRef),
          typeof(Xtensive.Sql.Dml.SqlTable),
          typeof(Xtensive.Sql.Dml.SqlTableRef),
          typeof(Xtensive.Sql.Dml.SqlTimePart),
          typeof(Xtensive.Sql.Dml.SqlTrimType),
        ]);

      public static readonly Lazy<IReadOnlyList<Type>> SqlDdl = new Lazy<IReadOnlyList<Type>>(
        () => [
          //Ddl Actions
          typeof(Xtensive.Sql.Ddl.SqlAction),
          typeof(Xtensive.Sql.Ddl.SqlAddColumn),
          typeof(Xtensive.Sql.Ddl.SqlAddConstraint),
          typeof(Xtensive.Sql.Ddl.SqlAlterIdentityInfo),
          typeof(Xtensive.Sql.Ddl.SqlCascadableAction),
          typeof(Xtensive.Sql.Ddl.SqlDropColumn),
          typeof(Xtensive.Sql.Ddl.SqlDropConstraint),
          typeof(Xtensive.Sql.Ddl.SqlDropDefault),
          typeof(Xtensive.Sql.Ddl.SqlRenameColumn),
          typeof(Xtensive.Sql.Ddl.SqlSetDefault),
          // Ddl Misc
          typeof(Xtensive.Sql.Ddl.SqlAlterIdentityInfoOptions),
          typeof(Xtensive.Sql.Ddl.SqlAlterPartitionFunctionOption),
          typeof(Xtensive.Sql.Ddl.SqlCommandType),
          //Ddl
          typeof(Xtensive.Sql.Ddl.SqlAlterDomain),
          typeof(Xtensive.Sql.Ddl.SqlAlterPartitionFunction),
          typeof(Xtensive.Sql.Ddl.SqlAlterPartitionScheme),
          typeof(Xtensive.Sql.Ddl.SqlAlterSequence),
          typeof(Xtensive.Sql.Ddl.SqlAlterTable),
          typeof(Xtensive.Sql.Ddl.SqlCommand),
          typeof(Xtensive.Sql.Ddl.SqlCreateAssertion),
          typeof(Xtensive.Sql.Ddl.SqlCreateCharacterSet),
          typeof(Xtensive.Sql.Ddl.SqlCreateCollation),
          typeof(Xtensive.Sql.Ddl.SqlCreateDomain),
          typeof(Xtensive.Sql.Ddl.SqlCreateIndex),
          typeof(Xtensive.Sql.Ddl.SqlCreatePartitionFunction),
          typeof(Xtensive.Sql.Ddl.SqlCreatePartitionScheme),
          typeof(Xtensive.Sql.Ddl.SqlCreateSchema),
          typeof(Xtensive.Sql.Ddl.SqlCreateSequence),
          typeof(Xtensive.Sql.Ddl.SqlCreateTable),
          typeof(Xtensive.Sql.Ddl.SqlCreateTranslation),
          typeof(Xtensive.Sql.Ddl.SqlCreateView),
          typeof(Xtensive.Sql.Ddl.SqlDropAssertion),
          typeof(Xtensive.Sql.Ddl.SqlDropCharacterSet),
          typeof(Xtensive.Sql.Ddl.SqlDropCollation),
          typeof(Xtensive.Sql.Ddl.SqlDropDomain),
          typeof(Xtensive.Sql.Ddl.SqlDropIndex),
          typeof(Xtensive.Sql.Ddl.SqlDropPartitionFunction),
          typeof(Xtensive.Sql.Ddl.SqlDropPartitionScheme),
          typeof(Xtensive.Sql.Ddl.SqlDropSchema),
          typeof(Xtensive.Sql.Ddl.SqlDropSequence),
          typeof(Xtensive.Sql.Ddl.SqlDropTable),
          typeof(Xtensive.Sql.Ddl.SqlDropView),
          typeof(Xtensive.Sql.Ddl.SqlRenameColumn),
          typeof(Xtensive.Sql.Ddl.SqlTruncateTable),
        ]);

      public static readonly Lazy<IReadOnlyList<Type>> SerializableExpressions = new Lazy<IReadOnlyList<Type>>(
        () => [
          typeof(Xtensive.Linq.SerializableExpressions.SerializableBinaryExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableConditionalExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableConstantExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableElementInit),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableInvocationExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableLambdaExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableListInitExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberAssignment),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberBinding),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberInitExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberListBinding),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberMemberBinding),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableMethodCallExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableNewArrayExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableNewExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableParameterExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableTypeBinaryExpression),
          typeof(Xtensive.Linq.SerializableExpressions.SerializableUnaryExpression),
        ]);
    }


    private static readonly IFormatter Formatter = new BinaryFormatter();

    /// <summary>
    /// Clones the <paramref name="source"/> using <see cref="BinaryFormatter"/>.
    /// </summary>
    /// <param name="source">The source to clone.</param>
    [Obsolete]
    public static T CloneViaBinarySerialization<T>(T source)
    {
      using (var stream = new MemoryStream()) {
        Formatter.Serialize(stream, source);
        stream.Position = 0;
        return (T) Formatter.Deserialize(stream);
      }
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="knownTypes">The known type to pass into the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaXmlSerialization<T>(T source, IEnumerable<Type> knownTypes)
    {
      var settings = new DataContractSerializerSettings { KnownTypes = knownTypes, PreserveObjectReferences = true };
      return CloneViaXmlSerialization(source, settings);      
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="settings">The settings for the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaXmlSerialization<T>(T source, DataContractSerializerSettings settings)
    {
      using (var mStream = new MemoryStream()) {
        var dcSerializer = new DataContractSerializer(typeof(T), settings);
        dcSerializer.WriteObject(mStream, source);
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return (T) dcSerializer.ReadObject(mStream);
      }
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractJsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="knownTypes">The known type to pass into the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaJsonSerialization<T>(T source, IEnumerable<Type> knownTypes)
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = knownTypes};
      return CloneViaJsonSerialization(source, settings);
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractJsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="settings">The settings for the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaJsonSerialization<T>(T source, DataContractJsonSerializerSettings settings)
    {
      using (var mStream = new MemoryStream()) {
        var dcSerializer = new DataContractJsonSerializer(typeof(T), settings);
        dcSerializer.WriteObject(mStream, source);
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return (T) dcSerializer.ReadObject(mStream);
      }
    }

    /// <summary>
    /// Clones <paramref name="source"/> via <see cref="JsonSerializer"/> with indentation and reference preservation.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <param name="source">The object to clone.</param>
    /// <param name="options">Options for <see cref="JsonSerializer"/>.</param>
    /// <returns>Cloned object</returns>
    public static T CloneViaJsonSerialization<T>(T source, bool useDataContract = false)
    {
      if (useDataContract) {
        var settings = new DataContractJsonSerializerSettings();
        return CloneViaJsonSerialization(source, settings);
      }
      else {
        var settings = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
        return CloneViaJsonSerialization(source, settings);
      }
    }

    /// <summary>
    /// Clones <paramref name="source"/> via <see cref="JsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <param name="source">The object to clone.</param>
    /// <param name="options">Options for <see cref="JsonSerializer"/>.</param>
    /// <returns>Cloned object</returns>
    public static T CloneViaJsonSerialization<T>(T source, JsonSerializerOptions options)
    {
      using (var mStream = new MemoryStream()) {

        JsonSerializer.Serialize<T>(mStream, source, options);
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return JsonSerializer.Deserialize<T>(mStream, options);

      }
    }
  }
}
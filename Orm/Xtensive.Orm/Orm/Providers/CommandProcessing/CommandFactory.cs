// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A factory of <see cref="Command"/>s and <see cref="CommandPart"/>s.
  /// </summary>
  public class CommandFactory
  {
    private const string ParameterNameFormat = "{0}{1}";
    private const string RowFilterParameterNameFormat = "{0}_{1}_{2}";
    private const string DefaultParameterNamePrefix = "p0_";
    private const int LobBlockSize = ushort.MaxValue;

    private readonly bool emptyStringIsNull;

    public StorageDriver Driver { get; private set; }

    public Session Session { get; private set; }

    public SqlConnection Connection { get; private set; }

    public Command CreateCommand()
    {
      return new Command(this, Connection.CreateCommand());
    }

    public IEnumerable<CommandPart> CreatePersistParts(SqlPersistTask task)
    {
      return CreatePersistParts(task, DefaultParameterNamePrefix);
    }

    public virtual IEnumerable<CommandPart> CreatePersistParts(SqlPersistTask task, string parameterNamePrefix)
    {
      ArgumentValidator.EnsureArgumentNotNull(task, "task");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(parameterNamePrefix, "parameterNamePrefix");

      var nodeConfiguration = Session.StorageNode.Configuration;
      var shareStorageNodesOverNodes = Session.Domain.Configuration.ShareStorageSchemaOverNodes;

      var result = new List<CommandPart>();
      int parameterIndex = 0;
      foreach (var request in task.RequestSequence) {
        var compilationResult = request.GetCompiledStatement();
        var configuration = shareStorageNodesOverNodes
          ? new SqlPostCompilerConfiguration(nodeConfiguration.GetDatabaseMapping(), nodeConfiguration.GetSchemaMapping())
          : new SqlPostCompilerConfiguration();

        var part = new CommandPart();
        
        foreach (var binding in request.ParameterBindings) {
          var parameterValue = GetParameterValue(task, binding);
          if (binding.BindingType==PersistParameterBindingType.VersionFilter && IsHandledLikeNull(parameterValue)) {
            configuration.AlternativeBranches.Add(binding);
          }
          else {
            var parameterName = GetParameterName(parameterNamePrefix, ref parameterIndex);
            configuration.PlaceholderValues.Add(binding, Driver.BuildParameterReference(parameterName));
            AddParameter(part, binding, parameterName, parameterValue);
          }
        }

        part.Statement = compilationResult.GetCommandText(configuration);
        result.Add(part);
      }
      return result;
    }

    public CommandPart CreateQueryPart(SqlLoadTask task)
    {
      return CreateQueryPart(task.Request, DefaultParameterNamePrefix, task.ParameterContext);
    }

    public CommandPart CreateQueryPart(SqlLoadTask task, string parameterNamePrefix)
    {
      return CreateQueryPart(task.Request, parameterNamePrefix, task.ParameterContext);
    }

    public CommandPart CreateQueryPart(IQueryRequest request, ParameterContext parameterContext)
    {
      return CreateQueryPart(request, DefaultParameterNamePrefix, parameterContext);
    }

    public virtual CommandPart CreateQueryPart(IQueryRequest request, string parameterNamePrefix, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(request, "request");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(parameterNamePrefix, "parameterNamePrefix");

      int parameterIndex = 0;
      var compilationResult = request.GetCompiledStatement();
      var upgradeContext = Upgrade.UpgradeContext.GetCurrent(Session.Domain.UpgradeContextCookie);
      var nodeConfiguration = upgradeContext != null ? upgradeContext.NodeConfiguration : Session.StorageNode.Configuration;

      var shareStorageNodesOverNodes = Session.Domain.Configuration.ShareStorageSchemaOverNodes;
      var configuration = shareStorageNodesOverNodes
          ? new SqlPostCompilerConfiguration(nodeConfiguration.GetDatabaseMapping(), nodeConfiguration.GetSchemaMapping())
          : new SqlPostCompilerConfiguration();
      ;
      var result = new CommandPart();

      foreach (var binding in request.ParameterBindings) {
        object parameterValue = GetParameterValue(binding, parameterContext);
        switch (binding.BindingType) {
        case QueryParameterBindingType.Regular:
          break;
        case QueryParameterBindingType.SmartNull:
          // replacing "x = @p" with "x is null" when @p = null (or empty string in case of Oracle)
          if (IsHandledLikeNull(parameterValue)) {
            configuration.AlternativeBranches.Add(binding);
            continue;
          }
          break;
        case QueryParameterBindingType.BooleanConstant:
          // expanding true/false parameters to constants to help query optimizer with branching
          if ((bool) parameterValue)
            configuration.AlternativeBranches.Add(binding);
          continue;
        case QueryParameterBindingType.LimitOffset:
          // not parameter, just inlined constant
          configuration.PlaceholderValues.Add(binding, parameterValue.ToString());
          continue;
        case QueryParameterBindingType.NonZeroLimitOffset:
          // Like "LimitOffset" but we handle zero value specially
          // We replace value with 1 and activate special branch that evaluates "where" part to "false"
          var stringValue = parameterValue.ToString();
          if (stringValue=="0") {
            configuration.PlaceholderValues.Add(binding, "1");
            configuration.AlternativeBranches.Add(binding);
          }
          else
            configuration.PlaceholderValues.Add(binding, stringValue);
          continue;
        case QueryParameterBindingType.RowFilter:
          var filterData = (List<Tuple>) parameterValue;
          var rowTypeMapping = ((QueryRowFilterParameterBinding) binding).RowTypeMapping;
          if (filterData==null) {
            configuration.AlternativeBranches.Add(binding);
            continue;
          }
          var commonPrefix = GetParameterName(parameterNamePrefix, ref parameterIndex);
          var filterValues = new List<string[]>();
          for (int tupleIndex = 0; tupleIndex < filterData.Count; tupleIndex++) {
            var tuple = filterData[tupleIndex];
            var parameterReferences = new string[tuple.Count];
            for (int fieldIndex = 0; fieldIndex < tuple.Count; fieldIndex++) {
              var name = string.Format(RowFilterParameterNameFormat, commonPrefix, tupleIndex, fieldIndex);
              var value = tuple.GetValueOrDefault(fieldIndex);
              parameterReferences[fieldIndex] = Driver.BuildParameterReference(name);
              AddRegularParameter(result, rowTypeMapping[fieldIndex], name, value);
            }
            filterValues.Add(parameterReferences);
          }
          configuration.DynamicFilterValues.Add(binding, filterValues);
          continue;
          case QueryParameterBindingType.TypeIdentifier: {
            var originalTypeId = ((QueryTypeIdentifierParameterBinding) binding).OriginalTypeId;
            parameterValue = Session.StorageNode.TypeIdRegistry[Session.Domain.Model.Types[originalTypeId]];
            break;
          }
        default:
          throw new ArgumentOutOfRangeException("binding.BindingType");
        }
        // regular case -> just adding the parameter
        string parameterName = GetParameterName(parameterNamePrefix, ref parameterIndex);
        configuration.PlaceholderValues.Add(binding, Driver.BuildParameterReference(parameterName));
        AddParameter(result, binding, parameterName, parameterValue);
      }

      result.Statement = compilationResult.GetCommandText(configuration);
      return result;
    }

    private bool IsHandledLikeNull(object parameterValue)
    {
      return parameterValue==null || emptyStringIsNull && parameterValue.Equals(string.Empty);
    }

    private static object GetParameterValue(QueryParameterBinding binding, ParameterContext parameterContext)
    {
      try {
        return binding.ValueAccessor.Invoke(parameterContext);
      }
      catch(Exception exception) {
        throw new TargetInvocationException(Strings.ExExceptionHasBeenThrownByTheParameterValueAccessor, exception);
      }
    }

    private object GetParameterValue(SqlPersistTask task, PersistParameterBinding binding)
    {
      switch (binding.BindingType) {
        case PersistParameterBindingType.Regular when task.Tuple!=null:
          return task.Tuple.GetValueOrDefault(binding.FieldIndex);
        case PersistParameterBindingType.Regular when task.Tuples!=null: {
          //tupleSize = task.Tuples[0].Count;
          //var columnIndex2 = binding.FieldIndex;
          //tupleIndex = Math.DivRem(binding.FieldIndex, tupleSize, out var columnIndex1);
          return task.Tuples[binding.RowIndex].GetValueOrDefault(binding.FieldIndex);
        }
        case PersistParameterBindingType.VersionFilter:
          return task.OriginalTuple.GetValueOrDefault(binding.FieldIndex);
        default:
          throw new ArgumentOutOfRangeException("binding.Source");
      }
    }

    private void AddRegularParameter(CommandPart commandPart, TypeMapping mapping, string name, object value)
    {
      var parameter = Connection.CreateParameter();
      parameter.ParameterName = name;
      mapping.BindValue(parameter, value);
      commandPart.Parameters.Add(parameter);
    }

    private void AddCharacterLobParameter(CommandPart commandPart, string name, string value)
    {
      var lob = Connection.CreateCharacterLargeObject();
      commandPart.Resources.Add(lob);
      if (value!=null) {
        if (value.Length > 0) {
          int offset = 0;
          int remainingSize = value.Length;
          while (remainingSize >= LobBlockSize) {
            lob.Write(value.ToCharArray(offset, LobBlockSize), 0, LobBlockSize);
            offset += LobBlockSize;
            remainingSize -= LobBlockSize;
          }
          if (remainingSize > 0)
            lob.Write(value.ToCharArray(offset, remainingSize), 0, remainingSize);
        }
        else {
          lob.Erase();
        }
      }
      var parameter = Connection.CreateParameter();
      parameter.ParameterName = name;
      lob.BindTo(parameter);
      commandPart.Parameters.Add(parameter);
    }

    private void AddBinaryLobParameter(CommandPart commandPart, string name, byte[] value)
    {
      var lob = Connection.CreateBinaryLargeObject();
      commandPart.Resources.Add(lob);
      if (value!=null) {
        if (value.Length > 0)
          lob.Write(value, 0, value.Length);
        else
          lob.Erase();
      }
      var parameter = Connection.CreateParameter();
      parameter.ParameterName = name;
      lob.BindTo(parameter);
      commandPart.Parameters.Add(parameter);
    }

    private void AddParameter(
      CommandPart commandPart, ParameterBinding binding, string parameterName, object parameterValue)
    {
      switch (binding.TransmissionType) {
      case ParameterTransmissionType.Regular:
        AddRegularParameter(commandPart, binding.TypeMapping, parameterName, parameterValue);
        break;
      case ParameterTransmissionType.CharacterLob:
        AddCharacterLobParameter(commandPart, parameterName, (string) parameterValue);
        break;
      case ParameterTransmissionType.BinaryLob:
        AddBinaryLobParameter(commandPart, parameterName, (byte[]) parameterValue);
        break;
      default:
        throw new ArgumentOutOfRangeException("binding.BindingType");
      }
    }
    
    private string GetParameterName(string prefix, ref int index)
    {
      var result = string.Format(ParameterNameFormat, prefix, index);
      index++;
      return result;
    }


    // Constructors

    public CommandFactory(StorageDriver driver, Session session, SqlConnection connection)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(connection, "connection");

      Driver = driver;
      Session = session;
      Connection = connection;

      emptyStringIsNull = driver.ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
    }
  }
}
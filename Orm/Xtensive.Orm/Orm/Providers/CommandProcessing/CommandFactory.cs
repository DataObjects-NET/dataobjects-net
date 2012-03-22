// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Parameters;
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

      var result = new List<CommandPart>();
      int parameterIndex = 0;
      foreach (var request in task.RequestSequence) {
        var tuple = task.Tuple;
        var compilationResult = request.GetCompiledStatement();
        var configuration = new SqlPostCompilerConfiguration();
        var part = new CommandPart();
        
        foreach (var binding in request.ParameterBindings) {
          var parameterValue = tuple.GetValueOrDefault(binding.FieldIndex);
          string parameterName = GetParameterName(parameterNamePrefix, ref parameterIndex);
          configuration.PlaceholderValues.Add(binding, Driver.BuildParameterReference(parameterName));
          AddPersistParameter(part, parameterName, parameterValue, binding);
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

    public CommandPart CreateQueryPart(IQueryRequest request)
    {
      return CreateQueryPart(request, DefaultParameterNamePrefix, null);
    }

    public CommandPart CreateQueryPart(IQueryRequest request, string parameterNamePrefix)
    {
      return CreateQueryPart(request, parameterNamePrefix, null);
    }

    public virtual CommandPart CreateQueryPart(IQueryRequest request, string parameterNamePrefix, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(request, "request");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(parameterNamePrefix, "parameterNamePrefix");

      int parameterIndex = 0;
      var compilationResult = request.GetCompiledStatement();
      var configuration = new SqlPostCompilerConfiguration();
      var result = new CommandPart();

      using (parameterContext.ActivateSafely()) {
        foreach (var binding in request.ParameterBindings) {
          object parameterValue = binding.ValueAccessor.Invoke();
          switch (binding.BindingType) {
          case QueryParameterBindingType.Regular:
            break;
          case QueryParameterBindingType.SmartNull:
            // replacing "x = @p" with "x is null" when @p = null (or empty string in case of Oracle)
            if (parameterValue==null || emptyStringIsNull && parameterValue.Equals(string.Empty)) {
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
                AddRegularParameter(result, name, value, rowTypeMapping[fieldIndex]);
              }
              filterValues.Add(parameterReferences);
            }
            configuration.DynamicFilterValues.Add(binding, filterValues);
            continue;
          default:
            throw new ArgumentOutOfRangeException("binding.BindingType");
          }
          // regular case -> just adding the parameter
          string parameterName = GetParameterName(parameterNamePrefix, ref parameterIndex);
          configuration.PlaceholderValues.Add(binding, Driver.BuildParameterReference(parameterName));
          AddRegularParameter(result, parameterName, parameterValue, binding.TypeMapping);
        }
      }
      result.Statement = compilationResult.GetCommandText(configuration);
      return result;
    }
    
    private void AddRegularParameter(CommandPart commandPart, string name, object value, TypeMapping mapping)
    {
      var parameter = Connection.CreateParameter();
      parameter.ParameterName = name;
      mapping.SetParameterValue(parameter, value);
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

    private void AddPersistParameter(CommandPart commandPart,
      string parameterName, object parameterValue, PersistParameterBinding binding)
    {
      switch (binding.TransmissionType) {
      case ParameterTransmissionType.Regular:
        AddRegularParameter(commandPart, parameterName, parameterValue, binding.TypeMapping);
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
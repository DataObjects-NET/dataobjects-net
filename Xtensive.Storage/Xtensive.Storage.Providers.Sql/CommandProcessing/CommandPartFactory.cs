// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A factory of <see cref="CommandPart"/>s.
  /// </summary>
  public class CommandPartFactory
  {
    private const string ParameterNameFormat = "{0}{1}";
    private const string RowFilterParameterNameFormat = "{0}_{1}_{2}";

    private const int LobBlockSize = ushort.MaxValue;
    private readonly DomainHandler domainHandler;
    private readonly Driver driver;
    private readonly bool emptyStringIsNull;

    /// <summary>
    /// Connection this command part factory is bound to,
    /// </summary>
    protected readonly SqlConnection connection;

    /// <summary>
    /// Creates the persist command part.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="parameterNamePrefix">The parameter name prefix.</param>
    /// <returns>Created command part.</returns>
    public virtual IEnumerable<CommandPart> CreatePersistCommandPart(SqlPersistTask task, string parameterNamePrefix)
    {
      var result = new List<CommandPart>();
      int parameterIndex = 0;
      foreach (var request in task.RequestSequence) {
        var tuple = task.Tuple;
        var compilationResult = request.GetCompiledStatement(domainHandler);
        var configuration = new SqlPostCompilerConfiguration();
        var part = new CommandPart();
        
        foreach (var binding in request.ParameterBindings) {
          var parameterValue = tuple.GetValueOrDefault(binding.FieldIndex);
          string parameterName = GetParameterName(parameterNamePrefix, ref parameterIndex);
          configuration.PlaceholderValues.Add(binding, driver.BuildParameterReference(parameterName));
          AddPersistParameter(part, parameterName, parameterValue, binding);
        }

        part.Query = compilationResult.GetCommandText(configuration);
        result.Add(part);
      }
      return result;
    }

    /// <summary>
    /// Creates the query command part.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="parameterNamePrefix">The parameter name prefix.</param>
    /// <returns>Created command part.</returns>
    public virtual CommandPart CreateQueryCommandPart(SqlQueryTask task, string parameterNamePrefix)
    {
      var request = task.Request;
      int parameterIndex = 0;
      var compilationResult = request.GetCompiledStatement(domainHandler);
      var configuration = new SqlPostCompilerConfiguration();
      var result = new CommandPart();

      using (task.ParameterContext.ActivateSafely()) {
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
                parameterReferences[fieldIndex] = driver.BuildParameterReference(name);
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
          configuration.PlaceholderValues.Add(binding, driver.BuildParameterReference(parameterName));
          AddRegularParameter(result, parameterName, parameterValue, binding.TypeMapping);
        }
      }
      result.Query = compilationResult.GetCommandText(configuration);
      return result;
    }
    
    private void AddRegularParameter(CommandPart commandPart, string name, object value, TypeMapping mapping)
    {
      var parameter = connection.CreateParameter();
      parameter.ParameterName = name;
      mapping.SetParameterValue(parameter, value);
      commandPart.Parameters.Add(parameter);
    }

    private void AddCharacterLobParameter(CommandPart commandPart, string name, string value)
    {
      var lob = connection.CreateCharacterLargeObject();
      commandPart.Disposables.Add(lob);
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
      var parameter = connection.CreateParameter();
      parameter.ParameterName = name;
      lob.BindTo(parameter);
      commandPart.Parameters.Add(parameter);
    }

    private void AddBinaryLobParameter(CommandPart commandPart, string name, byte[] value)
    {
      var lob = connection.CreateBinaryLargeObject();
      commandPart.Disposables.Add(lob);
      if (value!=null) {
        if (value.Length > 0)
          lob.Write(value, 0, value.Length);
        else
          lob.Erase();
      }
      var parameter = connection.CreateParameter();
      parameter.ParameterName = name;
      lob.BindTo(parameter);
      commandPart.Parameters.Add(parameter);
    }

    private void AddPersistParameter(CommandPart commandPart,
      string parameterName, object parameterValue, PersistParameterBinding binding)
    {
      switch (binding.BindingType) {
      case PersistParameterBindingType.Regular:
        AddRegularParameter(commandPart, parameterName, parameterValue, binding.TypeMapping);
        break;
      case PersistParameterBindingType.CharacterLob:
        AddCharacterLobParameter(commandPart, parameterName, (string) parameterValue);
        break;
      case PersistParameterBindingType.BinaryLob:
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

    public CommandPartFactory(DomainHandler domainHandler, SqlConnection connection)
    {
      this.domainHandler = domainHandler;
      this.connection = connection;
      driver = domainHandler.Driver;
      emptyStringIsNull = domainHandler.ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
    }
  }
}
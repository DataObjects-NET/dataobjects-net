// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using Xtensive.Core.Parameters;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class CommandPartFactory
  {
    private const int LobBlockSize = ushort.MaxValue;
    private readonly DomainHandler domainHandler;
    private readonly Driver driver;
    private readonly SqlConnection connection;
    private readonly bool emptyStringIsNull;
    
    public CommandPart CreatePersistCommandPart(SqlPersistTask task, string parameterNamePrefix)
    {
      var request = task.Request;
      var tuple = task.Tuple;
      int parameterIndex = 0;
      var compilationResult = request.Compile(domainHandler);
      var configuration = new SqlPostCompilerConfiguration();
      var result = new CommandPart();
      
      foreach (var binding in request.ParameterBindings) {
        var parameterValue = tuple.GetValueOrDefault(binding.FieldIndex);
        string parameterName = parameterNamePrefix + parameterIndex++;
        configuration.PlaceholderValues.Add(binding, driver.BuildParameterReference(parameterName));
        AddPersistParameter(result, parameterName, parameterValue, binding);
      }
      result.Query = compilationResult.GetCommandText(configuration);
      return result;
    }

    public CommandPart CreateQueryCommandPart(SqlQueryTask task, string parameterNamePrefix)
    {
      var request = task.Request;
      int parameterIndex = 0;
      var compilationResult = request.Compile(domainHandler);
      var configuration = new SqlPostCompilerConfiguration();
      var result = new CommandPart();

      using (task.ParameterContext.ActivateSafely()) {
        foreach (var binding in request.ParameterBindings) {
          object parameterValue = binding.ValueAccessor.Invoke();
          // no parameters - just inlined constant
          if (binding.BindingType==QueryParameterBindingType.LimitOffset) {
            configuration.PlaceholderValues.Add(binding, parameterValue.ToString());
            continue;
          }
          // expanding true/false parameters to constants to help query optimizer with branching
          if (binding.BindingType==QueryParameterBindingType.BooleanConstant) {
            if ((bool) parameterValue)
              configuration.AlternativeBranches.Add(binding);
            continue;
          }
          // replacing "x = @p" with "x is null" when @p = null (or empty string in case of Oracle)
          if (binding.BindingType==QueryParameterBindingType.SmartNull &&
            (parameterValue==null || emptyStringIsNull && parameterValue.Equals(string.Empty))) {
            configuration.AlternativeBranches.Add(binding);
            continue;
          }
          // regular case -> just adding the parameter
          string parameterName = parameterNamePrefix + parameterIndex++;
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
      lob.SetParameterValue(parameter);
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
      lob.SetParameterValue(parameter);
      commandPart.Parameters.Add(parameter);
    }

    private void AddPersistParameter(CommandPart commandPart, string parameterName, object parameterValue, PersistParameterBinding binding)
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
    

    // Constructors

    public CommandPartFactory(DomainHandler domainHandler, SqlConnection connection)
    {
      this.connection = connection;
      this.domainHandler = domainHandler;
      driver = domainHandler.Driver;
      emptyStringIsNull = domainHandler.ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
    }
  }
}
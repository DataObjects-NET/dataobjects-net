// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Disposing;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  internal abstract class CommandProcessor : IDisposable
  {
    private const int LobBlockSize = ushort.MaxValue;

    private bool emptyStringIsNull;
    private DisposableSet disposables;

    protected const string DefaultParameterNamePrefix = "p";

    protected readonly DomainHandler domainHandler;
    protected readonly Driver driver;
    protected readonly LinkedList<SqlQueryTask> queryTasks;
    protected readonly LinkedList<SqlPersistTask> persistTasks;

    protected DbCommand command;

    public SqlConnection Connection { get; set; }
    public DbTransaction Transaction { get; set; }
    
    #region Low-level execute methods

    public object ExecuteScalar(SqlScalarTask task)
    {
      CreateCommand();
      try {
        command.CommandText = task.Request.Compile(domainHandler).GetCommandText();
        return driver.ExecuteScalar(command);
      }
      finally {
        DisposeCommand();
      }
    }
    
    public void ExecutePersist(SqlPersistTask task)
    {
      CreateCommand();
      try {
        command.CommandText = CreatePersistCommandPart(task, DefaultParameterNamePrefix);
        driver.ExecuteNonQuery(command);
      }
      finally {
        DisposeCommand();
      }
    }

    public DbDataReader ExecuteQuery(SqlQueryTask task)
    {
      CreateCommand();
      try {
        command.CommandText = CreateQueryCommandPart(task, DefaultParameterNamePrefix);
        return driver.ExecuteReader(command);
      }
      finally {
        DisposeCommand();
      }
    }

    #endregion

    public abstract void ExecuteRequests(bool dirty);

    public abstract IEnumerator<Tuple> ExecuteRequestsWithReader(SqlQueryRequest request);
    
    public void RegisterTask(SqlQueryTask task)
    {
      queryTasks.AddLast(task);
    }

    public void RegisterTask(SqlPersistTask tasks)
    {
      persistTasks.AddLast(tasks);
    }

    protected void CreateCommand()
    {
      command = Connection.CreateCommand();
      command.Transaction = Transaction;
    }

    protected void DisposeCommand()
    {
      command.DisposeSafely();
      command = null;
      disposables.DisposeSafely();
      disposables = null;
    }
    
    protected string CreatePersistCommandPart(SqlPersistTask task, string parameterNamePrefix)
    {
      var request = task.Request;
      var tuple = task.Tuple;
      int parameterIndex = 0;
      var compilationResult = request.Compile(domainHandler);
      var parameterNames = new Dictionary<object, string>();
      
      foreach (var binding in request.ParameterBindings) {
        var parameterValue = tuple.GetValueOrDefault(binding.FieldIndex);
        string parameterName = parameterNamePrefix + parameterIndex++;
        parameterNames.Add(binding.ParameterReference.Parameter, parameterName);
        AddPersistParameter(parameterName, parameterValue, binding);
      }
      return compilationResult.GetCommandText(parameterNames);
    }

    protected string CreateQueryCommandPart(SqlQueryTask task, string parameterNamePrefix)
    {
      var request = task.Request;
      int parameterIndex = 0;
      var compilationResult = request.Compile(domainHandler);
      var parameterNames = new Dictionary<object, string>();
      var variantKeys = new List<object>();

      using (task.ParameterContext.ActivateSafely()) {
        foreach (var binding in request.ParameterBindings) {
          object parameterValue = binding.ValueAccessor.Invoke();
          // expanding true/false parameters to constants to help query optimizer with branching
          if (binding.BindingType==SqlQueryParameterBindingType.BooleanConstant) {
            if ((bool) parameterValue)
              variantKeys.Add(binding.ParameterReference.Parameter);
            continue;
          }
          // replacing "x = @p" with "x is null" when @p = null (or empty string in case of Oracle)
          if (binding.BindingType==SqlQueryParameterBindingType.SmartNull &&
            (parameterValue==null || emptyStringIsNull && parameterValue.Equals(string.Empty))) {
            variantKeys.Add(binding.ParameterReference.Parameter);
            continue;
          }
          // regular case -> just adding the parameter
          string parameterName = parameterNamePrefix + parameterIndex++;
          parameterNames.Add(binding.ParameterReference.Parameter, parameterName);
          AddRegularParameter(parameterName, parameterValue, binding.TypeMapping);
        }
      }
      return compilationResult.GetCommandText(variantKeys, parameterNames);
    }

    protected void AddRegularParameter(string name, object value, TypeMapping mapping)
    {
      var parameter = command.CreateParameter();
      parameter.ParameterName = name;
      mapping.SetParameterValue(parameter, value);
      command.Parameters.Add(parameter);
    }

    protected void AddCharacterLobParameter(string name, string value)
    {
      var lob = Connection.CreateCharacterLargeObject();
      RegisterDisposable(lob);
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
      var parameter = command.CreateParameter();
      parameter.ParameterName = name;
      lob.SetParameterValue(parameter);
      command.Parameters.Add(parameter);
    }

    protected void AddBinaryLobParameter(string name, byte[] value)
    {
      var lob = Connection.CreateBinaryLargeObject();
      RegisterDisposable(lob);
      if (value!=null) {
        if (value.Length > 0)
          lob.Write(value, 0, value.Length);
        else
          lob.Erase();
      }
      var parameter = command.CreateParameter();
      parameter.ParameterName = name;
      lob.SetParameterValue(parameter);
      command.Parameters.Add(parameter);
    }

    protected void AddPersistParameter(string parameterName, object parameterValue, SqlPersistParameterBinding binding)
    {
      switch (binding.BindingType) {
      case SqlPersistParameterBindingType.Regular:
        AddRegularParameter(parameterName, parameterValue, binding.TypeMapping);
        break;
      case SqlPersistParameterBindingType.CharacterLob:
        AddCharacterLobParameter(parameterName, (string) parameterValue);
        break;
      case SqlPersistParameterBindingType.BinaryLob:
        AddBinaryLobParameter(parameterName, (byte[]) parameterValue);
        break;
      default:
        throw new ArgumentOutOfRangeException("binding.BindingType");
      }
    }

    protected IEnumerator<Tuple> RunTupleReader(DbDataReader reader, TupleDescriptor descriptor)
    {
      var accessor = domainHandler.GetDataReaderAccessor(descriptor);
      using (reader) {
        while (driver.ReadRow(reader)) {
          var tuple = Tuple.Create(descriptor);
          accessor.Read(reader, tuple);
          yield return tuple;
        }
      }
    }

    protected void RegisterDisposable(IDisposable disposable)
    {
      if (disposables==null)
        disposables = new DisposableSet();
      disposables.Add(disposable);
    }
    
    public void Dispose()
    {
      DisposeCommand();
    }


    // Constructors

    protected CommandProcessor(DomainHandler handler)
    {
      domainHandler = handler;
      driver = handler.Driver;
      queryTasks = new LinkedList<SqlQueryTask>();
      persistTasks = new LinkedList<SqlPersistTask>();
      emptyStringIsNull = handler.ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
    }
  }
}
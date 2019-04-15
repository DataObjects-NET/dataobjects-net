using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Storage.CommandProcessing
{
  public class CommandCounter : IDisposable
  {
    private readonly Session session;
    private bool isAttached;
    private int count;

    public int Count
    {
      get { return count; }
    }

    public IDisposable Attach()
    {
      if (isAttached)
        new Disposable((a) => Detach());
      session.Events.DbCommandExecuting += EventsOnDbCommandExecuting;
      isAttached = true;
      return new Disposable((a) => Detach());
    }

    public void Detach()
    {
      if (!isAttached)
        return;
      session.Events.DbCommandExecuting -= EventsOnDbCommandExecuting;
      isAttached = false;
    }

    public void Reset()
    {
      count = 0;
    }

    public void Dispose()
    {
      Detach();
    }

    private void EventsOnDbCommandExecuting(object sender, DbCommandEventArgs dbCommandEventArgs)
    {
      count++;
      Console.WriteLine(dbCommandEventArgs.Command.CommandText);
    }

    public CommandCounter(Session session)
    {
      this.session = session;
    }
  }

  [HierarchyRoot]
  public class NormalAmountOfFieldsEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public int Value { get; set; }

    [Field]
    public int? AlternativeValue { get; set; }
  }

  [HierarchyRoot]
  public class ALotOfFieldsEntityValid : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    // fields are added dynamically
  }

  [HierarchyRoot]
  public class ALotOfFieldsEntityInvalid : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    // fields are added dynamically
  }

  [HierarchyRoot]
  public class ALotOfFieldsEntityVersionized : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Version { get; private set; }

    // fields are added dynamically
  }


  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class SeveralPersistActionsEntityValidA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

  }

  public class SeveralPersistActionsEntityValidB : SeveralPersistActionsEntityValidA
  {
    // fields are added dynamically
  }

  public class SeveralPersistActionsEntityValidC : SeveralPersistActionsEntityValidB
  {
    // fields are added dynamically
  }

  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class SeveralPersistActionsEntityInvalidA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    // fields are added dynamically
  }

  public class SeveralPersistActionsEntityInvalidB : SeveralPersistActionsEntityInvalidA
  {
    // fields are added dynamically
  }

  public class SeveralPersistActionsEntityInvalidC : SeveralPersistActionsEntityInvalidB
  {
    // fields are added dynamically
  }


  public class FieldsCreator : IModule
  {
    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      var originalCount = context.Domain.Handlers.ProviderInfo.MaxQueryParameterCount;
      var maxcount = originalCount;

      if (maxcount > 1024)
        maxcount = 1000;

      var validFieldCount = maxcount - 1;
      var invalidFieldCount = validFieldCount + 10;
      var t = model.Types[typeof(ALotOfFieldsEntityValid)];
      foreach (var fieldName in GetFieldNames(validFieldCount))
        t.DefineField(fieldName, typeof(int));

      t = model.Types[typeof(ALotOfFieldsEntityInvalid)];
      foreach (var fieldName in GetFieldNames(invalidFieldCount))
        t.DefineField(fieldName, typeof(int));

      t = model.Types[typeof(ALotOfFieldsEntityVersionized)];
      foreach (var fieldName in GetFieldNames(validFieldCount - 1))
        t.DefineField(fieldName, typeof(int));


      // each entitity field count is valid
      // overall entity field count is valid
      var count = 0;
      var types = new TypeDef[] {
        model.Types[typeof (SeveralPersistActionsEntityValidA)],
        model.Types[typeof (SeveralPersistActionsEntityValidB)],
        model.Types[typeof (SeveralPersistActionsEntityValidC)],
      };
      int indexToWrite = 0;
      var currentFieldCount = 0;
      foreach (var fieldName in GetFieldNames(validFieldCount))
      {
        types[indexToWrite].DefineField(fieldName, typeof(int));
        currentFieldCount++;
        if (currentFieldCount > validFieldCount / 3)
          indexToWrite = 1;
        if (currentFieldCount > validFieldCount / 3 * 2)
          indexToWrite = 2;
      }


      // each entity field count should be valid
      // overall count should be invalid
      types = new TypeDef[] {
        model.Types[typeof (SeveralPersistActionsEntityInvalidA)],
        model.Types[typeof (SeveralPersistActionsEntityInvalidB)],
        model.Types[typeof (SeveralPersistActionsEntityInvalidC)],
      };
      indexToWrite = 0;
      currentFieldCount = 0;
      var fieldsCount = 2100;
      foreach (var fieldName in GetFieldNames(fieldsCount))
      {
        types[indexToWrite].DefineField(fieldName, typeof(int));
        currentFieldCount++;
        if (currentFieldCount > fieldsCount / 3)
          indexToWrite = 1;
        if (currentFieldCount > fieldsCount / 3 * 2)
          indexToWrite = 2;
      }
    }

    public void OnBuilt(Domain domain)
    {

    }

    private IEnumerable<string> GetFieldNames(int numberOfFields)
    {
      var numberOfFieldsLocal = numberOfFields;
      var currentCount = 0;
      while (currentCount++ < numberOfFieldsLocal)
        yield return "Value" + currentCount;
    }
  }

  public static class FieldUpdater
  {
    public static void UpdateGeneratedFields(this Entity entity)
    {
      var generatedFields = entity.TypeInfo.Fields
        .Where(f => !f.IsSystem && f.UnderlyingProperty == null)
        .Select(f => f.Name)
        .ToList();

      foreach (var fieldName in generatedFields)
      {
        var oldValue = (int)entity[fieldName];
        entity[fieldName] = oldValue + 1;
      }
    }

    public static void InitializeGeneratedFieldsWith(this Entity entity, int value)
    {
      var generatedFields = entity.TypeInfo.Fields
        .Where(f => !f.IsSystem && f.UnderlyingProperty == null)
        .Select(f => f.Name)
        .ToList();

      foreach (var fieldName in generatedFields)
        entity[fieldName] = value;
    }
  }
}

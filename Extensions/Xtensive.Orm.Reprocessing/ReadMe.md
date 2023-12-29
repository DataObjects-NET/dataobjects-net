Xtensive.Orm.Reprocessing
=========================

Summary
-------
The extension provides API for reprocessible operations. The reprocessible operation 
should represent a separate block of logic, usually a delegate of a method and be transactional.

Prerequisites
-------------
DataObjects.Net 6.0.x (http://dataobjects.net)

Examples of usage
-----------------
**Examples #1**. Simple reprocessible operation looks like this:

```csharp
  Domain.Execute(session => {
    // Task logic
  });
```

**Expample #2**. Usage of built-in reprocessing strategies.

There are 3 strategies that can be used for task execution:
- **HandleReprocessibleException** strategy
    The strategy catches all reprocessible expections (deadlock and transaction serialization exceptions)
    and makes another attempt to execute the task
- **HandleUniqueConstraintViolation** strategy
    The same as previous one but also catches unique constraint violation exception 
- **NoReprocess** strategy
    No reprocessing is provided

To indicate that a particular strategy should be used, use the following syntax:

```csharp
  Domain.WithStrategy(new HandleReprocessExceptionStrategy())
    .Execute(session => {
      // Task logic
    });
```

**Expample #3**. Confugure reprocessing in configuration file. To omit setting up the strategy each time consider configuring it in 
application configuration file, e.g.:

```xml
  <configSections>
    <!-- ... -->
    <section name="Xtensive.Orm.Reprocessing" 
      type="Xtensive.Orm.Reprocessing.Configuration.ConfigurationSection, Xtensive.Orm.Reprocessing" />
  </configSections>

  <Xtensive.Orm.Reprocessing 
    defaultTransactionOpenMode="New" 
    defaultExecuteStrategy="Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing">
  </Xtensive.Orm.Reprocessing>
```

Having that done, in scenarios with no strategy specified, the extension will automatically use 
the strategy from the configuration.
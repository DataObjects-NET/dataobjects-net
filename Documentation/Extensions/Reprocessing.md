# Reprocessing extension

### Installation

The extension is available on Nuget

    dotnet add package Xtensive.Orm.Reprocessing

### Usage

Simple reprocessible operation looks like this

    Domain.Execute(session =>
      {
        // Task logic
      });

There are 3 built-in strategies that can me used for task execution:

- `HandleReprocessibleException` strategy catches all reprocessible exceptions (deadlock and transaction serialization exceptions) and makes another attempt to execute task
- `HandleUniqueConstraintViolation` strategy catches same exceptions as previous but also unique constraint violation exception
- `NoReprocess` strategy provides no reprocessing.

If built-in strategies is not enough it is possible to extend existing or create brand new one which targets you needs derived from `ExecutionStrategy` abstract class, e.g.

    public class HandleUniqueConstraintViolationFirstTwoTimesStrategy : HandleUniqueConstraintViolationStrategy
    {
      protected override bool OnError(ExecuteErrorEventArgs context)
      {
        return context.Attempt < 2;
      }
    }

To indicate that a particular strategy should be used, use the following syntax:

    Domain.WithStrategy(new HandleReprocessExceptionStrategy())
        .Execute(session =>
            {
              // Task logic
            });

To omit settings up the strategy each time consider configuring it in application configuration file, e.g:

    <configSections>
      ...
      <section name="Xtensive.Orm.Reprocessing"
        type="Xtensive.Orm.Reprocessing.Configuration.ConfigurationSection, Xtensive.Orm.Reprocessing" />
    </configSections>

    <Xtensive.Orm.Reprocessing
      defaultTransactionOpenMode="New"
      defaultExecuteStrategy="Xtensive.Orm.Reprocessing.HandleReprocessableExceptionStrategy, Xtensive.Orm.Reprocessing">
    </Xtensive.Orm.Reprocessing>

Having that done, in scenarious with no strategy specified, the extension will automatically use the strategy from the configuration.


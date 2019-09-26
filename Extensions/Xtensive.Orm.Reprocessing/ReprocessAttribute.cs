#if NOT_IMPLMENETED

using System;
using System.Transactions;
using PostSharp.Aspects;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Execute method in reprocessable mode.
  /// </summary>
  [Serializable]
  public class ReprocessAttribute : MethodInterceptionAspect
  {
    private StandardExecutionStrategy? _strategy;
    private TransactionOpenMode? _transactionOpenMode;

    /// <summary>
    /// Gets or sets the custom strategy. This property overrides <see cref="Strategy"/> property if set.
    /// </summary>
    /// <value>
    /// The custom strategy.
    /// </value>
    public Type CustomStrategy { get; set; }
    /// <summary>
    /// Gets or sets the standard strategy.
    /// </summary>
    /// <value>
    /// The strategy.
    /// </value>
    public StandardExecutionStrategy Strategy
    {
      get { return _strategy.GetValueOrDefault(); }
      set { _strategy = value; }
    }

    /// <summary>
    /// Gets or sets the isolation level.
    /// </summary>
    /// <value>
    /// The isolation level.
    /// </value>
    public IsolationLevel IsolationLevel { get; set; }

    /// <summary>
    /// Gets or sets the transaction open mode.
    /// </summary>
    /// <value>
    /// The transaction open mode.
    /// </value>
    public TransactionOpenMode TransactionOpenMode
    {
      get { return _transactionOpenMode.GetValueOrDefault(); }
      set { _transactionOpenMode = value; }
    }


    /// <summary>
    /// Gets or sets the domain in which context should run all methods marked with this attribute.
    /// </summary>
    /// <value>
    /// The domain.
    /// </value>
    public static Domain Domain { get; set; }

    /// <summary>
    /// Method invoked <i>instead</i> of the method to which the aspect has been applied.
    /// </summary>
    /// <param name="args">Advice arguments.</param>
    public override void OnInvoke(MethodInterceptionArgs args)
    {
      IExecuteActionStrategy strategy = null;
      if (CustomStrategy != null)
        strategy = ExecuteActionStrategy.GetSingleton(CustomStrategy);
      else if (_strategy != null)
      {
        switch (_strategy.Value)
        {
          case StandardExecutionStrategy.HandleReprocessableException:
            strategy = ExecuteActionStrategy.HandleReprocessableException;
            break;
          case StandardExecutionStrategy.NoReprocess:
            strategy = ExecuteActionStrategy.NoReprocess;
            break;
          case StandardExecutionStrategy.HandleUniqueConstraintViolation:
            strategy = ExecuteActionStrategy.HandleUniqueConstraintViolation;
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      
      GetDomain().ExecuteInternal(IsolationLevel, _transactionOpenMode, strategy, session => args.Proceed());
    }

    #region Non-public methods

    /// <summary>
    /// Gets the domain in wich context should run.
    /// </summary>
    /// <returns>The domain.</returns>
    protected virtual Domain GetDomain()
    {
      return Domain;
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ReprocessAttribute"/> class.
    /// </summary>
    public ReprocessAttribute()
    {
      IsolationLevel = IsolationLevel.Unspecified;
    }
  }
}

#endif

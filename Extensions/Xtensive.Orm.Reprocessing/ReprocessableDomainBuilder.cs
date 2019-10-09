using System;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Domain builder with reprocess after <see cref="ReprocessableException"/>
  /// </summary>
  public class ReprocessableDomainBuilder : DomainBuilder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ReprocessableDomainBuilder"/> class.
    /// </summary>
    public ReprocessableDomainBuilder()
    {
      Attempts = 5;
    }

    /// <summary>
    /// Gets or sets the number of attempts.
    /// </summary>
    /// <value>
    /// The number of attempts.
    /// </value>
    public int Attempts { get; set; }

    /// <summary>
    /// Builds the domain with specified configuration.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <returns>The domain.</returns>
    public override Domain Build(DomainConfiguration config)
    {
      int i = 0;
      while (true)
      {
        try
        {
          return base.Build(config);
        }
        catch (ReprocessableException)
        {
          i++;
          if (i >= Attempts)
            throw;
        }
      }
    }

    /// <summary>
    /// Raises the <see cref="E:Error"/> event.
    /// </summary>
    /// <param name="args">The <see cref="Xtensive.Orm.Reprocessing.DomainBuildErrorEventArgs"/> instance containing the event data.</param>
    protected virtual void OnError(DomainBuildErrorEventArgs args)
    {
      if (Error != null)
        Error(this, args);
    }

    /// <summary>
    /// Occurs when <see cref="ReprocessableException"/> is thrown.
    /// </summary>
    public event EventHandler<DomainBuildErrorEventArgs> Error;
  }
}
# Tracking extension

The extension provides tracking/auditing funtionality on Session/Domain level.

###Installation

The extension is available on Nuget

    dotnet add package Xtensive.Orm.Tracking.Core

### Usage

Include types from `Xtensive.Orm.Tracking` assembly into the domain.

    <Xtensive.Orm>
      <domains>
        <domain ... >
          <types>
            <add assembly="your assembly"/>
            <add assembly="Xtensive.Orm.Tracking"/>
          </types>
        </domain>
      </domains>
    </Xtensive.Orm>

To track changes on `Session` level obtain an instance of `ISessionTrackingMonitor` through `Session.Services.Get<ISessionTrackingMonitor>()` method. To track changes on `Domain` level (from all sessions) obtain an instance of `IDomainTrackingMonitor` through `Domain.Services.Get<IDomainTrackingMonitor>()` method. Subscribe to TrackingCompleted event. After each tracked transaction is committed you receive the TrackingCompletedEventArgs object. TrackingCompletedEventArgs.Changes contains a collection of ITrackingItem objects, each of them represents a set of changes that occurred to an Entity within the transaction committed.

For instance, subscribe to ISessionTrackingMonitor/IDomainTrackingMonitor TrackingCompleted event

    var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
    monitor.TrackingCompleted += TrackingCompletedListener;

Then make some changes to persistent entities

    using (var session = Domain.OpenSession())
    using (var t = session.OpenTransaction()) {
      var e = new MyEntity(session);
      e.Text = "some text";
      t.Complete();
    }

And handle TrackingCompleted event call and do whatever you want with tracked changes

    private void TrackingCompletedListener(object sender, TrackingCompletedEventArgs e)
    {
      foreach (var change in e.Changes) {
        Console.WriteLine(change.Key);
        Console.WriteLine(change.State);

        foreach (var value in change.ChangedValues) {
          Console.WriteLine(value.Field.Name);
          Console.WriteLine(value.OriginalValue);
          Console.WriteLine(value.NewValue);
        }
      }
    }
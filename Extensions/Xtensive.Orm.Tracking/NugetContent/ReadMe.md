Xtensive.Orm.Tracking
=====================

Summary
-------
Provides tracking/auditing funtionality on Session/Domain level.

Prerequisites
-------------
DataObjects.Net 7.1.x or later (http://dataobjects.net)

Implementation
--------------

1. Include types from Xtensive.Orm.Tracking assembly into the domain:

```xml
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
```

2. To track changes on ```Session``` level obtain an instance of ```ISessionTrackingMonitor``` through ```Session.Services.Get<ISessionTrackingMonitor>()``` method; to track changes on ```Domain``` level (from all sessions) obtain an instance of ```IDomainTrackingMonitor``` through ```Domain.Services.Get<IDomainTrackingMonitor>()``` method
3. Subscribe to ```TrackingCompleted``` event. After each tracked transaction is committed you receive the ```TrackingCompletedEventArgs``` object.
4. ```TrackingCompletedEventArgs.Changes``` contains a collection of ```ITrackingItem``` objects, each of them represents a set of changes that occured to an ```Entity``` within the transaction committed.

Demo
----

First, subscribe to the ```TrackingCompleted``` event of ```ISessionTrackingMonitor``` / ```IDomainTrackingMonitor```

```csharp
var monitor = Domain.Services.Get<IDomainTrackingMonitor>();
monitor.TrackingCompleted += TrackingCompletedListener;
```

Then, do some changes to persistent entities

```csharp
using (var session = Domain.OpenSession()) {
  using (var t = session.OpenTransaction()) {
    var e = new MyEntity(session);
    e.Text = "some text";
    t.Complete();
  }
}
```

And handle ```TrackingCompleted``` event call and do whatever you want with tracked changes.

```csharp
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
```

## Overview

`ProducerProgress` is an [[IProgress<T>|http://msdn.microsoft.com/en-us/library/hh138298.aspx]] implementation that sends progress reports to an [[IProducerConsumerCollection<T>|http://msdn.microsoft.com/en-us/library/dd287147.aspx]].

## API

```C#
// A progress implementation that sends progress reports to a producer/consumer collection.
internal sealed class ProducerProgress<T> : IProgress<T>
{
  // Initializes a new instance of the ProducerProgress<T> class.
  public ProducerProgress(IProducerConsumerCollection<T> collection);
}
```

## Platform Support

This type is not supported on the following platforms:
* Windows Phone Silverlight 8.0 / 7.5.
* Silverlight 5 / 4.

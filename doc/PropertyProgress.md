## Overview

`PropertyProgress` is an [[IProgress<T>|http://msdn.microsoft.com/en-us/library/hh138298.aspx]] implementation that receives progress reports as a property update.

The most recent progress value can be retrieved by reading the `Progress` property, and you can monitor this property for changes using `INotifyPropertyChanged.PropertyChanged`. The `ProgressProperty` instance should be created on the UI thread.

## API

```C#
// A progress implementation that stores progress updates in a property.
// If this instance is created on a UI thread, its Progress property is suitable for data binding.
public sealed class PropertyProgress<T> : IProgress<T>, INotifyPropertyChanged
{
  // Initializes a new instance of the PropertyProgress<T> class.
  public PropertyProgress(T initialProgress = default(T));

  // The last reported progress value.
  public T Progress { get; }

  // Occurs when the property value changes.
  public event PropertyChangedEventHandler PropertyChanged;
}
```

## Platform Support

The full API is supported on all platforms.
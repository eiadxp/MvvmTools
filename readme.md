A tiny library to help cross-platform MVVM developer to be more productive by adding some shared and platform specific features:

# 1.Shared Library:
To be installed on any .NET Standard 2.0 compatible project, usually where you define your view models.
The main usage for this library is to help developer in creating commands in an easy way for the view models. It has also some basic classes to be used in the platform specified libraries (like command collection and basic implementation of event binding).

# 2.Platform Specific Library:
The main part of this library is to enable direct binding between events in your views and methods in the view models directly from XAML code Like:
```XAML
<Button Click="AcceptChanges(context)"/>
```




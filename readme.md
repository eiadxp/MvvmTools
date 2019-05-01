A tiny library to help cross-platform MVVM developer to be more productive by adding some shared and platform specific features:

# 1.Shared Library:
To be installed on any .NET Standard 2.0 compatible project, usually where you define your view models.
The main usage for this library is to help developer in creating commands in an easy way for the view models. It has also some basic classes to be used in the platform specified libraries (like command collection and basic implementation of event binding).
You can create commands in a simple way:
```C#
using System;
using System.Windows.Input;
using MvvmTools;

namespace Sample
{
    public class ViewModel
    {
        public ViewModel()
        {
            ShowModelsCountCommand = Command.Create(ShowModelsCount);
        }
        public List<Model> Models { get; set; }

        public void ShowModelsCount() => ShowMessage(Models?.Count);
        public ICommand ShowModelsCountCommand { get; private set; }
    }
}

```

# 2.Platform Specific Library:
The first part of this library enables direct binding between events in your views and methods in the view models directly from XAML code Like:
```XAML
<TextBox tools:Events.Bindings="LostFocus=Validate,GotFocus=Clear"/>
```
This will create two events bindings.... one will call method `Validate` on `LostFocus` event, and one will call `Clear` when event `GotFocus` is fired....
You can also pass single parameter to the method as Following: 
```xaml
<TextBox tools:Events.Bindings="LostFocus=Validate(!context),GotFocus=Clear(!sender)"/>
```

You can also use commands in the same way as methods and bind them to any event:
```xaml
<TextBox tools:Events.Bindings="LostFocus=ValidateCommand(!context),GotFocus=ClearCommand(!sender)"/>
```

The Second part will enable a quick preview of you view models and its data inside your designer to give a better way of previewing your interface at design time. You can write the following in your WPF application (the magic happens in the line `d:DataContext="{tools:DesignInstance Type=vm:ViewModel}"`):
```XAML
<Window x:Class="MvvmTools.Sample.WPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:MvvmTools.Sample.WPF"
        xmlns:tools="clr-namespace:MvvmTools.WPF;assembly=MvvmTools.WPF"
        xmlns:vm="clr-namespace:MvvmTools.Sample;assembly=MvvmTools.Sample"
        mc:Ignorable="d"
        d:DataContext="{tools:DesignInstance Type=vm:ViewModel}"
        Title="MainWindow">
</Window>
```

In your xamarin forms (also pay attention to `BindingContext="{tools:DesignInstance Type=vm:ViewModel}"`):
```XAML
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:MvvmTools.Sample.XForms"
             xmlns:vm="clr-namespace:MvvmTools.Sample;assembly=MvvmTools.Sample"
             xmlns:tools="clr-namespace:MvvmTools.XForms;assembly=MvvmTools.XForms"
             BindingContext="{tools:DesignInstance Type=vm:ViewModel}"
             x:Class="MvvmTools.Sample.XForms.MainPage">
</ContentPage>
```

![](https://github.com/eiadxp/MvvmTools/blob/master/images/Data%20preview%20xforms.png)

** PLEASE READ THE [WIKI](https://github.com/eiadxp/MvvmTools/wiki) FOR MORE INFORMATIONS **


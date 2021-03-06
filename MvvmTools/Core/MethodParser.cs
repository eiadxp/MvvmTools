﻿using System;

namespace MvvmTools.Core
{
    class MethodParser<TPlatform> where TPlatform : PlatformBase, new()
    {
        T ValueChanged<T>(T value)
        {
            _parameter = null;
            _methodSource = null;
            _methodName = "";
            return value;
        }
        ValueProvider<TPlatform> _parameter = null;
        ValueProvider<TPlatform> _methodSource = null;
        TPlatform _platform = new TPlatform();
        // _methodName may refer also to a property of type ICommand.
        string _methodName = "";
        Type _parameterType = null;

        private object _uIElement;
        private string _text;
        public object UIElement { get => _uIElement; set => _uIElement = ValueChanged(value); }
        public string Text { get => _text; set => _text = ValueChanged(value); }

        public void ExecuteFromEvent(object sender, object args, bool ignorExecute = false)
        {
            var source = GetMethodObject(sender, args);
            if (source == null) return;
            if (!ReflectionCash.HasMethodOrCommand(source.GetType(), _methodName))
                source = _platform.GetContext(source);
            if (_parameter == null)
            {
                ReflectionCash.ExecuteCommandOrMethod(source, _methodName, ignorExecute);
            }
            else
            {
                var parameter = _parameter.GetValueFromEvent(sender, args);
                ReflectionCash.ExecuteCommandOrMethod(source, _methodName, parameter, _parameterType, ignorExecute);
            }
        }
        public void ExecuteFromCommand(object commandParameter, bool ignorExecute = false)
        {
            var source = GetMethodObject(commandParameter);
            if (source == null) return;
            if (!ReflectionCash.HasMethod(source.GetType(), _methodName))
                source = _platform.GetContext(source);
            if (_parameter == null)
            {
                ReflectionCash.ExecuteMethod(source, _methodName, ignorExecute);
            }
            else
            {
                var parameter = _parameter.GetValueFromCommand(commandParameter);
                ReflectionCash.ExecuteMethod(source, _methodName, parameter, _parameterType, ignorExecute);
            }
        }
        public object GetMethodObject(object sender, object args)
        {
            if (UIElement == null) return null;
            if (string.IsNullOrWhiteSpace(Text)) return null;
            if (_methodSource == null) ParseMethod();
            return _methodSource.GetValueFromEvent(sender, args);
        }
        public object GetMethodObject(object commandParameter)
        {
            if (UIElement == null) return null;
            if (string.IsNullOrWhiteSpace(Text)) return null;
            if (_methodSource == null) ParseMethod();
            return _methodSource.GetValueFromCommand(commandParameter);
        }
        public void ParseMethod()
        {
            string s;
            int i = Text.IndexOf('(');
            i = i < 1 ? Text.LastIndexOf('.') : Text.LastIndexOf('.', i - 1);
            //i = Text.LastIndexOf(".");
            if (i > Text.Length - 2) throw new InvalidOperationException("Can not find method or command property name.");
            if (i < 1) //when no '.' is used that means the method name was used directly and it is in the data or binding context of UIElement.
            {
                s = "@context." + Text.Trim();
                i += "@context.".Length;
            }
            else
            {
                s = Text.Trim();
            }
            _methodSource = new ValueProvider<TPlatform>();
            _methodSource.UIElement = UIElement;
            _methodSource.Path = s.Substring(0, i);
            _methodName = s.Substring(i + 1); //Method name may be METHOD or METHOD() or METHOD(PARAMETER) or METHOD(TYPE PARAMETER)
            i = _methodName.IndexOf('(');
            if (i < 0) //Parameterless method.
            {
                _parameter = null;
                _parameterType = null;
                return;
            }
            if (!_methodName.EndsWith(")")) throw new InvalidOperationException("Can not find closing ')'.");
            s = _methodName.Substring(i + 1, _methodName.Length - i - 2);//s may be PARAMETER or TYPE PARAMETER
            _methodName = _methodName.Substring(0, i);
            i = s.IndexOf(' ');
            if (i < 1)
            {
                _parameterType = null;
            }
            else
            {
                _parameterType = ReflectionCash.GetType(s.Substring(0, i));
                s = s.Substring(i + 1);
            }
            if (!string.IsNullOrWhiteSpace(s))
            {
                _parameter = new ValueProvider<TPlatform>();
                _parameter.UIElement = UIElement;
                _parameter.Path = s;
            }
            else
            {
                _parameter = null;
            }
        }
    }
}

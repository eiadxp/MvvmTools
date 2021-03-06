﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmTools.Core
{
    /// <summary>
    /// This class provide a value from a string path and an UI element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the heart of this library. You should implement this class in every platform to enable translation from string paths
    /// into objects, methods.....
    /// </para>
    /// <para>We have provided some implementations in the platform specific libraries (Xamarin forms and WPF).</para>
    /// <para>
    /// By default the path will be applied to the data context of the UI element, unless you used a  named element (start with $),
    /// or used a keyword (this, args, sender, parameter, context, datacontext, or bindingcontext). The syntax of the path is:
    /// <code>[path_source.][Property1[.Property2.....]]</code>
    /// <para>
    /// where <c>path_source</c> is an optional reference to the object that will be used to get the value. This could be an element name
    /// (prefixed by $ sign), resource name (prefixed by # sign), or one of the following keyword (case insensitive):
    /// <list type="number">
    /// <item><c>this</c>: will use the <see cref="UIElement"/> property as source of path.</item>
    /// <item><c>args</c>: will use the event args as source of path.</item>
    /// <item><c>sender</c>: will use the event sender as source of path.</item>
    /// <item><c>parameter</c>: will use the command parameter as source of path.</item>
    /// <item><c>context | datacontext | bindingcontext</c>: will use the data (or binding) context of the <see cref="UIElement"/> as source of path.</item>
    /// </list>
    /// </para>
    /// <para>
    /// In case you did not use any of the previous options for <c>path_source</c>, the data (or binding) context of the 
    /// <see cref="UIElement"/> is used as source of path.
    /// </para>
    /// <para>
    /// You can use as many properties as you need, but they are case sensitive. The path could be like:
    /// <code>
    /// this.Text
    /// sender
    /// args.RowIndex
    /// parameter
    /// $txtUserName.Text
    /// #ContactsList.Count
    /// </code>
    /// </para>
    /// </para>
    /// </remarks>
    public sealed class ValueProvider<TPlatform> where TPlatform : PlatformBase, new()
    {
        private TPlatform platform = new TPlatform();
        private string _path;
        private string[] _pathItems;


        /// <summary>
        /// UI element to be used with this object.
        /// </summary>
        public object UIElement { get; set; }
        /// <summary>
        /// Path to be used to get value from <see cref="UIElement"/>.
        /// </summary>
        public string Path
        {
            get => _path; set
            {
                _path = value;
                if (string.IsNullOrWhiteSpace(value)) _pathItems = null;
                _pathItems = value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        internal object GetValueFromEvent(object sender, object args) => GetValue(sender, args, null, false);
        internal object GetValueFromCommand(object parameter) => GetValue(null, null, parameter, true);

        object GetValue(object sender, object args, object parameter, bool isCommand)
        {
            if (string.IsNullOrWhiteSpace(Path)) return null;
            if (_pathItems.Length == 0) return null;
            object dataObject;
            int startIndex = 1;
            bool isElementName = false;
            switch (_pathItems[0].ToLower())
            {
                case "@args":
                case "!args":
                    if (isCommand) throw new InvalidOperationException("Can not use 'args' in commands.");
                    dataObject = args;
                    break;
                case "@sender":
                case "!sender":
                    if (isCommand) throw new InvalidOperationException("Can not use 'sender' in commands.");
                    dataObject = sender;
                    break;
                case "@parameter":
                case "!parameter":
                    if (!isCommand) throw new InvalidOperationException("Can not use 'parameter' in events.");
                    dataObject = sender;
                    break;
                case "@context":
                case "@datacontext":
                case "@bindingcontext":
                case "!context":
                case "!datacontext":
                case "!bindingcontext":
                    dataObject = platform.GetContext(UIElement);
                    break;
                case "@this":
                case "!this":
                    dataObject = UIElement;
                    break;
                default:
                    if (_pathItems[0].StartsWith("$"))
                    {
                        dataObject = platform.GetElement(UIElement, _pathItems[0].Substring(1));
                        isElementName = true;
                    }
                    else if (_pathItems[0].StartsWith("#"))
                    {
                        dataObject = platform.GetResource(UIElement, _pathItems[0].Substring(1));
                    }
                    else if (_pathItems[0].StartsWith("!"))
                    {
                        try
                        {
                            dataObject = platform.GetElement(UIElement, _pathItems[0].Substring(1));
                            isElementName = dataObject!= null;
                        }
                        catch (Exception)
                        {
                            dataObject = null;
                        }
                        if (dataObject == null) dataObject = platform.GetResource(UIElement, _pathItems[0].Substring(1));
                    }
                    else
                    {
                        dataObject = platform.GetContext(UIElement);
                        startIndex = 0;
                    }

                    break;
            }
            for (int i = startIndex; i < _pathItems.Length; i++)
            {
                if (dataObject == null) break;
                try
                {
                    dataObject = ReflectionCash.GetPropertyValue(dataObject, _pathItems[i]);
                    isElementName = false;
                }
                catch (Exception)
                {
                    if (isElementName)
                    {
                        dataObject = platform.GetContext(dataObject);
                        isElementName = false;
                        dataObject = ReflectionCash.GetPropertyValue(dataObject, _pathItems[i]);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return dataObject;
        }
    }
}

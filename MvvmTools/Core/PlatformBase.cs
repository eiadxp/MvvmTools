using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmTools.Core
{
    public abstract class PlatformBase
    {
        public PlatformBase()
        {
            if (!IsInit)
            {
                Initialize();
                IsInit = true;
            }
        }
        static bool IsInit;
        /// <summary>
        /// This function is called only once in the application to do some platform initializing, 
        /// like setting default events.
        /// </summary>
        protected virtual void Initialize() { }
        /// <summary>
        /// Get user UI element by name.
        /// </summary>
        /// <param name="name">Name of UI element</param>
        /// <returns>Reference to UI element if found, or null if not.</returns>
        /// <remarks>
        /// Searches for UI element by name is done usually by using <see cref="UIElement"/>, but in some cases this methods is called 
        /// before defining <see cref="UIElement"/>, or before attaching it to the UI parent, in that case it must return null.
        /// </remarks>
        internal protected abstract object GetElement(object element, string name);
        /// <summary>
        /// Get a resource by key.
        /// </summary>
        /// <param name="Key">Key of the resource.</param>
        /// <returns>Resource if found, or null if not.</returns>
        /// <remarks>
        /// Searches for UI resources by key is done usually by using <see cref="UIElement"/>, but in some cases this methods is called 
        /// before defining <see cref="UIElement"/>, or before attaching it to the UI parent, in that case it must return null.
        /// </remarks>
        internal protected abstract object GetResource(object element, string Key);
        /// <summary>
        /// Gets the data (or binding) context of th <see cref="UIElement"/>.
        /// </summary>
        /// <returns>The data (or binding) context of <see cref="UIElement"/> if it is set, other wise it will returns null.</returns>
        /// <remarks>If <see cref="UIElement"/> is null, this method should return null.</remarks>
        internal protected abstract object GetContext(object element);
        /// <summary>
        /// Define weather we are in design mode or not.
        /// </summary>
        internal protected abstract bool IsDesignMode { get; }
    }
}

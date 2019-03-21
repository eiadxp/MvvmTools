using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace MvvmTools.Commands
{
    public class CommandsCollection : CommandBase, IList<ICommand>, IList
    {
        readonly List<ICommand> list = new List<ICommand>();
        #region IList
        int IList.Add(object value)
        {
            Add((ICommand)value);
            return Count - 1;
        }

        void IList.Clear() => Clear();
        bool IList.Contains(object value) => Contains((ICommand)value);
        int IList.IndexOf(object value) => IndexOf((ICommand)value);
        void IList.Insert(int index, object value) => Insert(index, (ICommand)value);
        void IList.Remove(object value) => Remove((ICommand)value);
        void IList.RemoveAt(int index) => RemoveAt(index);
        void ICollection.CopyTo(Array array, int index) => ((IList)list).CopyTo(array, index);

        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        int ICollection.Count => Count;
        bool ICollection.IsSynchronized => ((IList)list).IsSynchronized;
        object ICollection.SyncRoot => ((IList)list).SyncRoot;
        object IList.this[int index] { get => this[index]; set => this[index] = (ICommand)value; }
        #endregion
        #region IList<ICommand>
        /// <inheritdoc/>
        public void Add(ICommand item)
        {
            list.Add(item);
            SubscribeToCommand(item);
        }
        /// <inheritdoc/>
        public void Insert(int index, ICommand item)
        {
            list.Insert(index, item);
            SubscribeToCommand(item);
        }
        /// <inheritdoc/>
        public void Clear()
        {
            foreach (var item in list)
            {
                UnsbscribeToCommand(item);
            }
            list.Clear();
        }
        /// <inheritdoc/>
        public bool Remove(ICommand item)
        {
            var b = list.Remove(item);
            if (b) UnsbscribeToCommand(item);
            return b;
        }
        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            var a = list[index];
            list.RemoveAt(index);
            UnsbscribeToCommand(a);
        }
        /// <inheritdoc/>
        public ICommand this[int index]
        {
            get => list[index];
            set
            {
                UnsbscribeToCommand(list[index]);
                list[index] = value;
                SubscribeToCommand(value);
            }
        }

        /// <inheritdoc/>
        public int Count => list.Count;
        /// <inheritdoc/>
        public bool IsReadOnly => false;
        /// <inheritdoc/>
        public bool Contains(ICommand item) => list.Contains(item);
        /// <inheritdoc/>
        public void CopyTo(ICommand[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
        /// <inheritdoc/>
        public IEnumerator<ICommand> GetEnumerator() => list.GetEnumerator();
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <inheritdoc/>
        public int IndexOf(ICommand item) => list.IndexOf(item);
        #endregion
        #region CommandBase
        /// <inheritdoc/>
        public override bool CanExecute(object parameter) => alwaysCanExecute ? true : list.All(c => c.CanExecute(parameter));
        /// <inheritdoc/>
        public override void Execute(object parameter)
        {
            foreach (var item in list)
            {
                try
                {
                    item.Execute(parameter);
                }
                catch (Exception)
                {
                    if (ExceptionBehaviour == ExceptionBehaviour.Throw)
                        throw;
                    else if (ExceptionBehaviour == ExceptionBehaviour.Break)
                        return;
                    else if (ExceptionBehaviour == ExceptionBehaviour.Ignore) { }
                }
            }
        }

        #endregion
        #region Internals
        void SubscribeToCommand(ICommand command)
        {
            if (command == null) return;
            command.CanExecuteChanged += Command_CanExecuteChanged;
            _CanExecuteChanged();
        }
        void UnsbscribeToCommand(ICommand command)
        {
            if (command == null) return;
            command.CanExecuteChanged -= Command_CanExecuteChanged;
            _CanExecuteChanged();
        }
        void Command_CanExecuteChanged(object sender, EventArgs e) => _CanExecuteChanged();
        void _CanExecuteChanged()
        {
            if (!alwaysCanExecute) NotifyCanExecuteChanged();
        }
        #endregion

        private bool alwaysCanExecute = true;
        public bool AlwaysCanExecute
        {
            get { return alwaysCanExecute; }
            set
            {
                alwaysCanExecute = value;
                NotifyCanExecuteChanged();
            }
        }
        public ExceptionBehaviour ExceptionBehaviour { get; set; } = ExceptionBehaviour.Ignore;

    }
    public enum ExceptionBehaviour
    {
        Ignore,
        Throw,
        Break
    }
}

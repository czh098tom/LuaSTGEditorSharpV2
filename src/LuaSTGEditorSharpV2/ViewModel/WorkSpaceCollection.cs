using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class WorkSpaceCollection<T> : ObservableCollection<T>
        where T : DockingViewModelBase
    {
        private readonly Dictionary<Type, T> _typeIndex = [];

        public T this[Type ty] => _typeIndex[ty];

        public U? GetViewModelOfType<U>() where U : T
        { 
            return this[typeof(U)] as U;
        }

        public int FindIndex(T? obj)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == obj) return i;
            }
            return -1;
        }

        public int FindViewModelIndexOfType(Type type)
        {
            var inst = this[type];
            if (inst == null) return -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == inst) return i;
            }
            return -1;
        }

        public int FindViewModelIndexOfType<U>() where U : T
        {
            var inst = GetViewModelOfType<U>();
            if (inst == null) return -1;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == inst) return i;
            }
            return -1;
        }

        protected override void InsertItem(int index, T item)
        {
            _typeIndex.Add(item.GetType(), item);
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            _typeIndex.Remove(this[index].GetType());
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            _typeIndex.Remove(this[index].GetType());
            _typeIndex.Add(item.GetType(), item);
            base.SetItem(index, item);
        }

        protected override void ClearItems()
        {
            _typeIndex.Clear();
            base.ClearItems();
        }
    }
}

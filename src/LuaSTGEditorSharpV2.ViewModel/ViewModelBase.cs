using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected static NotifyCollectionChangedEventHandler
            GetHookItemEventsMarshallingHandler<TItem>(Action<TItem> hook)
            => GetHookItemEventsMarshallingHandler(hook, _ => { });

        protected static NotifyCollectionChangedEventHandler
            GetHookItemEventsMarshallingHandler<TItem>(
                Action<TItem> hook,
                Action<TItem> unhook)
        {
            return (sender, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var newItem in e.NewItems!)
                        {
                            if (newItem is TItem item)
                            {
                                hook(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (var oldItem in e.OldItems!)
                        {
                            if (oldItem is TItem item)
                            {
                                unhook(item);
                            }
                        }
                        foreach (var newItem in e.NewItems!)
                        {
                            if (newItem is TItem item)
                            {
                                hook(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var oldItem in e.OldItems!)
                        {
                            if (oldItem is TItem item)
                            {
                                unhook(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Reset:
                        break;
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

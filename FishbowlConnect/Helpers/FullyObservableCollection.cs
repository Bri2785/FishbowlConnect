using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FishbowlConnect.Helpers
{

    public class FullyObservableCollection<T> : ObservableCollection<T>
        //where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property is changed within an item.
        /// </summary>
        public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;

        public FullyObservableCollection() : base()
        { }

        public FullyObservableCollection(List<T> list) : base(list)
        {
            ObserveAll();
        }

        public FullyObservableCollection(IEnumerable<T> enumerable) : base(enumerable)
        {
            ObserveAll();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                //foreach (T item in e.OldItems)
                //if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T)))
                //{
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= ChildPropertyChanged;

            }

            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += ChildPropertyChanged;
            }

            base.OnCollectionChanged(e);
        }

        protected void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(this, e);
        }

        protected void OnItemPropertyChanged(int index, PropertyChangedEventArgs e)
        {
            OnItemPropertyChanged(new ItemPropertyChangedEventArgs(index, e));
        }

        protected override void ClearItems()
        {
            foreach (INotifyPropertyChanged item in Items)
                item.PropertyChanged -= ChildPropertyChanged;

            base.ClearItems();
        }

        private void ObserveAll()
        {
            foreach (INotifyPropertyChanged item in Items)
                item.PropertyChanged += ChildPropertyChanged;
        }

        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T typedSender = (T)sender;
            int i = Items.IndexOf(typedSender);

            if (i < 0)
                throw new ArgumentException("Received property notification from item not in collection");

            OnItemPropertyChanged(i, e);
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, System.ComponentModel.ListSortDirection direction)
        {
            switch (direction)
            {
                case System.ComponentModel.ListSortDirection.Ascending:
                    {
                        ApplySort(Items.OrderBy(keySelector));
                        break;
                    }
                case System.ComponentModel.ListSortDirection.Descending:
                    {
                        ApplySort(Items.OrderByDescending(keySelector));
                        break;
                    }
            }
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            ApplySort(Items.OrderBy(keySelector, comparer));
        }

        private void ApplySort(IEnumerable<T> sortedItems)
        {
            var sortedItemsList = sortedItems.ToList();

            foreach (var item in sortedItemsList)
            {
                Move(IndexOf(item), sortedItemsList.IndexOf(item));
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="FullyObservableCollection{T}.ItemPropertyChanged"/> event.
    /// </summary>
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the index in the collection for which the property change has occurred.
        /// </summary>
        /// <value>
        /// Index in parent collection.
        /// </value>
        public int CollectionIndex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index in the collection of chnaged item.</param>
        /// <param name="name">The name of the property that changed.</param>
        public ItemPropertyChangedEventArgs(int index, string name) : base(name)
        {
            CollectionIndex = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : this(index, args.PropertyName)
        { }
    }

    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public void Sort<TKey>(Func<T, TKey> keySelector, System.ComponentModel.ListSortDirection direction)
        {
            switch (direction)
            {
                case System.ComponentModel.ListSortDirection.Ascending:
                    {
                        ApplySort(Items.OrderBy(keySelector));
                        break;
                    }
                case System.ComponentModel.ListSortDirection.Descending:
                    {
                        ApplySort(Items.OrderByDescending(keySelector));
                        break;
                    }
            }
        }

        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            ApplySort(Items.OrderBy(keySelector, comparer));
        }

        private void ApplySort(IEnumerable<T> sortedItems)
        {
            var sortedItemsList = sortedItems.ToList();

            foreach (var item in sortedItemsList)
            {
                Move(IndexOf(item), sortedItemsList.IndexOf(item));
            }
        }
    }
}

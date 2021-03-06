﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Mirko.ViewModel
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public ObservableCollectionEx()
            : base()
        {
        }

        public ObservableCollectionEx(IEnumerable<T> col)
            : base(col)
        {
        }

        public void PrependRange(IEnumerable<T> col)
        {
            if (col != null && col.Count() > 0)
            {
                var moveEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this.Items, col.Count(), 0);
                base.OnCollectionChanged(moveEventArgs);

                var reversed = col.Reverse();
                foreach (var item in col.Reverse())
                    base.Insert(0, item);
            }
        }

        public void AddRange(IEnumerable<T> col)
        {
            if (col != null && col.Count() > 0)
            {
                var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, col, base.Items.Count);

                var list = base.Items as List<T>;
                list.AddRange(col);

                base.OnCollectionChanged(eventArgs);
                base.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            }
        }

        public List<T> GetRange(int startIndex, int count)
        {
            var list = new List<T>(count);
            var lastIndex = startIndex + count;

            if (lastIndex > this.Count)
                lastIndex = this.Count;

            for (int i = startIndex; i < lastIndex; i++)
                list.Add(this.Items[i]);

            return list;
        }

        public void Replace(int index, T item)
        {
            if (item != null && index >= 0 && index < this.Count)
            {
                var oldItem = this.Items[index];
                this.Items.RemoveAt(index);
                this.Items.Insert(index, item);

                var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index);
                base.OnCollectionChanged(eventArgs);
            }
        }

        public int GetIndex(T item)
        {
            if (item == null || this.Count == 0) return -1;

            for (int i = 0; i < this.Count; i++)
            {
                var currentItem = this.Items[i];
                if (currentItem != null && currentItem.Equals(item))
                    return i;
            }

            return -1; // not found
        }

        public void Sort()
        {
            var sorted = this.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                this.Move(this.IndexOf(sorted[i]), i);
        }
    }
}

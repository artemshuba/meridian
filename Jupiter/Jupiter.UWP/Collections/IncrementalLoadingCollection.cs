using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml.Data;

namespace Jupiter.Collections
{
    /// <summary>
    /// This class can used as a jumpstart for implementing ISupportIncrementalLoading. 
    /// Implementing the ISupportIncrementalLoading interfaces allows you to create a list that loads
    /// more data automatically when the user scrolls to the end of of a GridView or ListView.
    /// </summary>
    public class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        /// <summary>
        /// Instead of deriving from this class we provide external method for loading more items
        /// </summary>
        public Func<CancellationToken, uint, Task<List<T>>> OnMoreItemsRequested;

        public Func<bool> HasMoreItemsRequested;

        public IncrementalLoadingCollection()
        {

        }

        public IncrementalLoadingCollection(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                this.Add(list[i]);
            }
        }

        #region ISupportIncrementalLoading

        public bool HasMoreItems
        {
            get
            {
                if (HasMoreItemsRequested != null)
                    return HasMoreItemsRequested();

                return HasMoreItemsOverride();
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            _busy = true;

            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        #endregion

        #region Private methods

        async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken token, uint count)
        {
            uint c = 0;
            try
            {
                List<T> items = null;
                if (OnMoreItemsRequested != null)
                    items = await OnMoreItemsRequested(token, count);
                else
                    items = await LoadMoreItemsOverrideAsync(token, count);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        Add(item);
                    }

                    c = (uint)items.Count;
                }
            }
            finally
            {
                _busy = false;
            }

            return new LoadMoreItemsResult() { Count = c };
        }

        #endregion

        #region Overridable methods

        protected virtual Task<List<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            return null;
        }

        protected virtual bool HasMoreItemsOverride()
        {
            return false;
        }

        #endregion

        #region State

        //List<T> _storage = new List<T>();
        bool _busy = false;

        #endregion
    }
}

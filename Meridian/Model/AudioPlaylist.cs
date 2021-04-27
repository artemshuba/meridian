using Jupiter.Utils.Extensions;
using Meridian.Enum;
using Meridian.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Meridian.Model
{
    public class AudioPlaylist
    {
        private IAudio _currentItem;
        private int _currentIndex = -1;
        private IList<IAudio> _originalItems;
        private IList<IAudio> _items;
        private bool _shuffle;

        public event EventHandler<IAudio> OnCurrentItemChanged;

        /// <summary>
        /// Items
        /// </summary>
        public IList<IAudio> Items
        {
            get { return _items; }
            set
            {
                if (Shuffle)
                {
                    _originalItems = value.ToList(); //save original playlist
                    _items = value;
                    _items.Shuffle();
                }
                else
                {
                    _originalItems = value;
                    _items = value;
                }
            }
        }

        /// <summary>
        /// Current audio
        /// </summary>
        public IAudio CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem == value)
                    return;

                _currentItem = value;
                _currentIndex = Items.IndexOf(value);
                OnCurrentItemChanged?.Invoke(this, _currentItem);
            }
        }

        /// <summary>
        /// Repeate mode
        /// </summary>
        public RepeatMode Repeat { get; set; }

        /// <summary>
        /// Shuffle mode
        /// </summary>
        public bool Shuffle
        {
            get { return _shuffle; }
            set
            {
                if (_shuffle == value)
                    return;

                _shuffle = value;
                if (value)
                {
                    _items = new List<IAudio>(_originalItems.ToList()); //copy original playlist to current
                    _items.Shuffle(); //shuffle
                }
                else
                {
                    _items = new List<IAudio>(_originalItems);
                }

                //update current item
                if (_currentIndex >= 0 && _currentIndex < _items.Count && _currentItem == null)
                    _currentItem = _items[_currentIndex];
            }
        }

        public AudioPlaylist()
        {
            Items = new ObservableCollection<IAudio>();
        }

        public AudioPlaylist(IList<IAudio> source)
        {
            Items = new ObservableCollection<IAudio>(source);

            if (source.Count > 0)
            {
                _currentItem = source.First();
                _currentIndex = 0;
            }
        }

        /// <summary>
        /// Move to next track. After finishing this method, CurrentAudio will contain next track (if possible).
        /// </summary>
        public void MoveNext(bool skip = false)
        {
            if (Items.Count == 0)
                return;

            if (Repeat == RepeatMode.Always && !skip)
            {
                OnCurrentItemChanged?.Invoke(this, _currentItem);
                return;
            }

            var index = _currentIndex;

            index++;

            if (index >= Items.Count)
            {
                if (!skip)
                    return;

                index = 0;
            }
            MoveTo(index);
        }

        /// <summary>
        /// Move to previous track. After finishing this method, CurrentAudio will contain previous track (if possible).
        /// </summary>
        public void MovePrevious()
        {
            if (Items.Count == 0)
                return;

            var index = _currentIndex;

            if (index > 0)
                index--;
            else
                index = Items.Count - 1;

            MoveTo(index);
        }

        /// <summary>
        /// Move to track by index. After finishing this method, CurrentAudio will contain specified track (if possible).
        /// </summary>
        /// <param name="index"></param>
        public void MoveTo(int index)
        {
            if (!(index < Items.Count))
                throw new ArgumentOutOfRangeException(nameof(index), "Index is greater than items count");

            if (index == _currentIndex)
                return;

            _currentIndex = index;
            CurrentItem = Items[_currentIndex];
        }

        /// <summary>
        /// Add track after current
        /// </summary>
        public void AddAfterCurrent(IAudio item)
        {
            Items.Insert(_currentIndex + 1, item);
        }

        /// <summary>
        /// Add track
        /// </summary>
        public void Add(IAudio item)
        {
            Items.Add(item);
        }

        /// <summary>
        /// Remove track
        /// </summary>
        public void Remove(IAudio item)
        {
            if (item == CurrentItem)
            {
                _currentIndex++;
                if (_currentIndex > Items.Count - 1)
                    _currentIndex = 0;
            }

            Items.Remove(item);
        }

        /// <summary>
        /// Removes all tracks except current
        /// </summary>
        public void ClearAllExceptCurrent()
        {
            int i = 0;
            while (i < Items.Count)
            {
                if (Items[i] != CurrentItem)
                    Items.RemoveAt(i);
                else
                    i++;
            }

            _currentIndex = 0;
        }

        /// <summary>
        /// Removes all tracks
        /// </summary>
        public void ClearAll()
        {
            Items.Clear();
        }

        public string Serialize()
        {
            var o = new
            {
                items = Items,
                //originalItem = Shuffle ? _originalItems : null, //TODO someday
                shuffle = Shuffle,
                repeat = Repeat,
                currentIndex = _currentIndex
            };

            return JsonConvert.SerializeObject(o, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
        }

        public void Deserialize(string json)
        {
            var o = JObject.Parse(json);
            if (o["shuffle"] != null)
                _shuffle = o["shuffle"].Value<bool>();

            if (o["repeat"] != null)
            {
                var x = o["repeat"].Value<int>();
                Repeat = (RepeatMode)x;
            }

            _originalItems = o["items"].ToObject<ObservableCollection<IAudio>>(JsonSerializer.CreateDefault(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects }));
            _items = _originalItems;
            _currentIndex = o["currentIndex"].Value<int>();
            if (_currentIndex >= 0 && _currentIndex < _items.Count)
                _currentItem = _items[_currentIndex];
            else if (_items.Count > 0)
                _currentItem = _items[0];
        }
    }
}

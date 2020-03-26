using Barebones.MasterServer;
using Barebones.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GW.Master
{
    /* Represents clients friendlist, which emits events about changes.
     * Client, gameserver and master server will create a similar object. */
    public class ObservableFriendList : IEnumerable<IObservableProperty>
    {
        //Profile properties list
        private Dictionary<short, IObservableProperty> _properties;

        //Properties that have change and are to be saved
        public HashSet<IObservableProperty> UnsavedProperties { get; protected set; }

        //Properties that have been changed and waiting to be sent.
        private HashSet<IObservableProperty> _notBroadcasteedProperties;

        public event Action<short, IObservableProperty> OnPropertyUpdatedEvent;

        public event Action<ObservableFriendList> OnModifiedEvent;

        public ObservableFriendList()
        {
            _properties = new Dictionary<short, IObservableProperty>();
            UnsavedProperties = new HashSet<IObservableProperty>();
            _notBroadcasteedProperties = new HashSet<IObservableProperty>();
        }

        //Check if friendlist has changed properties
        public bool HasDirtyProperties {  get { return _notBroadcasteedProperties.Count > 0; } }

        //The number of proerties the friendlist has
        public int PropertyCount { get { return _properties.Count; } }

        //Returns an observable value of a given type
        public T GetProperty<T>(short key) where T : class, IObservableProperty
        {
            return _properties[key].CastTo<T>();
        }

        //Returns an observable value
        public IObservableProperty GetProperty(short key)
        {
            return _properties[key];
        }

        //Tries to get a friendlist property
        public bool TryGetProperty<t>(short key, out t result) where t :class, IObservableProperty
        {
            bool getResult = _properties.TryGetValue(key, out IObservableProperty val);
            result = val as t;
            return getResult;
        }

        //Add a value to friendlist
        public void AddProperty(IObservableProperty property)
        {
            _properties.Add(property.Key, property);
            property.OnDirtyEvent += OnDirtyProperty;
        }

        //Add a property to friendlist
        public void Add(IObservableProperty property)
        {
            AddProperty(property);
        }

        //Called when a value becomes dirty
        protected virtual void OnDirtyProperty(IObservableProperty property)
        {
            _notBroadcasteedProperties.Add(property);
            UnsavedProperties.Add(property);

            OnModifiedEvent?.Invoke(this);
        }

        //Writes all data from friendlist to buffer
        public byte[] ToBytes()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new EndianBinaryWriter(EndianBitConverter.Big, stream))
                {
                    //Write count
                    writer.Write(_properties.Count);

                    foreach(var value in _properties)
                    {
                        //Write Key
                        writer.Write(value.Key);

                        var data = value.Value.ToBytes();

                        //Write data length
                        writer.Write(data.Length);

                        //Write data
                        writer.Write(data);
                    }
                }
                return stream.ToArray();
            }
        }

        //Restores friendlist from data in the buffer
        public void FromBytes(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new EndianBinaryReader(EndianBitConverter.Big, ms))
                {
                    var count = reader.ReadInt32();

                    for(int i=0; i < count; i++)
                    {
                        var key = reader.ReadInt16();
                        var length = reader.ReadInt32();
                        var valueData = reader.ReadBytes(length);

                        if(!_properties.ContainsKey(key))
                        {
                            continue;
                        }

                        _properties[key].FromBytes(valueData);
                    }
                }
            }
        }

        //Restores friendlist from a dictionary of strings
        public void FromStrings(Dictionary<short, string> dataData)
        {
            foreach(var pair in dataData)
            {
                IObservableProperty property;
                _properties.TryGetValue(pair.Key, out property);
                if(property != null)
                {
                    property.Deserialize(pair.Value);
                }
            }
        }

        //Write changes into the writer
        public void GetUpdates(EndianBinaryWriter writer)
        {
            //Write values count
            writer.Write(_notBroadcasteedProperties.Count);

            foreach(var property in _notBroadcasteedProperties)
            {
                //Write key
                writer.Write(property.Key);

                var updates = property.GetUpdates();

                //Write updates length
                writer.Write(updates.Length);

                //Write actual updates
                writer.Write(updates);
            }
        }

        public void ClearUpdates()
        {
            foreach(var property in _notBroadcasteedProperties)
            {
                property.ClearUpdates();
            }

            _notBroadcasteedProperties.Clear();
        }

        //Use update data to upadte values in friendlist
        public void ApplyUpdates(byte[] updates)
        {
            using (var ms = new MemoryStream(updates))
            {
                using (var reader = new EndianBinaryReader(EndianBitConverter.Big, ms))
                {
                    ApplyUpdates(reader);
                }
            }
        }

        public void ApplyUpdates(EndianBinaryReader reader)
        {
            var count = reader.ReadInt32();

            var dataRead = new Dictionary<short, byte[]>(count);

            //Read data first. In case of an exception we want the pointer of reader
            //to be at the right place (at the end of current updates)
            for(var i=0; i < count; i++)
            {
                var key = reader.ReadInt16();

                var dataLength = reader.ReadInt32();

                var data = reader.ReadBytes(dataLength);

                if(!dataRead.ContainsKey(key))
                {
                    dataRead.Add(key, data);
                }
            }

            //Update observables
            foreach (var updateEntry in dataRead)
            {
                if(_properties.TryGetValue(updateEntry.Key, out IObservableProperty property))
                {
                    property.ApplyUpdates(updateEntry.Value);
                }
            }
        }

        //Serialize all properties into short/string dictionary
        public Dictionary<short, string> ToStringsDictionary()
        {
            var dict = new Dictionary<short, string>();

            foreach(var pair in _properties)
            {
                dict.Add(pair.Key, pair.Value.Serialize());
            }

            return dict;
        }

        public IEnumerator<IObservableProperty> GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
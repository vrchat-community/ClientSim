using System.Collections.Generic;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// This class is for holding a container of objects that can be deleted at any time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientSimObjectCollection<T>
    {
        private bool _shouldVerifyObjectList;
        private readonly Queue<T> _toBeAdded = new Queue<T>();
        private readonly Queue<T> _toBeRemoved = new Queue<T>();
        private List<T> _allObjects = new List<T>();

        public void AddObject(T obj)
        {
            if (obj == null)
            {
                return;
            }
            _toBeAdded.Enqueue(obj);
        }

        public void RemoveObject(T obj)
        {
            _shouldVerifyObjectList = true;
            _toBeRemoved.Enqueue(obj);
        }

        public void ShouldVerifyObjects()
        {
            _shouldVerifyObjectList = true;
        }

        public void ProcessAddedAndRemovedObjects()
        {
            if (_toBeAdded.Count > 0)
            {
                foreach (var objs in _toBeAdded)
                {
                    if (objs == null)
                    {
                        _shouldVerifyObjectList = true;
                        continue;
                    }
                    _allObjects.Add(objs);
                }
                _toBeAdded.Clear();
            }
            if (_toBeRemoved.Count > 0)
            {
                foreach (var objs in _toBeRemoved)
                {
                    if (objs == null)
                    {
                        _shouldVerifyObjectList = true;
                        continue;
                    }
                    _allObjects.Remove(objs);
                }
                _toBeRemoved.Clear();
            }

            if (_shouldVerifyObjectList)
            {
                List<T> allObjs = new List<T>();
                foreach (var objs in _allObjects)
                {
                    if (objs == null)
                    {
                        continue;
                    }
                    allObjs.Add(objs);
                }

                _allObjects = allObjs;
            }
        }

        public IEnumerable<T> GetObjects()
        {
            foreach (var obj in _allObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                yield return obj;
            }
        }
    }
}
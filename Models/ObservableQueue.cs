using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace YoloV7WebCamInference.Models
{
    public class ObservableQueue<T> : INotifyCollectionChanged, IEnumerable<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly Queue<T> queue = new();

        public void Enqueue(T item)
        {
            queue.Enqueue(item);
            CollectionChanged?.Invoke(this,
    new NotifyCollectionChangedEventArgs(
        NotifyCollectionChangedAction.Add, item));
        }

        public T Dequeue()
        {
            var item = queue.Dequeue();
            CollectionChanged?.Invoke(this,
    new NotifyCollectionChangedEventArgs(
        NotifyCollectionChangedAction.Remove, item, 0));
            return item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

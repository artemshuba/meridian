using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Neptune.Helpers
{
    /// <summary>
    /// A simple task queue
    /// </summary>
    public class TaskQueue
    {
        private readonly Queue<Task> _queue;
        private bool _isBusy;

        public int Count
        {
            get { return _queue.Count; }
        }

        public TaskQueue()
        {
            _queue = new Queue<Task>();
        }

        public void Enqueue(Task t)
        {
            EnqueuInternal(t);
        }

        public void Enqueue(Func<Task> action)
        {
            var t = new Task(() => action.Invoke().Wait());
            EnqueuInternal(t);
        }

        private void EnqueuInternal(Task t)
        {
            t.ContinueWith(TaskCallback);

            if (!_isBusy)
            {
                _isBusy = true;
                if (t.Status == TaskStatus.Created)
                {
                    t.Start();
                    return;
                }
            }

            _queue.Enqueue(t);
        }

        private void TaskCallback(Task t)
        {
            Debug.WriteLine("Task " + t.Id + " completed.");
            if (_queue.Count > 0)
            {
                var nextTask = _queue.Dequeue();
                t.ContinueWith(t1 => nextTask);
                if (nextTask.Status == TaskStatus.Created)
                {
                    Debug.WriteLine("Starting next " + nextTask.Id + " task.");
                    nextTask.Start();
                }
            }
            else
            {
                _isBusy = false;
            }
        }
    }
}

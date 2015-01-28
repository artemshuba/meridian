using System.Collections.Generic;
using Meridian.Helpers;

namespace Meridian.ViewModel
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        private bool _isWorking;
        private readonly Dictionary<string, LongRunningOperation> _tasks = new Dictionary<string, LongRunningOperation>();

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                Set(ref _isWorking, value);
            }
        }

        /// <summary>
        /// Calls when viewmodel is activated
        /// </summary>
        public virtual void Activate()
        {
            
        }

        /// <summary>
        /// Calls when viewmodel is deactivated
        /// </summary>
        public virtual void Deactivate()
        {

        }

        public Dictionary<string, LongRunningOperation> Tasks
        {
            get { return _tasks; }
        }

        #region Long Running Operations helpers

        private void RegisterTask(string id)
        {
            _tasks.Add(id, new LongRunningOperation());
        }

        protected void RegisterTasks(params string[] ids)
        {
            foreach (var id in ids)
            {
                RegisterTask(id);
            }
        }

        protected void OnTaskStarted(string id)
        {
            _tasks[id].Error = null;
            _tasks[id].IsWorking = true;
        }

        protected void OnTaskFinished(string id)
        {
            _tasks[id].IsWorking = false;
        }

        protected void OnTaskError(string id, string error)
        {
            _tasks[id].Error = error;
            _tasks[id].IsWorking = false;
        }


        #endregion
    }
}

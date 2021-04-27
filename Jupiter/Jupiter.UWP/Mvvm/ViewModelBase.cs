using System.Collections.Generic;
using Microsoft.UI.Xaml.Navigation;
using Jupiter.Services.Navigation;
using Jupiter.Application;

namespace Jupiter.Mvvm
{
    /// <summary>
    /// Base ViewModel class
    /// </summary>
    public abstract class ViewModelBase : BindableBase, INavigable
    {
        private readonly OperationTokenCollection _operations = new OperationTokenCollection();

        public NavigationService NavigationService { get; set; }

        public OperationTokenCollection Operations
        {
            get { return _operations; }
        }

        public virtual IStateItems SessionState { get; set; }

        protected ViewModelBase()
        {
            InitializeCommands();
        }

        /// <summary>
        /// Will be called after navigation to view associated with this ViewModel.
        /// </summary>
        public virtual void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
        }

        /// <summary>
        /// Will be called on navigating from view associated with this ViewModel.
        /// </summary>
        public virtual void OnNavigatingFrom(NavigatingEventArgs e)
        {
        }

        /// <summary>
        /// A method for initializing commands
        /// </summary>
        protected virtual void InitializeCommands()
        {

        }

        #region OperationToken helpers

        /// <summary>
        /// Registers new long running operations
        /// </summary>
        protected void RegisterTasks(params string[] ids)
        {
            foreach (var id in ids)
            {
                _operations.Add(id, new OperationToken());
            }
        }

        /// <summary>
        /// Indicates that long running operation has started
        /// </summary>
        /// <param name="id">Id of operation</param>
        /// <returns>Token of started operation</returns>
        protected OperationToken TaskStarted(string id)
        {
            if (!_operations.IsRegistered(id))
                _operations.Add(id, new OperationToken());

            _operations[id].Error = null;
            _operations[id].IsWorking = true;

            return _operations[id];
        }

        /// <summary>
        /// Indicates that long running operation has finished
        /// </summary>
        /// <param name="id">Id of operation</param>
        protected void TaskFinished(string id)
        {
            _operations[id].IsWorking = false;
        }

        /// <summary>
        /// Indicates that long running operation has failed
        /// </summary>
        /// <param name="id">Id of operation</param>
        /// <param name="error">Text that will be displayed to user</param>
        protected void TaskError(string id, string error = null)
        {
            _operations[id].Error = error;
            _operations[id].IsWorking = false;
        }

        #endregion
    }
}
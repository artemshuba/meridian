namespace Jupiter.Mvvm
{
    /// <summary>
    /// Represents long running operation (like loading data from the web).
    /// </summary>
    public class OperationToken : BindableBase
    {
        private bool _isWorking;
        private string _error;

        /// <summary>
        /// True if operation is running
        /// </summary>
        public bool IsWorking
        {
            get { return _isWorking; }
            set { Set(ref _isWorking, value); }
        }

        /// <summary>
        /// Should contain a message which will be displayed to user (e.g. "Loading failed" or "No data")
        /// </summary>
        public string Error
        {
            get { return _error; }
            set
            {
                if (Set(ref _error, value))
                    IsWorking = false;
            }
        }

        public void Finish()
        {
            IsWorking = false;
        }
    }
}
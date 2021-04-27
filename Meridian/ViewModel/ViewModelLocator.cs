using Windows.ApplicationModel;

namespace Meridian.ViewModel
{
    public class ViewModelLocator
    {
        public static MainViewModel Main { get; }

        public static PlayerViewModel Player { get; }

        static ViewModelLocator()
        {
            if (!DesignMode.DesignModeEnabled)
                Main = new MainViewModel();

            if (!DesignMode.DesignModeEnabled)
                Player = new PlayerViewModel();
        }
    }
}
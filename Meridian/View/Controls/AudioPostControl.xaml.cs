using Meridian.Interfaces;
using Meridian.Model;
using Meridian.Utils.Helpers;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Meridian.View.Controls
{
    public sealed partial class AudioPostControl : UserControl
    {
        // Using a DependencyProperty as the backing store for AudioPost.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AudioPostProperty =
            DependencyProperty.Register("AudioPost", typeof(AudioPost), typeof(AudioPostControl), new PropertyMetadata(null, (d, e) =>
            {
                var control = (AudioPostControl)d;
                var post = (AudioPost)e.NewValue;
                if (post != null)
                {
                    control.ParseText(post.Text);
                }
            }));

        public AudioPost AudioPost
        {
            get { return (AudioPost)GetValue(AudioPostProperty); }
            set { SetValue(AudioPostProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(AudioPostControl), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AuthorCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AuthorCommandProperty =
            DependencyProperty.Register("AuthorCommand", typeof(ICommand), typeof(AudioPostControl), new PropertyMetadata(null));

        /// <summary>
        /// Go to author command
        /// </summary>
        public ICommand AuthorCommand
        {
            get { return (ICommand)GetValue(AuthorCommandProperty); }
            set { SetValue(AuthorCommandProperty, value); }
        }


        // Using a DependencyProperty as the backing store for PostCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PostCommandProperty =
            DependencyProperty.Register("PostCommand", typeof(ICommand), typeof(AudioPostControl), new PropertyMetadata(null));

        /// <summary>
        /// Go to post command
        /// </summary>
        public ICommand PostCommand
        {
            get { return (ICommand)GetValue(PostCommandProperty); }
            set { SetValue(PostCommandProperty, value); }
        }

        public AudioPostControl()
        {
            this.InitializeComponent();
        }

        private void TracksListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var track = (IAudio)e.ClickedItem;
            var container = new AudioContainer() { Track = track, Tracklist = AudioPost.Tracks };

            if (Command?.CanExecute(container) == true)
                Command?.Execute(container);
        }

        private void ParseText(string text)
        {
            ContentTextBlock.Blocks.Clear();

            if (string.IsNullOrEmpty(text))
                return;

            var paragraph = TextHelper.ParseHyperlinks(text);

            TextHelper.ParseHashtags(paragraph);
            TextHelper.ParseInternalLinks(paragraph);

            ContentTextBlock.Blocks.Add(paragraph);
        }

        private void AuthorNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorCommand?.CanExecute(AudioPost) == true)
                AuthorCommand?.Execute(AudioPost);
        }

        private void PostDateButton_Click(object sender, RoutedEventArgs e)
        {
            if (PostCommand?.CanExecute(AudioPost) == true)
                PostCommand?.Execute(AudioPost);
        }
    }
}

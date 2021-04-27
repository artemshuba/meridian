using System;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Meridian.Controls
{
    public class LoopItemsPanel : Panel
    {
        private double offsetSeparator;

        public double itemHeight = 100;

        public LoopItemsPanel()
        {
            this.ManipulationDelta += OnManipulationDelta;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Clip to ensure items dont override container
            this.Clip = new RectangleGeometry { Rect = new Rect(0, 0, finalSize.Width, finalSize.Height) };

            double positionTop = 0d;

            // Must Create looping items count
            foreach (var item in this.Children)
            {
                if (item == null)
                    continue;

                Size desiredSize = item.DesiredSize;

                if (double.IsNaN(desiredSize.Width) || double.IsNaN(desiredSize.Height)) continue;

                // Get rect position
                var rect = new Rect(0, positionTop, desiredSize.Width, desiredSize.Height);
                item.Arrange(rect);

                // set internal CompositeTransform to handle movement
                TranslateTransform compositeTransform = new TranslateTransform();
                item.RenderTransform = compositeTransform;


                positionTop += desiredSize.Height;
            }

            return finalSize;
        }

        /// <summary>
        /// Updating position
        /// </summary>
        private void UpdatePositions(double offsetDelta)
        {
            double maxLogicalHeight = this.Children.Count * itemHeight;

            // Reaffect correct offsetSeparator
            this.offsetSeparator = (this.offsetSeparator + offsetDelta) % maxLogicalHeight;

            // Get the correct number item
            Int32 itemNumberSeparator = (Int32)(Math.Abs(this.offsetSeparator) / itemHeight);

            Int32 itemIndexChanging;
            Double offsetAfter;
            Double offsetBefore;

            if (this.offsetSeparator > 0)
            {
                itemIndexChanging = this.Children.Count - itemNumberSeparator - 1;
                offsetAfter = this.offsetSeparator;

                if (this.offsetSeparator % maxLogicalHeight == 0)
                    itemIndexChanging++;

                offsetBefore = offsetAfter - maxLogicalHeight;
            }
            else
            {
                itemIndexChanging = itemNumberSeparator;
                offsetBefore = this.offsetSeparator;
                offsetAfter = maxLogicalHeight + offsetBefore;
            }

            // items that must be before
            this.UpdatePosition(itemIndexChanging, this.Children.Count, offsetBefore);

            // items that must be after
            this.UpdatePosition(0, itemIndexChanging, offsetAfter);
        }

        /// <summary>
        /// Translate items to a new offset
        /// </summary>
        private void UpdatePosition(Int32 startIndex, Int32 endIndex, Double offset)
        {
            for (Int32 i = startIndex; i < endIndex; i++)
            {
                var loopListItem = this.Children[i];

                // Apply Transform
                TranslateTransform compositeTransform = (TranslateTransform)loopListItem.RenderTransform;

                if (compositeTransform == null)
                    continue;
                compositeTransform.Y = offset;

            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e == null)
                return;

            var translation = e.Delta.Translation;
            this.UpdatePositions(translation.Y / 2);
        }
    }
}

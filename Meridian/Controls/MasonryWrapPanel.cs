using Jupiter.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;

namespace Meridian.Controls
{
    //TODO virtualization
    public class MasonryWrapPanel : Panel
    {
        private Rect[] _rects;

        private bool _compositionInitialized = false;

        private ScrollViewer _scrollViewer;
        private ListViewBase _owner;

        private bool _isLoading;

        public MasonryWrapPanel()
        {
            Loaded += MasonryWrapPanel_Loaded;
            Unloaded += MasonryWrapPanel_Unloaded;
        }

        private void MasonryWrapPanel_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _scrollViewer = this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
            _owner = this.GetVisualAncestors().OfType<ListViewBase>().FirstOrDefault();

            if (_owner != null)
            {
                if (_scrollViewer != null)
                {
                   _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                }
            }
        }

        private void MasonryWrapPanel_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_scrollViewer != null)
                _scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;

            _scrollViewer = null;
            _owner = null;

            Loaded -= MasonryWrapPanel_Loaded;
            Unloaded -= MasonryWrapPanel_Unloaded;
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_isLoading)
                return;

            if (_scrollViewer == null)
                return;

            if (_scrollViewer.VerticalOffset / _scrollViewer.ScrollableHeight >= 0.85)
            {
                var incrementalLoadingCollection = _owner.ItemsSource as ISupportIncrementalLoading;
                if (incrementalLoadingCollection.HasMoreItems)
                {
                    _isLoading = true;
                    await incrementalLoadingCollection.LoadMoreItemsAsync(50);
                    _isLoading = false;
                }
            }
        }

        private void SetupComposition()
        {
            //TODO
            if (_compositionInitialized)
                return;

            var visual = ElementCompositionPreview.GetElementVisual(this);
            var compositor = visual.Compositor;

            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = nameof(Visual.Offset);
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(300);

            var implicitAnimations = compositor.CreateImplicitAnimationCollection();
            implicitAnimations[nameof(Visual.Offset)] = offsetAnimation;

            foreach (var child in Children)
            {
                var elementVisual = ElementCompositionPreview.GetElementVisual(child);
                elementVisual.ImplicitAnimations = implicitAnimations;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            SetupComposition();

            _rects = new Rect[Children.Count];

            var matrix = new List<int[]> { new[] { 0, (int)availableSize.Width, 0 } };
            var hMax = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                var control = Children[i];
                control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                var rect = _rects[i];

                rect.Width = control.DesiredSize.Width;
                rect.Height = control.DesiredSize.Height;

                var size = new[] { (int)rect.Width, (int)rect.Height };
                var point = GetAttachPoint(matrix, size[0]);
                matrix = UpdateAttachArea(matrix, point, size);
                hMax = Math.Max(hMax, point[1] + size[1]);

                if (Math.Abs(rect.X - point[0]) > 1 || Math.Abs(rect.Y - point[1]) > 1)
                {
                    rect.X = point[0];
                    rect.Y = point[1];
                }

                _rects[i] = rect;
            }

            return new Size(availableSize.Width, _rects.Length > 0 ? _rects.Max(r => r.Bottom) : 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var control = Children[i];
                var rect = _rects[i];

                control.Arrange(rect);
            }

            return base.ArrangeOverride(finalSize);
        }

        #region Calculations

        /// <summary>
        ///     Matrixes the sort depth.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private int MatrixSortDepth(int[] a, int[] b)
        {
            return (a[2] == b[2] && a[0] > b[0]) || a[2] > b[2] ? 1 : -1;
        }

        /// <summary>
        ///     Gets the attach point.
        /// </summary>
        /// <param name="mtx">The MTX.</param>
        /// <param name="width">The width.</param>
        /// <returns></returns>
        private int[] GetAttachPoint(List<int[]> mtx, int width)
        {
            mtx.Sort(this.MatrixSortDepth);
            var max = mtx[mtx.Count - 1][2];
            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtx[i][2] >= max)
                {
                    break;
                }
                if (mtx[i][1] - mtx[i][0] >= width)
                {
                    return new[] { mtx[i][0], mtx[i][2] };
                }
            }
            return new[] { 0, max };
        }

        /// <summary>
        ///     Updates the attach area.
        /// </summary>
        /// <param name="mtx">The MTX.</param>
        /// <param name="point">The point.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        private List<int[]> UpdateAttachArea(List<int[]> mtx, int[] point, int[] size)
        {
            mtx.Sort(this.MatrixSortDepth);
            int[] cell = { point[0], point[0] + size[0], point[1] + size[1] };
            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtx.Count - 1 >= i)
                {
                    if (cell[0] <= mtx[i][0] && mtx[i][1] <= cell[1])
                    {
                        mtx.RemoveAt(i);
                    }
                    else
                    {
                        mtx[i] = this.MatrixTrimWidth(mtx[i], cell);
                    }
                }
            }
            return this.MatrixJoin(mtx, cell);
        }

        /// <summary>
        ///     Matrixes the width of the trim.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private int[] MatrixTrimWidth(int[] a, int[] b)
        {
            if (a[0] >= b[0] && a[0] < b[1] || a[1] >= b[0] && a[1] < b[1])
            {
                if (a[0] >= b[0] && a[0] < b[1])
                {
                    a[0] = b[1];
                }
                else
                {
                    a[1] = b[0];
                }
            }
            return a;
        }

        /// <summary>
        ///     Matrixes the sort x.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private int MatrixSortX(int[] a, int[] b)
        {
            return a[0] > b[0] ? 1 : -1;
        }

        /// <summary>
        ///     Matrixes the join.
        /// </summary>
        /// <param name="mtx">The MTX.</param>
        /// <param name="cell">The cell.</param>
        /// <returns></returns>
        private List<int[]> MatrixJoin(List<int[]> mtx, int[] cell)
        {
            mtx.Add(cell);
            mtx.Sort(this.MatrixSortX);
            var mtxJoin = new List<int[]>();
            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtxJoin.Count > 0 && mtxJoin[mtxJoin.Count - 1][1] == mtx[i][0]
                    && mtxJoin[mtxJoin.Count - 1][2] == mtx[i][2])
                {
                    mtxJoin[mtxJoin.Count - 1][1] = mtx[i][1];
                }
                else
                {
                    mtxJoin.Add(mtx[i]);
                }
            }
            return mtxJoin;
        }

        #endregion
    }
}

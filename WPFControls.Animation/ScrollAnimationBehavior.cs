using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace System.Windows.Controls.Animation
{
    public static class ScrollAnimationBehavior
    {
        private class ScrollAnimationInternal
        {
            internal Storyboard PreviewHorizontalStoryboard;
            internal Storyboard PreviewVerticalStoryboard;
            internal TimeSpan PreviewHorizontalAdditionTime;
            internal TimeSpan PreviewVerticalAdditionTime;
            internal int PreviewVerticalAddition;
            internal int PreviewHorizontalAddition;

            public double TargetHorizontalOffset { get; set; }
            public double TargetVerticalOffset { get; set; }
        }

        private static readonly IEasingFunction DefaultEaseOut = new CircleEase { EasingMode = EasingMode.EaseOut };

        #region Private ScrollViewer for ItemsControl

        private static ScrollViewer _itemsControlScrollViewer = new ScrollViewer();

        #endregion

        #region DefaultOrientation Property

        public static readonly DependencyProperty DefaultOrientationProperty =
            DependencyProperty.RegisterAttached("DefaultOrientation",
                typeof(Orientation),
                typeof(ScrollAnimationBehavior),
                new UIPropertyMetadata(Orientation.Vertical));

        public static void SetDefaultOrientation(FrameworkElement target, Orientation value)
        {
            target.SetValue(DefaultOrientationProperty, value);
        }

        public static Orientation GetDefaultOrientation(FrameworkElement target)
        {
            return (Orientation)target.GetValue(DefaultOrientationProperty);
        }

        #endregion

        #region HorizontalOffset Property

        private static readonly DependencyProperty ScrollAnimationInternalProperty =
            DependencyProperty.RegisterAttached("ScrollAnimationInternal",
                typeof(ScrollAnimationInternal),
                typeof(ScrollAnimationBehavior),
                new UIPropertyMetadata(default));

        private static void SetScrollAnimationInternal(FrameworkElement target, Orientation value)
        {
            target.SetValue(ScrollAnimationInternalProperty, value);
        }

        private static ScrollAnimationInternal GetScrollAnimationInternal(FrameworkElement target)
        {
            var scrollAnimationInternal = (ScrollAnimationInternal)target.GetValue(ScrollAnimationInternalProperty);
            if (scrollAnimationInternal == null)
            {
                target.SetValue(ScrollAnimationInternalProperty, new ScrollAnimationInternal());
            }

            return (ScrollAnimationInternal)target.GetValue(ScrollAnimationInternalProperty);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.RegisterAttached("HorizontalOffset",
                typeof(double),
                typeof(ScrollAnimationBehavior),
                new UIPropertyMetadata(0.0, OnHorizontalOffsetChanged));

        public static void SetHorizontalOffset(FrameworkElement target, double value)
        {
            target.SetValue(HorizontalOffsetProperty, value);
            var @internal = GetScrollAnimationInternal(target);
            @internal.TargetHorizontalOffset = value;
        }

        public static double GetHorizontalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(HorizontalOffsetProperty);
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset",
                typeof(double),
                typeof(ScrollAnimationBehavior),
                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
            var @internal = GetScrollAnimationInternal(target);
            @internal.TargetVerticalOffset = value;
        }

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        #endregion

        #region TimeDuration Property

        public static readonly DependencyProperty TimeDurationProperty =
            DependencyProperty.RegisterAttached("TimeDuration",
                typeof(TimeSpan),
                typeof(ScrollAnimationBehavior),
                new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 0)));

        public static void SetTimeDuration(FrameworkElement target, TimeSpan value)
        {
            target.SetValue(TimeDurationProperty, value);
        }

        public static TimeSpan GetTimeDuration(FrameworkElement target)
        {
            return (TimeSpan)target.GetValue(TimeDurationProperty);
        }

        #endregion

        #region PointsToScroll Property

        public static readonly DependencyProperty PointsToScrollProperty =
            DependencyProperty.RegisterAttached("PointsToScroll",
                typeof(double),
                typeof(ScrollAnimationBehavior),
                new PropertyMetadata(0.0));

        public static void SetPointsToScroll(FrameworkElement target, double value)
        {
            target.SetValue(PointsToScrollProperty, value);
        }

        public static double GetPointsToScroll(FrameworkElement target)
        {
            return (double)target.GetValue(PointsToScrollProperty);
        }

        #endregion

        #region OnHorizontalOffset Changed

        private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
            }
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        #endregion

        #region IsEnabled Property

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled",
                typeof(bool),
                typeof(ScrollAnimationBehavior),
                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(FrameworkElement target, bool value)
        {
            target.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(FrameworkElement target)
        {
            return (bool)target.GetValue(IsEnabledProperty);
        }

        #endregion

        #region OnIsEnabledChanged Changed

        private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = sender;

            if (target is ScrollViewer scrollViewer)
            {
                scrollViewer.Loaded += ScrollViewer_Loaded;
            }
            else if (target is ItemsControl ic)
            {
                ic.Loaded += ItemsControl_Loaded;
            }
        }

        #endregion

        #region AnimateScroll Helper

        private static void AnimateScroll(ScrollViewer scrollViewer, double toValue, Orientation orientation)
        {
            var @internal = GetScrollAnimationInternal(scrollViewer);

            var timeDuration = GetTimeDuration(scrollViewer);
            if (orientation == Orientation.Horizontal)
            {
                var previous = @internal.PreviewHorizontalStoryboard;
                if (previous != null && !previous.GetIsPaused())
                {
                    previous.Pause();
                    @internal.PreviewHorizontalAdditionTime += (timeDuration - previous.GetCurrentTime());
                    @internal.PreviewHorizontalAddition += 1;
                }

                var horizontalAnimation = new DoubleAnimation
                {
                    //From = @internal.TargetHorizontalOffset,
                    EasingFunction = DefaultEaseOut,
                    To = toValue,
                    Duration = new Duration(timeDuration +
                                            (@internal.PreviewHorizontalAddition == 0
                                                ? TimeSpan.Zero
                                                : @internal.PreviewHorizontalAdditionTime /
                                                  @internal.PreviewHorizontalAddition))
                };

                var storyboard = new Storyboard();

                storyboard.Children.Add(horizontalAnimation);
                Storyboard.SetTarget(horizontalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(horizontalAnimation, new PropertyPath(HorizontalOffsetProperty));
                storyboard.Completed += (s, e) =>
                {
                    storyboard.Stop();
                    SetHorizontalOffset(scrollViewer, toValue);
                    @internal.PreviewHorizontalAdditionTime = TimeSpan.Zero;
                    @internal.PreviewVerticalAddition = 0;
                };
                @internal.PreviewHorizontalStoryboard = storyboard;
                storyboard.Begin();
                @internal.TargetHorizontalOffset = toValue;
            }
            else
            {
                var previous = @internal.PreviewVerticalStoryboard;
                if (previous != null && !previous.GetIsPaused())
                {
                    previous.Pause();
                    @internal.PreviewVerticalAdditionTime += (timeDuration - previous.GetCurrentTime());
                    @internal.PreviewVerticalAddition += 1;
                }

                var verticalAnimation = new DoubleAnimation
                {
                    //From = @internal.TargetVerticalOffset,
                    EasingFunction = DefaultEaseOut,
                    To = toValue,
                    Duration = new Duration(timeDuration +
                                            (@internal.PreviewVerticalAddition == 0
                                                ? TimeSpan.Zero
                                                : @internal.PreviewVerticalAdditionTime /
                                                  @internal.PreviewVerticalAddition))
                };

                var storyboard = new Storyboard();

                storyboard.Children.Add(verticalAnimation);
                Storyboard.SetTarget(verticalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(VerticalOffsetProperty));
                storyboard.Completed += (s, e) =>
                {
                    storyboard.Stop();
                    SetVerticalOffset(scrollViewer, toValue);
                    @internal.PreviewVerticalAdditionTime = TimeSpan.Zero;
                    @internal.PreviewVerticalAddition = 0;
                };
                @internal.PreviewVerticalStoryboard = storyboard;
                storyboard.Begin();
                @internal.TargetVerticalOffset = toValue;
            }

        }

        #endregion

        #region NormalizeScrollPos Helper

        private static double NormalizeScrollPos(ScrollViewer scroll, double scrollChange, Orientation o)
        {
            double returnValue = scrollChange;

            if (scrollChange < 0)
            {
                returnValue = 0;
            }

            if (o == Orientation.Vertical && scrollChange > scroll.ScrollableHeight)
            {
                returnValue = scroll.ScrollableHeight;
            }
            else if (o == Orientation.Horizontal && scrollChange > scroll.ScrollableWidth)
            {
                returnValue = scroll.ScrollableWidth;
            }

            return returnValue;
        }

        #endregion

        #region UpdateScrollPosition Helper

        private static void UpdateScrollPosition(object sender)
        {
            if (!(sender is Selector listbox)) return;

            double scrollTo = 0;

            for (int i = 0; i < (listbox.SelectedIndex); i++)
            {
                if (listbox.ItemContainerGenerator.ContainerFromItem(listbox.Items[i]) is ListBoxItem tempItem)
                {
                    scrollTo += tempItem.ActualHeight;
                }
            }

            AnimateScroll(_itemsControlScrollViewer, scrollTo, Orientation.Vertical);
        }

        #endregion

        #region SetEventHandlersForScrollViewer Helper

        private static void SetEventHandlersForScrollViewer(ScrollViewer scrollViewer)
        {
            var scrollBar = FindVisualChildHelper.GetFirstChildOfType<ScrollBar>(scrollViewer);
            scrollBar.Scroll += ScrollBar_Scroll;
            scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            scrollViewer.PreviewKeyDown += ScrollViewer_PreviewKeyDown;
        }

        private static void ScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region ScrollViewer_Loaded Event Handler

        private static void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scroller = sender as ScrollViewer;

            SetEventHandlersForScrollViewer(scroller);
        }

        #endregion

        #region ItemsControl_Loaded Event Handler

        private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = (ItemsControl)sender;

            _itemsControlScrollViewer = FindVisualChildHelper.GetFirstChildOfType<ScrollViewer>(itemsControl);
            SetEventHandlersForScrollViewer(_itemsControlScrollViewer);

            SetTimeDuration(_itemsControlScrollViewer, new TimeSpan(0, 0, 0, 0, 200));
            //SetPointsToScroll(_itemsControlScrollViewer, 16.0);
            var value = GetPointsToScroll(itemsControl);
            SetPointsToScroll(_itemsControlScrollViewer, value);
            //if (itemsControl is Selector selector)
            //    selector.SelectionChanged += Selector_SelectionChanged;

            UpdateScrollPosition(sender);

            itemsControl.LayoutUpdated += ItemsControl_LayoutUpdated;
        }

        #endregion

        #region ScrollViewer_PreviewMouseWheel Event Handler

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double mouseWheelChange = (double)e.Delta;
            ScrollViewer scroller = (ScrollViewer)sender;
            var orientation = GetDefaultOrientation(scroller);
            var @internal = GetScrollAnimationInternal(scroller);
            if (scroller.CanContentScroll)
            {
                var pointsToScroll = GetPointsToScroll(scroller);
                mouseWheelChange = mouseWheelChange / Math.Abs(mouseWheelChange) * pointsToScroll;
            }

            double newOffset = orientation == Orientation.Horizontal
                ? @internal.TargetHorizontalOffset - (mouseWheelChange/* / 3*/)
                : @internal.TargetVerticalOffset - (mouseWheelChange/* / 3*/);

            if (newOffset < 0)
            {
                AnimateScroll(scroller, 0, orientation);
            }
            else if (orientation == Orientation.Horizontal && newOffset > scroller.ScrollableWidth)
            {
                AnimateScroll(scroller, scroller.ScrollableWidth, orientation);
            }
            else if (orientation == Orientation.Vertical && newOffset > scroller.ScrollableHeight)
            {
                AnimateScroll(scroller, scroller.ScrollableHeight, orientation);
            }
            else
            {
                AnimateScroll(scroller, newOffset, orientation);
            }

            e.Handled = true;
        }

        #endregion

        #region ScrollViewer_PreviewKeyDown Handler

        private static void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ScrollViewer scroller = (ScrollViewer)sender;

            Key keyPressed = e.Key;
            var @internal = GetScrollAnimationInternal(scroller);
            double newHorizontalPos = @internal.TargetHorizontalOffset;
            double newVerticalPos = @internal.TargetVerticalOffset;
            bool isKeyHandled = false;

            if (keyPressed == Key.Down)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + GetPointsToScroll(scroller)), Orientation.Vertical);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.PageDown)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + scroller.ViewportHeight), Orientation.Vertical);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.Up)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - GetPointsToScroll(scroller)), Orientation.Vertical);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.PageUp)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - scroller.ViewportHeight), Orientation.Vertical);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.Left)
            {
                newHorizontalPos = NormalizeScrollPos(scroller, (newHorizontalPos + GetPointsToScroll(scroller)), Orientation.Horizontal);
                isKeyHandled = true;
            }
            else if (keyPressed == Key.Right)
            {
                newHorizontalPos = NormalizeScrollPos(scroller, (newHorizontalPos - GetPointsToScroll(scroller)), Orientation.Horizontal);
                isKeyHandled = true;
            }

            if (newHorizontalPos != @internal.TargetHorizontalOffset)
            {
                AnimateScroll(scroller, newHorizontalPos, Orientation.Horizontal);
            }
            else if (newVerticalPos != @internal.TargetVerticalOffset)
            {
                AnimateScroll(scroller, newVerticalPos, Orientation.Vertical);
            }

            e.Handled = isKeyHandled;
        }

        #endregion

        #region ListBox Event Handlers

        private static void ItemsControl_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateScrollPosition(sender);
        }

        private static void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateScrollPosition(sender);
        }

        #endregion
    }
}
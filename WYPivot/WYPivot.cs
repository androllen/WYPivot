/********************************************************************************
** 作者： Androllen
** 日期： 16/12/18 16:46:17
** 微博： http://weibo.com/Androllen
*********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml.Data;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using System.Collections.ObjectModel;

namespace CCUWPToolkit.Controls
{
    [TemplatePart(Name = ScrollViewerStateName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ListViewStateName, Type = typeof(ListView))]
    public class WYPivot : Pivot
    {
        private const string ScrollViewerStateName = "ScrollViewer";
        private const string ListViewStateName = "PART_ListViewStateName";
        private ScrollViewer _scrollViewerStateName;
        private ListView _listViewStateName;
        private Rectangle rect_old; // 上一次选中的 Rectangle
        private Rectangle rect_current;// 当前选中的 Rectangle

        private readonly TaskCompletionSource<bool> _waitForApplyTemplateTaskSource = new TaskCompletionSource<bool>(false);

        public WYPivot()
        {
            DefaultStyleKey = typeof(WYPivot);
        }
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewerStateName = (ScrollViewer)GetTemplateChild(ScrollViewerStateName);
            _listViewStateName = (ListView)GetTemplateChild(ListViewStateName);
            if (_scrollViewerStateName != null)
            {
                _scrollViewerStateName.ViewChanged += _scrollViewerStateName_ViewChanged;
                _scrollViewerStateName.DirectManipulationStarted += _scrollViewerStateName_DirectManipulationStarted;
            }
            if (_listViewStateName != null)
            {
                _listViewStateName.SelectionChanged += _listViewStateName_SelectionChanged;
                _listViewStateName.SelectedIndex = 0;
                _listViewStateName.Loaded += _listViewStateName_Loaded;
            }

            var selectedItemBinding = new Binding()
            {
                Source = _listViewStateName,
                Path = new PropertyPath("SelectedItem"),
                Mode = BindingMode.TwoWay
            };
            SetBinding(SelectedItemProperty, selectedItemBinding);

            _waitForApplyTemplateTaskSource.SetResult(true);
        }

        private void _listViewStateName_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewItem item = _listViewStateName.ContainerFromIndex(0) as ListViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            var rect = grid.FindName("PART_RectangleStateName") as Rectangle;
            rect.Loaded += Rect_Loaded;
        }

        private void Rect_Loaded(object sender, RoutedEventArgs e)
        {
            var r = sender as Rectangle;
            r.Loaded -= Rect_Loaded;

            if (_listViewStateName.ContainerFromIndex(0) != null)
            {
                var grid = (_listViewStateName.ContainerFromIndex(0) as ListViewItem).ContentTemplateRoot as Grid;
                rect_current = grid.FindName("PART_RectangleStateName") as Rectangle;
                rect_current.Opacity = 1;
            }
        }

        private async void _listViewStateName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await _waitForApplyTemplateTaskSource.Task;

            ListViewItem item = _listViewStateName.ContainerFromIndex(_listViewStateName.SelectedIndex) as ListViewItem;
            if (item != null)
            {
                var grid = item.ContentTemplateRoot as Grid;
                var rect = grid.FindName("PART_RectangleStateName") as Rectangle;
                (rect.RenderTransform as CompositeTransform).TranslateX = 0;// 重置横向位移为 0
                (rect.RenderTransform as CompositeTransform).ScaleX = 1.0;// 重置横向位移为 0
                _listViewStateName.ScrollIntoView(_listViewStateName.SelectedItem);

                rect_old = rect_current;
                rect_current = rect;

                StoryBordImg(grid);
                _listViewStateName.IsHitTestVisible = false; // 当“划动动画”在播放的时候，不再接受单击事件
            }
        }

        private void StoryBordImg(Grid grid)
        {
            if (rect_old != null && rect_current != null)
            {
                var old_rect = GetBounds(rect_old, _listViewStateName);
                var new_rect = GetBounds(rect_current, _listViewStateName);

                var sb = new Storyboard();
                var anim = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(anim, rect_old);
                Storyboard.SetTargetProperty(anim, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");

                EasingDoubleKeyFrame KeyFrame = new EasingDoubleKeyFrame();
                KeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
                KeyFrame.Value = 0;

                EasingDoubleKeyFrame KeyFrame2 = new EasingDoubleKeyFrame();
                KeyFrame2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(350));
                KeyFrame2.Value = 600;

                QuarticEase easing = new QuarticEase();
                easing.EasingMode = EasingMode.EaseOut;
                easing.Ease(0.3);
                KeyFrame2.EasingFunction = easing;

                anim.KeyFrames.Add(KeyFrame);
                anim.KeyFrames.Add(KeyFrame2);
                anim.KeyFrames[1].Value = new_rect.X - old_rect.X;

                var anim2 = new DoubleAnimation();
                anim2.To = rect_current.ActualWidth / old_rect.Width;
                System.Diagnostics.Debug.WriteLine("x :" + rect_current.ActualWidth / rect_old.ActualWidth);
                anim2.Duration = TimeSpan.FromMilliseconds(100);
                Storyboard.SetTarget(anim2, rect_old);
                Storyboard.SetTargetProperty(anim2, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");

                sb.Children.Add(anim);
                sb.Children.Add(anim2);
                sb.Begin();

                sb.Completed += (s, e) =>
                {
                    _listViewStateName.IsHitTestVisible = true;
                    rect_current.Opacity = 1;
                    rect_old.Opacity = 0;
                    sb.Stop();
                };
            }
        }

        // 获取子元素相对于父元素的左边
        public Rect GetBounds(FrameworkElement childElement, FrameworkElement parentElement)
        {
            GeneralTransform transform = childElement.TransformToVisual(parentElement);
            return transform.TransformBounds(new Rect(0, 0, childElement.ActualWidth, childElement.ActualHeight));
        }

        private void _scrollViewerStateName_DirectManipulationStarted(object sender, object e)
        {
        }

        private void _scrollViewerStateName_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
        }
    }
}


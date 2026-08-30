using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace LuaSTGEditorSharpV2.WPF.Behavior
{
	public class AutoCollapseBehavior : Behavior<Control>
	{
		public Orientation Orientation { get; set; } = Orientation.Horizontal;
		public double Threshold { get; set; } = 0.5;

		private Visibility _originalVisibility;
		private FrameworkElement? _parent;

		protected override void OnAttached()
		{
			base.OnAttached();
			_originalVisibility = AssociatedObject.Visibility;
			var parent = VisualTreeHelper.GetParent(AssociatedObject) as FrameworkElement;
			if (parent is not null)
			{
				_parent = parent;
				parent.SizeChanged += OnParentSizeChanged;
			}
		}

		private void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (AssociatedObject.Parent is not FrameworkElement fe) return;
			double ratio = Orientation == Orientation.Horizontal
				? fe.ActualWidth / AssociatedObject.ActualWidth
				: fe.ActualHeight / AssociatedObject.ActualHeight;
			OnSizeRatioChanged(ratio);
		}

		private void OnSizeRatioChanged(double ratio)
		{
			if (ratio < Threshold)
			{
				AssociatedObject.Visibility = System.Windows.Visibility.Collapsed;
			}
			else
			{
				AssociatedObject.Visibility = System.Windows.Visibility.Visible;
			}
		}

		protected override void OnDetaching()
		{
			if (_parent is not null) _parent.SizeChanged -= OnParentSizeChanged;
			if (AssociatedObject is not null) AssociatedObject.Visibility = System.Windows.Visibility.Visible;
			base.OnDetaching();
		}
	}
}

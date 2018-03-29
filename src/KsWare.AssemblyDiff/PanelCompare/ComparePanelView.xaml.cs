using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace KsWare.AssemblyDiff.PanelCompare {
	/// <summary>
	/// Interaction logic for ComparePanelView.xaml
	/// </summary>
	public partial class ComparePanelView : UserControl {
		private ScrollViewer _scrollViewerA;
		private ScrollViewer _scrollViewerB;
		private ScrollViewer _scrollViewerC;

		public ComparePanelView() {
			InitializeComponent();
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
				_scrollViewerA = GetScrollViewer(TreeViewA);
				_scrollViewerB = GetScrollViewer(TreeViewB);
				_scrollViewerC = GetScrollViewer(TreeViewC);

				_scrollViewerA.ScrollChanged += ScrollChanged;
				_scrollViewerB.ScrollChanged += ScrollChanged;
				_scrollViewerC.ScrollChanged += ScrollChanged;
			}));
		}

		private ScrollViewer GetScrollViewer(TreeView treeView) {
			var ch=VisualTreeHelper.GetChild(treeView, 0);
			ch = VisualTreeHelper.GetChild(ch, 0);
			return (ScrollViewer) ch;
		}

		private void ScrollChanged(object sender, ScrollChangedEventArgs e) {
			if (e.VerticalOffset > 0) {
				if (sender == _scrollViewerA) {
					_scrollViewerB.ScrollToVerticalOffset(e.VerticalOffset);
					_scrollViewerC.ScrollToVerticalOffset(e.VerticalOffset);
				} else if(sender == _scrollViewerB) {
					_scrollViewerA.ScrollToVerticalOffset(e.VerticalOffset);
					_scrollViewerC.ScrollToVerticalOffset(e.VerticalOffset);
				}else if (sender == _scrollViewerC) {
					_scrollViewerA.ScrollToVerticalOffset(e.VerticalOffset);
					_scrollViewerB.ScrollToVerticalOffset(e.VerticalOffset);
				}
			}

		}
	}
}

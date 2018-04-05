using System;
using System.Windows;
using JetBrains.Annotations;
using KsWare.AssemblyDiff.PanelCompare;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.AssemblyDiff {

	public class MainWindowVM : WindowVM {

		public MainWindowVM() {
			RegisterChildren(() => this);

			MenuItems.Add(new MenuItemVM{Caption = "_File",Items = {
				new MenuItemVM{Caption = "_Exit", CommandAction = { MːDoAction = DoExit}}
			}});
		}

		private void DoExit() {
			Environment.Exit(0);
		}

		public ListVM<MenuItemVM> MenuItems { get; [UsedImplicitly] private set; }

		public ComparePanelVM ComparePanel { get; [UsedImplicitly] private set; }
	}

}

using KsWare.Presentation.ViewModelFramework;

namespace KsWare.AssemblyDiff {

	public class AppVM : ApplicationVM {

		public AppVM() {
			RegisterChildren(() => this);
			StartupUri = typeof(MainWindowVM);
		}
	}

}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using KsWare.DependencyWalker.AppDomainWorkers;
using KsWare.Presentation;

namespace KsWare.AssemblyDiff {

	public class SelectorVM : KsWare.Presentation.ViewModelFramework.ObjectVM {

		public SelectorVM() {
			RegisterChildren(() => this);

			Fields[nameof(SelectedDirectory)].ValueChangedEvent.add=AtSelectedDirectoryChanged;
			Fields[nameof(SelectedAssemblyFile)].ValueChangedEvent.add = AtSelectedAssemblyFileChanged;
			Fields[nameof(SelectedTypeFullName)].ValueChangedEvent.add = AtSelectedTypeFullNameChanged;
		}

		public AssemblyWalker AssemblyWalker { get; private set;}

		public string SelectedDirectory { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		public string SelectedAssemblyFile { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		public MyAssemblyInfo SelectedAssembly { get => Fields.GetValue<MyAssemblyInfo>(); set => Fields.SetValue(value); }

		public List<string> AssemblyFiles { get => Fields.GetValue<List<string>>(); set => Fields.SetValue(value); }

		public string SelectedTypeFullName { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		public MyTypeInfo SelectedType { get => Fields.GetValue<MyTypeInfo>(); set => Fields.SetValue(value); }

		public List<string> TypeFullNames { get => Fields.GetValue<List<string>>(); set => Fields.SetValue(value); }

		private void AtSelectedDirectoryChanged(object sender, ValueChangedEventArgs e) {
			if (!Directory.Exists(SelectedDirectory)) {
				SelectedAssemblyFile = null;
			}
			else {
				AssemblyFiles = Directory.GetFiles(SelectedDirectory).Where(n => Path.GetExtension(n)?.ToLower() == ".dll" || Path.GetExtension(n)?.ToLower() == ".exe").ToList();
				SelectedAssemblyFile = null;
				AssemblyWalker = AssemblyWalker.GetInstance(SelectedDirectory);
			}
		}
		private void AtSelectedAssemblyFileChanged(object sender, ValueChangedEventArgs e) {
			if (!File.Exists(SelectedAssemblyFile)) {
				SelectedTypeFullName = null;
			}
			else {
				SelectedAssembly = AssemblyWalker.LoadAssembly(SelectedAssemblyFile);
				AssemblyWalker.LoadDependencies(SelectedAssembly);
				AssemblyWalker.UpdateExportedTypes(SelectedAssembly, true);
				TypeFullNames = SelectedAssembly.Types.Select(t => t.FullName).ToList();
				SelectedTypeFullName = null;
			}
		}
		private void AtSelectedTypeFullNameChanged(object sender, ValueChangedEventArgs e) {
			if (SelectedTypeFullName==null) {
				SelectedType = null;
			}
			else {
				SelectedType = SelectedAssembly.Types.FirstOrDefault(t => t.FullName == SelectedTypeFullName);
			}
		}
	}

}

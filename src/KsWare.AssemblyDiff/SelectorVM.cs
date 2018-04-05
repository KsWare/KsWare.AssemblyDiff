using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;
using KsWare.DependencyWalker.AppDomainWorkers;
using KsWare.Presentation;
using KsWare.Presentation.ViewModelFramework;
using Microsoft.Win32;

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

		/// <summary>
		/// Gets the <see cref="ActionVM"/> to SelectDirectory
		/// </summary>
		/// <seealso cref="DoSelectDirectory"/>
		public ActionVM SelectDirectoryAction { get; [UsedImplicitly] private set; }

		/// <summary>
		/// Method for <see cref="SelectDirectoryAction"/>
		/// </summary>
		[UsedImplicitly]
		private void DoSelectDirectory() {
			var dlg = new OpenFileDialog {
				Title = "Select directory or nuget package...",
				Filter="All|*.*|NuGet Packages|*.nupkg|Libraries|*.dll",
				FilterIndex = 1
			};
			if(dlg.ShowDialog()!=true) return;
			var ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
			switch (ext) {
				case ".nupkg": {
					var temp = SelectedDirectory = Extract(dlg.FileName);
					var dlg2 = new OpenFileDialog {
						Title            = "Select directory or nuget package...",
						Filter           = "All|*.*|NuGet Packages|*.nupkg|Libraries|*.dll",
						FilterIndex      = 1,
						InitialDirectory = temp
					};
					if (dlg2.ShowDialog() != true) {
						SelectedDirectory = temp;
					}
					else {
						SelectedDirectory = Path.GetDirectoryName(dlg2.FileName);
					}
					break;
				}
				default: {
					SelectedDirectory = Path.GetDirectoryName(dlg.FileName);
					break;
				}
			}
		}

		private string Extract(string nupkg) {
			if (nupkg == null) throw new ArgumentNullException(nameof(nupkg));
			var temp = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(nupkg));
			Directory.CreateDirectory(temp);

			using (var archive = ZipFile.OpenRead(nupkg)) {
				foreach (ZipArchiveEntry entry in archive.Entries) {
					var fullName = Path.Combine(temp, entry.FullName);
					Directory.CreateDirectory(Path.GetDirectoryName(fullName));
					entry.ExtractToFile(fullName,true);
				}
			}

			return temp;
		}

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

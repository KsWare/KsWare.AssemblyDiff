using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using KsWare.DependencyWalker.AppDomainWorkers;
using KsWare.Presentation.ViewModelFramework;

namespace KsWare.AssemblyDiff.PanelCompare {

	public class ComparePanelVM : ObjectVM {

		public ComparePanelVM() {
			RegisterChildren(() => this);

			SelectorA.SelectedDirectory =
				@"D:\Develop\Extern\GitHub.KsWare\KsWare.Presentation\master\src\KsWare.Presentation\bin\Debug";
			SelectorB.SelectedDirectory = @"C:\Users\KayS\Downloads\KsWare.Presentation.0.18.11\lib\net45";

			IsSelectorExpanded = true;
		}

		public SelectorVM SelectorA { get; [UsedImplicitly] private set; }

		public SelectorVM SelectorB { get; [UsedImplicitly] private set; }

		/// <summary>
		/// Gets the <see cref="ActionVM"/> to Compare
		/// </summary>
		/// <seealso cref="DoCompare"/>
		public ActionVM CompareAction { get; [UsedImplicitly] private set; }

		public List<ICompareResult> Items { get => Fields.GetValue<List<ICompareResult>>(); set => Fields.SetValue(value); }
		public bool IsSelectorExpanded { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }

		/// <summary>
		/// Method for <see cref="CompareAction"/>
		/// </summary>
		[UsedImplicitly]
		private void DoCompare() {
			if (SelectorA.SelectedType != null) {
				Items = CompareType(SelectorA.SelectedType, SelectorB.SelectedType).Cast<ICompareResult>().ToList();
			}
			else if (SelectorA.SelectedAssembly != null) {
				Items = CompareAssembly(SelectorA.SelectedAssembly, SelectorB.SelectedAssembly).Cast<ICompareResult>().ToList();
			}
			else if (SelectorA.SelectedDirectory != null) {
				Items = CompareFolder(SelectorA.SelectedDirectory, SelectorB.SelectedDirectory).Cast<ICompareResult>().ToList();
			}
		}

		private List<AssemblyCompareResult> CompareFolder(string directoryA, string directoryB) {
			if (!directoryA.EndsWith("\\")) directoryA += "\\";
			if (!directoryB.EndsWith("\\")) directoryB += "\\";
			var filesA = Directory.GetFiles(directoryA)
				.Where(n => Path.GetExtension(n)?.ToLower() == ".dll" || Path.GetExtension(n)?.ToLower() == ".exe")
				.Select(f => f.Substring(directoryA.Length)).ToArray();
			var filesB = Directory.GetFiles(directoryB)
				.Where(n => Path.GetExtension(n)?.ToLower() == ".dll" || Path.GetExtension(n)?.ToLower() == ".exe")
				.Select(f => f.Substring(directoryB.Length)).ToArray();

			var all    = filesA.Concat(filesB).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
			var result = new List<AssemblyCompareResult>();
			foreach (var f in all) {
				var ra = filesA.Contains(f, StringComparer.OrdinalIgnoreCase);
				var rb = filesB.Contains(f, StringComparer.OrdinalIgnoreCase);
				if (ra  && rb) result.Add(new AssemblyCompareResult(f,  Result.None));
				if (ra  && !rb) result.Add(new AssemblyCompareResult(f, Result.OnlyLeft){HistoryResult = HistoryResult.Added});
				if (!ra && rb) result.Add(new AssemblyCompareResult(f,  Result.OnlyRight){HistoryResult = HistoryResult.Removed});
			}

			foreach (var c in result.Where(r => r.Result == Result.None)) {
				c.AssemblyA = SelectorA.AssemblyWalker.LoadAssembly(directoryA + c.Name);
				SelectorA.AssemblyWalker.LoadDependencies(c.AssemblyA);
				SelectorA.AssemblyWalker.UpdateExportedTypes(c.AssemblyA, true);

				c.AssemblyB = SelectorB.AssemblyWalker.LoadAssembly(directoryB + c.Name);
				SelectorB.AssemblyWalker.LoadDependencies(c.AssemblyB);
				SelectorB.AssemblyWalker.UpdateExportedTypes(c.AssemblyB, true);

				c.SubResults = CompareAssembly(c.AssemblyA, c.AssemblyB);
				c.Result = c.SubResults.All(r => r.Result == Result.Equal) ? Result.Equal : Result.Different;

				if (     c.SubResults.Any(r => r.HistoryResult == HistoryResult.Removed)) c.HistoryResult = HistoryResult.Removed;
				else if (c.SubResults.Any(r => r.HistoryResult == HistoryResult.Changed)) c.HistoryResult = HistoryResult.Changed;
				else if (c.SubResults.Any(r => r.HistoryResult == HistoryResult.Added  )) c.HistoryResult   = HistoryResult.Added;
				else if (c.SubResults.Any(r => r.HistoryResult == HistoryResult.Equal  )) c.HistoryResult   = HistoryResult.Equal;
				else c.HistoryResult = HistoryResult.Equal;
			}

			return result;
		}

		private TypeCompareResult[] CompareAssembly(MyAssemblyInfo assemblyA, MyAssemblyInfo assemblyB) {
			var all = assemblyA.Types.Select(t => t.DisplayName).Concat(assemblyB.Types.Select(t => t.DisplayName)).Distinct();

			var result = new List<TypeCompareResult>();
			foreach (var f in all) {
				var ra = assemblyA.Types.Any(t => t.DisplayName == f);
				var rb = assemblyB.Types.Any(t => t.DisplayName == f);
				if (ra  && rb) result.Add(new TypeCompareResult(f,  Result.None));
				if (ra  && !rb) result.Add(new TypeCompareResult(f, Result.OnlyLeft){HistoryResult = HistoryResult.Added});
				if (!ra && rb) result.Add(new TypeCompareResult(f,  Result.OnlyRight){HistoryResult = HistoryResult.Removed});
			}

			foreach (var c in result.Where(r => r.Result == Result.None)) {
				c.TypeA = assemblyA.Types.First(t => t.DisplayName == c.Name);
				c.TypeB = assemblyB.Types.First(t => t.DisplayName == c.Name);
				c.SubResults = CompareType(c.TypeA, c.TypeB).ToArray();
				c.Result = c.SubResults.All(r => r.Result == Result.Equal) ? Result.Equal : Result.Different;

				if      (c.SubResults.Any(r => r.HistoryResult == HistoryResult.Removed)) c.HistoryResult = HistoryResult.Removed;
				else if (c.SubResults.Any(r => r.HistoryResult == HistoryResult.Changed)) c.HistoryResult = HistoryResult.Changed;
				else if (c.SubResults.Any(r => r.HistoryResult == HistoryResult.Added  )) c.HistoryResult = HistoryResult.Added;
				else if (c.SubResults.Any(r => r.HistoryResult == HistoryResult.Equal  )) c.HistoryResult = HistoryResult.Equal;
				else c.HistoryResult = HistoryResult.Equal;
			}

			return result.ToArray();
		}

		private List<MemberCompareResult> CompareType(MyTypeInfo typeA, MyTypeInfo typeB) {
			var all=typeA.Members.Select(m => m.Signature)
				.Concat(typeB.Members.Select(m => m.Signature))
				.Distinct();

			var result = new List<MemberCompareResult>();
			foreach (var s in all) {
				var ma = typeA.Members.FirstOrDefault(m => m.Signature == s);
				var mb = typeB.Members.FirstOrDefault(m => m.Signature == s);

				var historyResult = HistoryResult.None;
				if (ma != null && mb != null) {
					if (ma.Documentation != mb.Documentation) historyResult=HistoryResult.Changed;
					else historyResult=HistoryResult.Equal;
				}
				else if (ma != null) historyResult=HistoryResult.Added;
				else if (mb != null) historyResult=HistoryResult.Removed;


				var r = Result.None;
				if (ma != null && mb != null) {
					if(ma.Documentation != mb.Documentation) r= Result.Different;
					else r = Result.Equal;
				}
				else if (ma != null) r = Result.OnlyLeft;
				else if (mb != null) r = Result.OnlyRight;

				if (ma != null && mb != null) result.Add(new MemberCompareResult(s,  r) {MemberA = ma, MemberB = mb,HistoryResult = historyResult});
				else if (ma != null) result.Add(new MemberCompareResult(s, r) {MemberA  = ma, HistoryResult = historyResult});
				else if (mb != null) result.Add(new MemberCompareResult(s, r) {MemberB = mb, HistoryResult = historyResult});
			}

			return result;
		}

	}

}
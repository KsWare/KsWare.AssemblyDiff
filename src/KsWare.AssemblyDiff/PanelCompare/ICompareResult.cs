using KsWare.Presentation.BusinessFramework;

namespace KsWare.AssemblyDiff.PanelCompare {

	public interface ICompareResult:IObjectSlimBM {
		string DisplayName { get; set; }
		bool IsSelected { get; set; }
		bool IsExpanded { get; set; }
		ICompareResult[] SubResults { get; set; }
		string Name { get; set; }
		string NameLeft { get; set; }
		string NameRight { get; set; }
		Result Result { get; set; }

		HistoryResult HistoryResult { get; set; }
	}

}
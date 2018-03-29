using KsWare.Presentation.BusinessFramework;

namespace KsWare.AssemblyDiff.PanelCompare {

	public class CompareResult : ObjectSlimBM, ICompareResult {

		public CompareResult(string name, Result result) {
			NameLeft = NameRight = Name = name;
			Result   = result;
			switch (result) {
				case Result.OnlyLeft:
					NameRight = null;
					break;
				case Result.OnlyRight:
					NameLeft = null;
					break;
			}
		}

		public ICompareResult[] SubResults { get => Fields.GetValue<ICompareResult[]>(); set => Fields.SetValue(value); }

		public Result Result { get => Fields.GetValue<Result>(); set => Fields.SetValue(value); }

		public string Name { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		public string NameLeft { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		public string NameRight { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		public string DisplayName { get => Fields.GetValue<string>(); set => Fields.SetValue(value); }

		public bool IsExpanded { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }

		public bool IsSelected { get => Fields.GetValue<bool>(); set => Fields.SetValue(value); }
	}

}
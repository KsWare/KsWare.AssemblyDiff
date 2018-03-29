using KsWare.DependencyWalker.AppDomainWorkers;

namespace KsWare.AssemblyDiff.PanelCompare {

	public class TypeCompareResult : CompareResult {

		public TypeCompareResult(string name, Result result) : base(name, result) { }

		public MyTypeInfo TypeA { get => Fields.GetValue<MyTypeInfo>(); set => Fields.SetValue(value); }
		public MyTypeInfo TypeB { get => Fields.GetValue<MyTypeInfo>(); set => Fields.SetValue(value); }
	}

}
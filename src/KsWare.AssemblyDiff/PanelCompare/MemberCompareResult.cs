using KsWare.DependencyWalker.AppDomainWorkers;

namespace KsWare.AssemblyDiff.PanelCompare {

	public class MemberCompareResult : CompareResult {

		public MemberCompareResult(string name, Result result) : base(name, result) { }

		
		public MyMemberInfo MemberA { get => Fields.GetValue<MyMemberInfo>(); set => Fields.SetValue(value); }
		public MyMemberInfo MemberB { get => Fields.GetValue<MyMemberInfo>(); set => Fields.SetValue(value); }
	}

}
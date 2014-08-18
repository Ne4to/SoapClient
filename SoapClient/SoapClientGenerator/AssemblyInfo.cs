using System.Collections.Generic;

namespace SoapClientGenerator
{
	internal class AssemblyInfo
	{
		public List<ServiceInterface> Services { get; set; }
		public List<ContractInfo> Contracts { get; set; }
		public List<EnumInfo> Enums { get; set; }
	}
}
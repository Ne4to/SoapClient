using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SoapClientGenerator
{
	internal class ServiceInterface
	{
		public TypeInfo TypeInfo { get; set; }
		public string Name { get; set; }
		public string Namespace { get; set; }
		public List<ServiceMethod> Methods { get; set; }

		public ServiceInterface(TypeInfo typeInfo)
		{
			TypeInfo = typeInfo;

			Name = typeInfo.Name;

			var serviceContractAttr = typeInfo.GetCustomAttribute<ServiceContractAttribute>();
			Namespace = serviceContractAttr.Namespace;

			var q = from m in typeInfo.DeclaredMethods
				let operationContractAttr = m.GetCustomAttribute<OperationContractAttribute>()
				where operationContractAttr != null && m.ReturnType.IsSubclassOf(typeof(Task))
				select new ServiceMethod(m);

			Methods = new List<ServiceMethod>(q);
		}
	}
}
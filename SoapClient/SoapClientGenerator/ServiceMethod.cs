using System.Reflection;
using System.ServiceModel;

namespace SoapClientGenerator
{
	internal class ServiceMethod
	{
		public MethodInfo MethodInfo { get; set; }

		public string Name { get; set; }
		public string Action { get; set; }

		public ServiceMethod(MethodInfo methodInfo)
		{
			MethodInfo = methodInfo;
			Name = methodInfo.Name;

			var operationContractAttr = methodInfo.GetCustomAttribute<OperationContractAttribute>();
			Action = operationContractAttr.Action;
		}
	}
}
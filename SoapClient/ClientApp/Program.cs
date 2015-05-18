using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ClientApp.ServiceReference1;
using ClientApp.ServiceReference4;

namespace ClientApp
{
	class Program
	{
		static void Main(string[] args)
		{
			//ServiceReference1.Service1Client cl = new Service1Client();
			//var x = cl.GetData(new GetDataRequest(10));

			var x = cl.GetDataUsingDataContract(new GetDataUsingDataContractRequest(new CompositeType()
			{
				BoolValue = true,
				StringValue = "test str"
			}));

			//ServiceReference4.WeatherSoapClient cl = new WeatherSoapClient(new BasicHttpBinding(), new EndpointAddress("http://wsf.cdyne.com/WeatherWS/Weather.asmx"));
			//cl.GetWeatherInformation();
		}
	}
}

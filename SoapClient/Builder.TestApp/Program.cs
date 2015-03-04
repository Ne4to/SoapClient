using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SoapClientBuilder;

namespace Builder.TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			//LoadTestServiceWsdl();
			//CreateTestServiceClient();

			//LoadRussianPostWsdl();
			//CreateRussianPostClient();

			//LoadOnvifDmWsdl();
			//CreateOnvifDmClient();

			//LoadWeatherWsdl();
			CreateWeatherClient();
		}

		private static void LoadTestServiceWsdl()
		{
			Load("http://localhost:57368/Service1.svc?wsdl", "WcfService1.wsdl");
		}

		private static void CreateTestServiceClient()
		{
			CreateClient("WcfService1.wsdl", @"1.cs");
		}

		private static void LoadRussianPostWsdl()
		{
			Load("http://voh.russianpost.ru:8080/niips-operationhistory-web/OperationHistory?wsdl", "WcfService2.wsdl");
		}

		private static void CreateRussianPostClient()
		{
			CreateClient("WcfService2.wsdl", @"2.cs");
		}

		private static void LoadOnvifDmWsdl()
		{
			Load("http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl", "WcfService3.wsdl");
		}

		private static void CreateOnvifDmClient()
		{
			CreateClient("WcfService3.wsdl", @"3.cs");
		}

		private static void LoadWeatherWsdl()
		{
			Load("http://wsf.cdyne.com/WeatherWS/Weather.asmx?WSDL", "WcfService4.wsdl");
		}

		private static void CreateWeatherClient()
		{
			CreateClient("WcfService4.wsdl", @"4.cs");
		}
		
		private static void Load(string uri, string filename)
		{
			WsdlDocumentLoader loader = new WsdlDocumentLoader();
			var doc = loader.LoadAsync(new Uri(uri)).Result;
			doc.Save(filename);
		}

		private static void CreateClient(string wsdlFilename, string outFileName)
		{
			var wsdlDoc = XDocument.Parse(File.ReadAllText(wsdlFilename));
			var builder = new ProxyBuilder(wsdlDoc);
			builder.Run();

			var sourceCode = builder.GetSourceCode();

			File.WriteAllText(@"C:\Users\Alexey\Source\Repos\SoapClient\SoapClient\WcfService1ClientApp\" + outFileName, sourceCode);
		}
	}
}

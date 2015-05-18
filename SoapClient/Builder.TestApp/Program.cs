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

			//LoadWeatherWsdl();
			//CreateWeatherClient();
			return;

			// http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl
			// http://www.onvif.org/onvif/ver10/media/wsdl/media.wsdl
			// http://www.onvif.org/onvif/ver20/ptz/wsdl/ptz.wsdl

			//LoadOnvifDmWsdl();
			CreateOnvifDmClient();
			
			//LoadOnvifMediaWsdl();
			CreateOnvifMediaClient();

			//LoadOnvifPtzWsdl();
			CreateOnvifPtzClient();
		}

		private static void LoadTestServiceWsdl()
		{
			Load("http://localhost:57368/Service1.svc?wsdl", "WcfService1.wsdl");
		}

		private static void CreateTestServiceClient()
		{
			CreateClient("WcfService1.wsdl", @"1.cs", 1);
		}

		private static void LoadRussianPostWsdl()
		{
			Load("http://voh.russianpost.ru:8080/niips-operationhistory-web/OperationHistory?wsdl", "RussianPost.wsdl");
		}

		private static void CreateRussianPostClient()
		{
			CreateClient("RussianPost.wsdl", @"RussianPost.cs", 2);
		}

		private static void LoadOnvifDmWsdl()
		{
			Load("http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl", "OnvifDm.wsdl");
		}

		private static void CreateOnvifDmClient()
		{
			CreateClient("OnvifDm.wsdl", @"OnvifDm.cs", "AstroSoft.WindowsStore.Onvif.Proxies.OnvifServices.DeviceManagement");
		}

		private static void LoadWeatherWsdl()
		{
			Load("http://wsf.cdyne.com/WeatherWS/Weather.asmx?WSDL", "WeatherWS.wsdl");
		}

		private static void CreateWeatherClient()
		{
			CreateClient("WeatherWS.wsdl", @"WeatherWS.cs", 4);
		}

		private static void LoadOnvifMediaWsdl()
		{
			Load("http://www.onvif.org/onvif/ver10/media/wsdl/media.wsdl", "OnvifMedia.wsdl");
		}

		private static void CreateOnvifMediaClient()
		{
			CreateClient("OnvifMedia.wsdl", @"OnvifMedia.cs", "AstroSoft.WindowsStore.Onvif.Proxies.OnvifServices.Media");
		}

		private static void LoadOnvifPtzWsdl()
		{
			Load("http://www.onvif.org/onvif/ver20/ptz/wsdl/ptz.wsdl", "OnvifPtz.wsdl");
		}

		private static void CreateOnvifPtzClient()
		{
			CreateClient("OnvifPtz.wsdl", @"OnvifPtz.cs", "AstroSoft.WindowsStore.Onvif.Proxies.OnvifServices.PTZ");
		}
		
		private static void Load(string uri, string filename)
		{
			WsdlDocumentLoader loader = new WsdlDocumentLoader();
			var doc = loader.LoadAsync(new Uri(uri)).Result;
			doc.Save(filename);
		}

		private static void CreateClient(string wsdlFilename, string outFileName, int i)
		{
			var wsdlDoc = XDocument.Parse(File.ReadAllText(wsdlFilename));
			var builder = new ProxyBuilder(wsdlDoc, new BuilderParameters()
			{
				CodeNamespace = "MyNs" + i
			});
			builder.Run();

			var sourceCode = builder.GetSourceCode();

			File.WriteAllText(@"C:\Users\Alexey\Source\Repos\SoapClient\SoapClient\WcfService1ClientApp\" + outFileName, sourceCode);
		}

		private static void CreateClient(string wsdlFilename, string outFileName, string ns)
		{
			var wsdlDoc = XDocument.Parse(File.ReadAllText(wsdlFilename));
			var builder = new ProxyBuilder(wsdlDoc, new BuilderParameters()
			{
				CodeNamespace = ns
			});
			builder.Run();

			var sourceCode = builder.GetSourceCode();

			File.WriteAllText(@"C:\Users\Alexey\Source\Repos\SoapClient\SoapClient\WcfService1ClientApp\" + outFileName, sourceCode);
		}

	}
}

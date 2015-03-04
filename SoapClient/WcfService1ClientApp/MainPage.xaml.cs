using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Aaaa;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WcfService1ClientApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

	    private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
	    {
		    Service1Client client = new Service1Client();			
			client.EndpointAddress = new Uri("http://ne4topc/soaptest/Service1.svc");
		    //client.ContentType = "text/xml";
		    //client.EnvelopeNamespace = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");

 		    var x = await client.GetData(new GetData()
		    {
			    value = 456
		    });

		    var y = await client.GetDataUsingDataContract(new GetDataUsingDataContract()
		    {
			    composite = new CompositeType()
			    {
				    BoolValue = true,
					StringValue = "ssss"
			    }
		    });
	    }
    }
}

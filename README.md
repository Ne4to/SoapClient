SoapClient - WinRT SOAP Client
==========

Windows Phone 8.1 does not contain the System.ServiceModel namespace. 
The application cannot use WCF services.
The project allows to generate SOAP Client based on HttpClient.
It also can be used for Windows 8.1 to overcome WCF limitations such as sending the 'Except 100-Continue' HTTP header.

Syntax: SoapClientGenerator.exe <metadataDocumentPath> <file> <namespace> [/svcutil:<svcutilPath>]
<metadataDocumentPath> - The path to a metadata document (wsdl)
<file>                 - Output file path
<namespace>            - Output file namespace
<svcutil>              - SvcUtil.exe path

Example: SoapClientGenerator.exe \"http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl\" \"C:\\temp\\devicemgmt.wsdl.cs\" OnvifServices.DeviceManagement
Example: SoapClientGenerator.exe \"http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl\" \"C:\\temp\\devicemgmt.wsdl.cs\" OnvifServices.DeviceManagement /svcutil:\"C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.0A\\bin\\NETFX 4.0 Tools\\SvcUtil.exe\"

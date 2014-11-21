SoapClient - WinRT SOAP Client
==========

Windows Phone 8.1 does not contain the System.ServiceModel namespace. <br />
The applications cannot use WCF services. <br />
The project allows to generate SOAP Client based on HttpClient. <br />
It also can be used for Windows 8.1 to overcome WCF limitations such as sending the 'Except 100-Continue' HTTP header.<br />

Syntax: SoapClientGenerator.exe `<metadataDocumentPath>` `<file>` `<namespace>` [/svcutil:`<svcutilPath>`]<br />
>`<metadataDocumentPath>` - The path to a metadata document (wsdl)<br />
>`<file>`                 - Output file path<br />
>`<namespace>`            - Output file namespace<br />
>`<svcutil>`              - SvcUtil.exe path<br />


Example: SoapClientGenerator.exe "http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl" "C:\\temp\\devicemgmt.wsdl.cs" OnvifServices.DeviceManagement<br />
Example: SoapClientGenerator.exe "http://www.onvif.org/onvif/ver10/device/wsdl/devicemgmt.wsdl" "C:\\temp\\devicemgmt.wsdl.cs" OnvifServices.DeviceManagement /svcutil:"C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.0A\\bin\\NETFX 4.0 Tools\\SvcUtil.exe"<br />

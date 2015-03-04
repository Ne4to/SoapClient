using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoapClientBuilder
{
	// TODO Authentication
	// TODO handle web uri
	public class WsdlDocumentLoader
	{
		private HttpClient client;
		readonly Dictionary<string, XElement> _loadedSchemas = new Dictionary<string, XElement>();

		public WsdlDocumentLoader()
		{
			client = new HttpClient();
		}

		public async Task<XDocument> LoadAsync(Uri uri)
		{
			var wsdl = await client.GetStringAsync(uri);
			var wsdlDoc = XDocument.Parse(wsdl);

			var typesElement = wsdlDoc.Root.Element(Namespaces.Wsdl + "types");
			foreach (var schemaElement in typesElement.Elements(Namespaces.Xsd + "schema"))
			{
				await AddAsync(typesElement, schemaElement, uri);
			}

			return wsdlDoc;
		}

		private async Task AddAsync(XElement rootTypesElement, XElement schemaElement, Uri baseUri)
		{
			foreach (var importElement in schemaElement.Elements(Namespaces.Xsd + "import"))
			{
				var schemaLocation = importElement.Attribute("schemaLocation").Value;

				Uri schemaUri;

				try
				{					
					schemaUri = new Uri(schemaLocation);
				}
				catch (UriFormatException)
				{
					schemaUri = new Uri(baseUri, schemaLocation);
				}

				var schemaNamespace = importElement.Attribute("namespace").Value;
				XElement schema;
				if (!_loadedSchemas.TryGetValue(schemaNamespace, out schema))
				{
					var schemaContent = await client.GetStringAsync(schemaUri);					
					schema = XElement.Parse(schemaContent);
					rootTypesElement.Add(schema);					

					_loadedSchemas.Add(schemaNamespace, schema);
				}

				await AddAsync(rootTypesElement, schema, schemaUri);
			}
		}
	}
}
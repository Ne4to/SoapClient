using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoapClientBuilder
{
	public class WsdlDocumentLoader
	{
		readonly Dictionary<string, XElement> _loadedSchemas = new Dictionary<string, XElement>();

		public async Task<XDocument> LoadAsync(Uri uri)
		{
			var client = new HttpClient();
			var wsdl = await client.GetStringAsync(uri);
			var wsdlDoc = XDocument.Parse(wsdl);

			var typesElement = wsdlDoc.Root.Element(Namespaces.Wsdl + "types");
			foreach (var schemaElement in typesElement.Elements(Namespaces.Xsd + "schema"))
			{
				await AddAsync(schemaElement, client, typesElement, uri);
			}

			return wsdlDoc;
		}

		private async Task AddAsync(XElement schemaElement, HttpClient client, XElement typesElement, Uri uri)
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
					schemaUri = new Uri(uri, schemaLocation);
				}

				var schemaNamespace = importElement.Attribute("namespace").Value;
				XElement schema;
				if (!_loadedSchemas.TryGetValue(schemaNamespace, out schema))
				{
					var schemaContent = await client.GetStringAsync(schemaUri);					
					schema = XElement.Parse(schemaContent);
					typesElement.Add(schema);					

					_loadedSchemas.Add(schemaNamespace, schema);
				}
				else
				{
					//var schemaContent = await client.GetStringAsync(schemaUri);
					//schema = XElement.Parse(schemaContent);
					
				}

				await AddAsync(schema, client, typesElement, schemaUri);

				//importElement.Remove();
			}
		}
	}
}
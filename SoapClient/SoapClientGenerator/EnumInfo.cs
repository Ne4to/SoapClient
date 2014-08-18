using System.CodeDom;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace SoapClientGenerator
{
	internal class EnumInfo
	{
		public TypeInfo Info { get; set; }

		public EnumInfo(TypeInfo info)
		{
			Info = info;
		}

		public CodeTypeDeclaration GetCodeDeclaration()
		{
			var targetEnum = new CodeTypeDeclaration(Info.Name);
			targetEnum.IsEnum = true;
			targetEnum.TypeAttributes = TypeAttributes.Public;

			foreach (var memberInfo in Info.DeclaredMembers.Where(m => m.Name != "value__"))
			{
				var f = new CodeMemberField(memberInfo.Name, memberInfo.Name);

				var xmlEnumAttr = memberInfo.GetCustomAttribute<XmlEnumAttribute>();
				if (xmlEnumAttr != null)
				{
					var attrType = new CodeTypeReference(typeof (XmlEnumAttribute));
					f.CustomAttributes.Add(new CodeAttributeDeclaration(attrType,
						new CodeAttributeArgument(new CodePrimitiveExpression(xmlEnumAttr.Name))));
				}

				targetEnum.Members.Add(f);
			}

			return targetEnum;
		}
	}
}
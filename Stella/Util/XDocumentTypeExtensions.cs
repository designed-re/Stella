using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Stella.Util
{
    /// <summary>
    /// Extension methods to automatically add __type attributes to XDocument elements
    /// based on the actual C# type information from the response object.
    /// </summary>
    public static class XDocumentTypeExtensions
    {
        /// <summary>
        /// Adds __type attributes to all elements in the document based on the response type.
        /// </summary>
        public static XDocument AddKBinTypesFromResponse(this XDocument document, object response)
        {
            if (document?.Root == null || response == null)
                return document;

            var responseType = response.GetType();
            
            // Get the XmlRoot attribute to find the actual root element
            var xmlRootAttr = responseType.GetCustomAttribute<XmlRootAttribute>();
            var rootElementName = xmlRootAttr?.ElementName ?? responseType.Name;
            
            // Find the actual element in the document that corresponds to the response type
            var rootElement = FindElementByName(document.Root, rootElementName);
            if (rootElement != null)
            {
                ProcessElementWithType(rootElement, response, responseType);
            }

            return document;
        }

        private static XElement FindElementByName(XElement parent, string elementName)
        {
            // Check if current element matches
            if (parent.Name.LocalName.Equals(elementName, StringComparison.OrdinalIgnoreCase))
                return parent;

            // Recursively search in children
            foreach (var child in parent.Elements())
            {
                var result = FindElementByName(child, elementName);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static void ProcessElementWithType(XElement element, object obj, Type objType)
        {
            // Get all public properties from the object type
            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(obj);
                var propertyType = property.PropertyType;

                // Skip if value is null
                if (propertyValue == null)
                    continue;

                // Check if property has XmlAttribute attribute
                var xmlAttrAttr = property.GetCustomAttribute<XmlAttributeAttribute>();
                if (xmlAttrAttr != null)
                {
                    // Handle attribute
                    var attrName = xmlAttrAttr.AttributeName ?? property.Name;
                    var attr = element.Attribute(attrName);
                    
                    if (attr != null)
                    {
                        // Convert bool values to "1" or "0"
                        if (GetUnderlyingType(propertyType) == typeof(bool))
                        {
                            bool boolValue = (bool)propertyValue;
                            attr.Value = boolValue ? "1" : "0";
                        }
                    }
                    continue;
                }

                // Handle List<T> / IList<T> collections
                if (IsListType(propertyType, out var elementType))
                {
                    ProcessListProperty(element, property, propertyValue, elementType);
                    continue;
                }

                // Check if property has XmlElement attribute to get the actual element name
                var xmlElemAttr = property.GetCustomAttribute<XmlElementAttribute>();
                string childElementName = null;
                
                if (xmlElemAttr != null && !string.IsNullOrEmpty(xmlElemAttr.ElementName))
                {
                    childElementName = xmlElemAttr.ElementName;
                }

                // Try to find matching child element (case-insensitive)
                var childElement = FindChildElement(element, childElementName ?? property.Name);
                if (childElement == null)
                    continue;

                // If property is a complex type (class), recurse into it
                if (IsComplexType(propertyType))
                {
                    // Only recurse if element has child elements
                    if (childElement.Elements().Any())
                    {
                        ProcessElementWithType(childElement, propertyValue, propertyType);
                    }
                }
                else if (!childElement.Elements().Any())
                {
                    // For primitive/simple types, ensure value is set and add __type attribute
                    
                    // If PropertyValue is not null but XML value is empty, set it from PropertyValue
                    if (propertyValue != null && string.IsNullOrEmpty(childElement.Value))
                    {
                        childElement.Value = propertyValue.ToString();
                    }
                    
                    // Add __type attribute based on C# type for primitive/simple types
                    var kbinType = GetKBinType(propertyType, property.Name, childElement.Value);
                    if (kbinType != null)
                    {
                        childElement.SetAttributeValue("__type", kbinType);

                        // Convert bool values to "1" or "0"
                        if (GetUnderlyingType(propertyType) == typeof(bool))
                        {
                            bool boolValue = (bool)propertyValue;
                            childElement.Value = boolValue ? "1" : "0";
                        }
                    }
                }
            }
        }

        private static bool IsListType(Type type, out Type elementType)
        {
            elementType = null;

            // Check if type is List<T> or IList<T>
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                if (genericDef == typeof(List<>) || 
                    genericDef == typeof(System.Collections.Generic.IList<>) ||
                    genericDef == typeof(System.Collections.Generic.IEnumerable<>))
                {
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }

            return false;
        }

        private static void ProcessListProperty(XElement parent, PropertyInfo property, object listValue, Type elementType)
        {
            // Get the XML element name for list items
            var xmlElementName = GetListElementName(elementType, property.Name);
            
            // Find all elements with this name
            var listElements = parent.Elements(xmlElementName).ToList();
            
            if (listElements.Count == 0)
                return;

            // If element type is complex, process each element
            if (IsComplexType(elementType))
            {
                // Get the actual items from the list
                var items = (System.Collections.IEnumerable)listValue;
                var itemIndex = 0;
                
                foreach (var item in items)
                {
                    if (itemIndex < listElements.Count)
                    {
                        ProcessElementWithType(listElements[itemIndex], item, elementType);
                        itemIndex++;
                    }
                }
            }
            else if (IsNumericType(elementType))
            {
                // For numeric types in list, combine all values into first element with __count attribute
                var items = (System.Collections.IEnumerable)listValue;
                var itemList = new List<object>();
                
                foreach (var item in items)
                {
                    itemList.Add(item);
                }

                if (itemList.Count > 0 && listElements.Count > 0)
                {
                    var firstElem = listElements[0];
                    var kbinType = GetKBinType(elementType, property.Name);
                    
                    if (kbinType != null)
                    {
                        // Set __type attribute
                        firstElem.SetAttributeValue("__type", kbinType);
                        
                        // Set __count attribute
                        firstElem.SetAttributeValue("__count", itemList.Count.ToString());
                        
                        // Combine all values as space-separated string
                        var values = string.Join(" ", itemList.Select(item => item.ToString()));
                        firstElem.Value = values;
                    }
                    
                    // Remove extra elements (keep only the first one)
                    for (int i = 1; i < listElements.Count; i++)
                    {
                        listElements[i].Remove();
                    }
                }
            }
            else
            {
                // For primitive string/other types in list, add __type to each element
                var kbinType = GetKBinType(elementType, property.Name);
                if (kbinType != null)
                {
                    var items = (System.Collections.IEnumerable)listValue;
                    var itemIndex = 0;

                    foreach (var item in items)
                    {
                        if (itemIndex < listElements.Count)
                        {
                            var elem = listElements[itemIndex];
                            if (!elem.Elements().Any() && !string.IsNullOrEmpty(elem.Value))
                            {
                                elem.SetAttributeValue("__type", kbinType);

                                // Convert bool values to "1" or "0"
                                if (GetUnderlyingType(elementType) == typeof(bool) && item is bool boolValue)
                                {
                                    elem.Value = boolValue ? "1" : "0";
                                }
                            }
                        }
                        itemIndex++;
                    }
                }
            }
        }

        private static string GetListElementName(Type elementType, string propertyName)
        {
            // Try to get XmlElement attribute from the class
            var xmlElementAttr = elementType.GetCustomAttribute<XmlElementAttribute>();
            if (xmlElementAttr?.ElementName != null)
                return xmlElementAttr.ElementName;

            // Otherwise, convert property name from plural to singular or use default naming
            // Infos ¡æ info, Items ¡æ item, etc.
            var name = propertyName;
            if (name.EndsWith("ies"))
                name = name.Substring(0, name.Length - 3) + "y";
            else if (name.EndsWith("s") && !name.EndsWith("ss"))
                name = name.Substring(0, name.Length - 1);

            return ConvertToXmlElementName(name);
        }

        private static XElement FindChildElement(XElement parent, string propertyName)
        {
            // First, try to get the XmlElement attribute from the property
            // This requires reflection on the parent object type, so we search by name
            
            // Convert C# property name to XML element name (PascalCase to lowercase/snake_case)
            var xmlElementName = ConvertToXmlElementName(propertyName);
            
            // Try exact match first
            var element = parent.Element(xmlElementName);
            if (element != null)
                return element;

            // Try case-insensitive match
            var caseInsensitiveMatch = parent.Elements()
                .FirstOrDefault(e => e.Name.LocalName.Equals(xmlElementName, StringComparison.OrdinalIgnoreCase));
            
            if (caseInsensitiveMatch != null)
                return caseInsensitiveMatch;

            // Try snake_case conversion (MusicLimited ¡æ music_limited)
            var snakeCaseName = ConvertToSnakeCaseElementName(propertyName);
            var snakeCaseMatch = parent.Elements()
                .FirstOrDefault(e => e.Name.LocalName.Equals(snakeCaseName, StringComparison.OrdinalIgnoreCase));
            
            return snakeCaseMatch;
        }

        private static string ConvertToXmlElementName(string propertyName)
        {
            // Simple approach: just lowercase the first letter
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            return char.ToLower(propertyName[0]) + propertyName.Substring(1);
        }

        private static string ConvertToSnakeCaseElementName(string propertyName)
        {
            // Convert PascalCase to snake_case
            // MusicLimited ¡æ music_limited
            // SkillLevel ¡æ skill_level
            // ValgeneId ¡æ valgene_id
            
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            var result = new System.Text.StringBuilder();
            
            for (int i = 0; i < propertyName.Length; i++)
            {
                char c = propertyName[i];
                
                if (char.IsUpper(c) && i > 0)
                {
                    // Add underscore before uppercase letter (except at start)
                    result.Append('_');
                    result.Append(char.ToLower(c));
                }
                else
                {
                    result.Append(char.ToLower(c));
                }
            }
            
            return result.ToString();
        }

        private static bool IsComplexType(Type type)
        {
            // Exclude string and primitive types
            if (type == typeof(string) || type.IsPrimitive || type == typeof(decimal))
                return false;

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return IsComplexType(underlyingType);
            }

            // Classes and other reference types are complex types
            return type.IsClass;
        }

        private static Type GetUnderlyingType(Type type)
        {
            // Get the underlying type for nullable types
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static bool IsNumericType(Type type)
        {
            // Get the underlying type for nullable types
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType == typeof(byte) ||
                   underlyingType == typeof(sbyte) ||
                   underlyingType == typeof(short) ||
                   underlyingType == typeof(ushort) ||
                   underlyingType == typeof(int) ||
                   underlyingType == typeof(uint) ||
                   underlyingType == typeof(long) ||
                   underlyingType == typeof(ulong) ||
                   underlyingType == typeof(float) ||
                   underlyingType == typeof(double);
        }

        private static string GetKBinType(Type type, string propertyName = null, string value = null)
        {
            // Check if property name suggests it's an IP address
            if (!string.IsNullOrEmpty(propertyName) && 
                (propertyName.Contains("ip", StringComparison.OrdinalIgnoreCase) || 
                 propertyName.Contains("Ip", StringComparison.Ordinal)))
            {
                return "ip4";
            }

            // Check if value looks like an IP address (IPv4)
            if (!string.IsNullOrEmpty(value) && IsValidIPv4(value))
            {
                return "ip4";
            }

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // String types
            if (underlyingType == typeof(string))
                return "str";

            // Boolean
            if (underlyingType == typeof(bool))
                return "bool";

            // Unsigned integer types
            if (underlyingType == typeof(byte))
                return "u8";

            if (underlyingType == typeof(ushort))
                return "u16";

            if (underlyingType == typeof(uint))
                return "u32";

            if (underlyingType == typeof(ulong))
                return "u64";

            // Signed integer types
            if (underlyingType == typeof(sbyte))
                return "s8";

            if (underlyingType == typeof(short))
                return "s16";

            if (underlyingType == typeof(int))
                return "s32";

            if (underlyingType == typeof(long))
                return "s64";

            // Floating point types
            if (underlyingType == typeof(float))
                return "f32";

            if (underlyingType == typeof(double))
                return "f64";

            // Default
            return null;
        }

        private static bool IsValidIPv4(string value)
        {
            // Simple IPv4 validation (e.g., 127.0.0.1, 192.168.1.1)
            if (string.IsNullOrEmpty(value))
                return false;

            var parts = value.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!byte.TryParse(part, out _))
                    return false;
            }

            return true;
        }
    }
}



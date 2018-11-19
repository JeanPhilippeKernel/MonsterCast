using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MonsterCast.Core.Helpers
{
    public static class XmlParser
    {
        private static List<dynamic> _collection =  new List<dynamic>();
        private static ExpandoObject _currentItem = new ExpandoObject();
            
        public static Task<List<dynamic>> Parse(Stream _stream)
        {
            return Task.Run(()=> 
            {
                
                XElement rootElement = XElement.Load(_stream);
                var itemCollection = rootElement.Descendants("item");
                foreach (var item in itemCollection)
                {
                    var itemDescendant = item.Descendants();
                    foreach (var descendant in itemDescendant)
                    {
                        if (descendant.HasAttributes)
                        {
                            var attributesCollection = descendant.Attributes();
                            Dictionary<string, string> attributes = new Dictionary<string, string>();
                            
                            attributes.Add("value", descendant.Value);
                            foreach (var attribut in attributesCollection)
                            {
                                attributes.Add(attribut.Name.LocalName, attribut.Value);
                            }
                            ((IDictionary<string, object>)_currentItem).Add(descendant.Name.LocalName, attributes);
                        }
                        else
                            ((IDictionary<string, object>)_currentItem).Add(descendant.Name.LocalName, descendant.Value);                    
                    }
                    _collection.Add(_currentItem);
                    _currentItem = new ExpandoObject();
                } 
                return _collection;
            });
        }
    }
}

using System.Xml.Serialization;

namespace Cake.BenchmarkDotNet.Printer.Trx.Dto
{
    public class TestList
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}

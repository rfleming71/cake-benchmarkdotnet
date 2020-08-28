using System.Xml.Serialization;

namespace Cake.BenchmarkDotNet.Printer.Trx.Dto
{
    public class UnitTest
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("storage")]
        public string Storage { get; set; }
        [XmlAttribute("id")]
        public string Id { get; set; }
        public Execution Execution { get; set; } = new Execution();
        public TestMethod TestMethod { get; set; }
    }

    public class Execution
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
    }

    public class TestMethod
    {
        [XmlAttribute("codeBase")]
        public string CodeBase { get; set; }
        [XmlAttribute("className")]
        public string ClassName { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}

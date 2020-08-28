using System.Xml.Serialization;

namespace Cake.BenchmarkDotNet.Printer.Trx.Dto
{
    public class TestEntry
    {
        [XmlAttribute("testId")]
        public string TestId { get; set; }

        [XmlAttribute("executionId")]
        public string ExecutionId { get; set; }

        [XmlAttribute("testListId")]
        public string TestListId { get; set; }
    }
}

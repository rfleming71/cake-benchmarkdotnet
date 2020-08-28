using System;
using System.Xml.Serialization;

namespace Cake.BenchmarkDotNet.Printer.Trx.Dto
{
    public class UnitTestResult
    {
        [XmlAttribute("executionId")]
        public string ExecutionId { get; set; } = Guid.NewGuid().ToString();

        [XmlAttribute("testId")]
        public string TestId { get; set; } = Guid.NewGuid().ToString();

        [XmlAttribute("testName")]
        public string TestName { get; set; }

        [XmlAttribute("computerName")]
        public string ComputerName { get; set; } = Environment.MachineName;

        [XmlAttribute("testType")]
        public string TestType { get; set; } = "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b";

        [XmlAttribute("testListId")]
        public string TestListId { get; set; }

        [XmlAttribute("outcome")]
        public string Outcome { get; set; }

        [XmlAttribute("relativeResultsDirectory")]
        public string RelativeResultsDirectory { set { } get { return ExecutionId; } }

        public Output Output { get; set; } 
    }

    public class Output
    {
        public ErrorInfo ErrorInfo { get; set; } = new ErrorInfo();
    }

    public class ErrorInfo
    {
        public string Message { get; set; }
    }
}

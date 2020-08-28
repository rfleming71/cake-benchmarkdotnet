using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Cake.BenchmarkDotNet.Printer.Trx.Dto
{
    [XmlRoot(Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public class TestRun
    {
        public TestRun()
        {
            TestLists.Add(new TestList()
            {
                Id = TestListId,
                Name = "Results Not in a List",
            });
        }

        public List<UnitTestResult> Results { get; set; } = new List<UnitTestResult>();
        public List<UnitTest> TestDefinitions { get; set; } = new List<UnitTest>();
        public List<TestEntry> TestEntries { get; set; } = new List<TestEntry>();
        public List<TestList> TestLists { get; set; } = new List<TestList>();

        [XmlAttribute("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [XmlAttribute("name")]
        public string Name { get; set; } = $"{Environment.MachineName} - Perf {DateTime.Now}";


        private string TestListId = Guid.NewGuid().ToString();

        public void Add(UnitTest unitTest, UnitTestResult result)
        {
            var testEntry = new TestEntry()
            {
                ExecutionId = Guid.NewGuid().ToString(),
                TestId = Guid.NewGuid().ToString(),
                TestListId = TestListId,
            };

            unitTest.Id = result.TestId = testEntry.TestId;
            unitTest.Execution.Id = result.ExecutionId = testEntry.ExecutionId;
            result.TestListId = TestListId;

            Results.Add(result);
            TestDefinitions.Add(unitTest);
            TestEntries.Add(testEntry);
        }
    }
}

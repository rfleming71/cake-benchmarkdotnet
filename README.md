## Cake.BenchmarkDotNet

Cake Addin for working with BenchmarkDotNet file.

### Usage ###
Comparing two runs of the benchmark files and outputs the results into a TRX format.
*Compare uses the json-full output*

    #addin "nuget:?package=Cake.BenchmarkDotNet&version=0.1.13&loaddependencies=true"
    Task("CompareBenchmarks")
    .Does(() => {
	    BenchmarkDotNetCompareResults("C:/temp/bdn/baseline", "C:/temp/bdn/new", new  BenchmarkDotNetCompareSettings() {
		    TrxFilePath  =  "C:/temp/bdn/test-results.trx",
		    TestThreshold  =  "5%",
	    });
    });

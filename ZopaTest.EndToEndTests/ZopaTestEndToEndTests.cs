using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ZopaTest.EndToEndTests
{
    [TestFixture]
    public class ZopaTestEndToEndTests
    {
        [Test, TestCaseSource("FileInputTestCaseSource")]
        public void Project_EndToEndTest(string projectNameString, string marketFile, string loanAmount, string outputString)
        {
            var p = GetProcess(projectNameString, marketFile, loanAmount);

            p.Start();

            StreamWriter myStreamWriter = p.StandardInput;

            var output = p.StandardOutput.ReadToEnd();
            p.StandardOutput.Close();

            myStreamWriter.WriteLine();

            p.WaitForExit();

            Assert.That(output, Is.EqualTo(outputString));
        }

        private Process GetProcess(string projectName, string marketFile, string loanAmount)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\..\..\..\{projectName}\bin\Debug\{projectName}.exe";
            p.StartInfo.Arguments = $"{marketFile} {loanAmount}";

            return p;
        }

        // the output uses 'L' instead of '£' as the default console font may not have the '£' symbol - https://stackoverflow.com/a/29077672
        // changing the console font is advised against as not a portable solution since not all machines have the same fonts available for console
        private static IEnumerable<TestCaseData> FileInputTestCaseSource
        {
            get
            {
                yield return new TestCaseData("ZopaTest", "\"..\\..\\market_offers\\Market Data for Exercise.csv\"", "1000", getTextFromFile("ZopaTest_Output_1.txt"));
                yield return new TestCaseData("ZopaTest", "\"some not existing file.csv\"", "1000", getTextFromFile("ZopaTest_Output_2.txt"));
                yield return new TestCaseData("ZopaTest", "\"..\\..\\market_offers\\Market Data for Exercise.csv\"", "800" , getTextFromFile("ZopaTest_Output_3.txt"));
                yield return new TestCaseData("ZopaTest", "\"..\\..\\market_offers\\Market Data for Exercise.csv\"", "30000", getTextFromFile("ZopaTest_Output_4.txt"));
                yield return new TestCaseData("ZopaTest", "\"..\\..\\market_offers\\Market Data for Exercise.csv\"", "8333", getTextFromFile("ZopaTest_Output_5.txt"));
                yield return new TestCaseData("ZopaTest", "\"..\\..\\market_offers\\Market Data for Exercise.csv\"", "8000", getTextFromFile("ZopaTest_Output_6.txt"));
            }
        }

        private static string getTextFromFile(string fileName)
        {
            return string.Concat(System.IO.File.ReadAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\..\..\TestCaseSources\{fileName}"), "\r\n");
        }
    }
}

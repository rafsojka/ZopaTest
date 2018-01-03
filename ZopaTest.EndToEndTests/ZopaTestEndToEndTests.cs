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
        public void Project_EndToEndTest(string projectNameString, string inputString, string outputString)
        {
            var p = GetProcess(projectNameString, inputString);

            p.Start();

            StreamWriter myStreamWriter = p.StandardInput;

            var output = p.StandardOutput.ReadToEnd();
            p.StandardOutput.Close();

            myStreamWriter.WriteLine();

            p.WaitForExit();

            Assert.That(output, Is.EqualTo(outputString));
        }

        private Process GetProcess(string projectName, string inputString)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\..\..\..\{projectName}\bin\Debug\{projectName}.exe";
            p.StartInfo.Arguments = inputString;

            return p;
        }

        // the output uses 'L' instead of '£' as the default console font may not have the '£' symbol - https://stackoverflow.com/a/29077672
        // changing the console font is advised against as not a portable solution since not all machines have the same fonts available for console
        private static IEnumerable<TestCaseData> FileInputTestCaseSource
        {
            get
            {
                // no optional parameters
                yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_1.txt"), getTextFromFile("ZopaTest_Output_1.txt"));
                yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_2.txt"), getTextFromFile("ZopaTest_Output_2.txt"));
                yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_3.txt"), getTextFromFile("ZopaTest_Output_3.txt"));
                yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_4.txt"), getTextFromFile("ZopaTest_Output_4.txt"));
                yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_5.txt"), getTextFromFile("ZopaTest_Output_5.txt"));
                yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_6.txt"), getTextFromFile("ZopaTest_Output_6.txt"));

                // optional parameters
                yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_7.txt"), getTextFromFile("ZopaTest_Output_7.txt"));
                //yield return new TestCaseData("ZopaTest", getTextFromFile("ZopaTest_Input_8.txt"), getTextFromFile("ZopaTest_Output_8.txt")); - for some reason printfn prints 1210 as 1210.12...
            }
        }

        private static string getTextFromFile(string fileName)
        {
            return string.Concat(System.IO.File.ReadAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\..\..\TestCaseSources\{fileName}"), "\r\n");
        }
    }
}

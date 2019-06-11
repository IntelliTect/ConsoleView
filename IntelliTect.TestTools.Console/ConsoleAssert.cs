﻿using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IntelliTect.TestTools.Console
{
    /// <summary>
    /// Provides assertion methods for tests that 
    /// </summary>
    public static class ConsoleAssert
    {
        /// <summary>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="action">Method to be run</param>
        /// <param name="normalizeLineEndings">Whether differences in line ending styles should be ignored.</param>
        public static string Expect(string expected, Action action, bool normalizeLineEndings = true)
        {
            return Expect(expected, action, (left, right) => left == right, normalizeLineEndings);
        }

        /// <summary>
        /// <para>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </para>
        /// <para>Newlines will not be normalized, and trailing newlines will not be trimmed.</para>
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="action">Method to be run</param>
        public static string ExpectNoTrimOutput(string expected, Action action)
        {
            return Expect(expected, action, (left, right) => left == right, false);
        }

        /// <summary>
        /// <para>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </para>
        /// <para>
        /// In addition to the checking of the console output, the return value of the 
        /// called function will be asserted for equality with <paramref name="expectedReturn"/>
        /// </para>
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="func">Method to be run</param>
        /// <param name="expectedReturn">Value against which equality with the method's return value will be asserted.</param>
        /// <param name="args">Args to pass to the function.</param>
        public static void Expect<T>(string expected, Func<string[], T> func, T expectedReturn = default, params string[] args)
        {
            T @return = default;
            Expect(expected, () => @return = func(args));

            if (!expectedReturn.Equals(@return))
            {
                throw new Exception($"The value returned from {nameof(func)} ({@return}) was not the { nameof(expectedReturn) }({expectedReturn}) value.");
            }
        }

        /// <summary>
        /// <para>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </para>
        /// <para>
        /// In addition to the checking of the console output, the return value of the 
        /// called function will be asserted for equality with <paramref name="expectedReturn"/>
        /// </para>
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="func">Method to be run</param>
        /// <param name="expectedReturn">Value against which equality with the method's return value will be asserted.</param>
        public static void Expect<T>(string expected, Func<T> func, T expectedReturn)
        {
            Expect(expected, (_) => func(), expectedReturn);
        }

        /// <summary>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="func">Method to be run</param>
        /// <param name="args">Args to pass to the function.</param>
        public static void Expect(string expected, Action<string[]> func, params string[] args) =>
            Expect(expected, () => func(args));

        /// <summary>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="action">Method to be run</param>
        /// <param name="comparisonOperator"></param>
        /// <param name="normalizeLineEndings">Whether differences in line ending styles should be ignored.</param>
        private static string Expect(string expected, Action action, Func<string, string, bool> comparisonOperator, bool normalizeLineEndings = true)
        {
            string[] data = Parse(expected);

            string input = data[0];
            string expectedOutput = data[1];

            return Execute(input, expectedOutput, action, comparisonOperator, normalizeLineEndings);
        }

        private static readonly Func<string, string, bool> LikeOperator =
            (expected, output) => output.IsLike(expected);

        /// <summary>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="action">Method to be run</param>
        public static string ExpectLike(string expected, Action action)
        {
            return Expect(expected, action, LikeOperator);
        }

        /// <summary>
        /// Performs a unit test on a console-based method. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="escapeCharacter"></param>
        /// <param name="action">Method to be run</param>
        public static string ExpectLike(string expected, char escapeCharacter, Action action)
        {
            return Expect(expected, action, (pattern, output) => output.IsLike(pattern, escapeCharacter));
        }

        /// <summary>
        /// Normalizes all line endings of the input string into <see cref="Environment.NewLine" />
        /// </summary>
        /// <param name="input">The input to normalize</param>
        /// <param name="trimTrailingNewline">True if trailing newlines should be trimmed.</param>
        /// <returns>The normalized input.</returns>
        public static string NormalizeLineEndings(string input, bool trimTrailingNewline = false)
        {
            // https://stackoverflow.com/questions/140926/normalize-newlines-in-c-sharp
            input = Regex.Replace(input, @"\r\n|\n\r|\n|\r", Environment.NewLine);

            if (trimTrailingNewline && input.EndsWith(Environment.NewLine))
            {
                input = input.Substring(0, input.Length - Environment.NewLine.Length);
            }

            return input;
        }

        /// <summary>
        /// Executes the unit test while providing console input.
        /// </summary>
        /// <param name="givenInput">Input which will be given</param>
        /// <param name="expectedOutput">The expected output</param>
        /// <param name="action">Action to be tested</param>
        /// <param name="areEquivalentOperator">delegate for comparing the expected from actual output.</param>
        /// <param name="normalizeLineEndings">Whether differences in line ending styles should be ignored.</param>
        private static string Execute(string givenInput, string expectedOutput, Action action,
            Func<string, string, bool> areEquivalentOperator, bool normalizeLineEndings = true)
        {
            string output = Execute(givenInput, action, normalizeLineEndings);

            if (normalizeLineEndings)
            {
                // output = NormalizeLineEndings(output, true);
                expectedOutput = NormalizeLineEndings(expectedOutput, true);
            }

            AssertExpectation(expectedOutput, output, areEquivalentOperator);
            return output;
        }

        private static void AssertExpectation(string expectedOutput, string output, Func<string, string, bool> areEquivalentOperator)
        {
            bool failTest = !areEquivalentOperator(expectedOutput, output);
            if (failTest)
            {
                throw new Exception(GetMessageText(expectedOutput, output));
            }
        }

        private static readonly object ExecuteLock = new object();

        /// <summary>
        /// Executes the <paramref name="action"/> while providing console input.
        /// </summary>
        /// <param name="givenInput">Input which will be given at the console when prompted</param>
        /// <param name="action">The action to run.</param>
        /// <param name="normalizeLineEndings">Whether differences in line ending styles should be ignored.</param>
        public static string Execute(string givenInput, Action action, bool normalizeLineEndings = true)
        {
            TextWriter savedOutputStream = System.Console.Out;
            TextReader savedInputStream = System.Console.In;
            try
            {
                lock (ExecuteLock)
                {
                    string output;
                    using (TextWriter writer = new StringWriter())
                    using (TextReader reader = new StringReader(string.IsNullOrWhiteSpace(givenInput) ? "" : givenInput))
                    {
                        System.Console.SetOut(writer);

                        System.Console.SetIn(reader);
                        action();

                        output = writer.ToString();
                        if (normalizeLineEndings)
                        {
                            output = NormalizeLineEndings(output, true);
                        }
                    }

                    return output;
                }
            }
            finally
            {
                System.Console.SetOut(savedOutputStream);
                System.Console.SetIn(savedInputStream);
            }
        }

        private static string GetMessageText(string expectedOutput, string output)
        {
            string result = "";

            if (expectedOutput.Length <= 2 || output.Length <= 2)
            {
                // Don't display differing lengths.
            }
            else
            {
                result = $"expected: {(int)expectedOutput[expectedOutput.Length - 2]}{(int)expectedOutput[expectedOutput.Length - 1]}{Environment.NewLine}";
                result += $"actual: {(int)output[output.Length - 2]}{(int)output[output.Length - 1]}{Environment.NewLine}";
            }

            if (WildcardPattern.WildCardCharacters.Any(c => expectedOutput.Contains(c)))
            {
                result += "NOTE: The expected string contains wildcard charaters: [,],?,*,#" + Environment.NewLine;
            }

            if (expectedOutput.Contains(Environment.NewLine))
            {
                result += string.Join(Environment.NewLine, "AreEqual failed:", "",
                    "Expected:", "-----------------------------------", expectedOutput, "-----------------------------------",
                    "Actual: ", "-----------------------------------", output, "-----------------------------------");
            }
            else
            {
                result += string.Join(Environment.NewLine, "AreEqual failed:",
                    "Expected: ", expectedOutput,
                    "Actual:   ", output);
            }

            int expectedOutputLength = expectedOutput.Length;
            int outputLength = output.Length;
            if (expectedOutputLength != outputLength)
            {
                result += $"{Environment.NewLine}The expected length of {expectedOutputLength} does not match the output length of {outputLength}. ";
                string[] items = (new string[] { expectedOutput, output }).OrderBy(item => item.Length).ToArray();
                if (items[1].StartsWith(items[0]))
                {
                    result += $"{Environment.NewLine}The additional characters are '"
                        + $"{CSharpStringEncode(items[1].Substring(items[0].Length))}'.";
                }
            }
            else
            {
                // Write the output that shows the difference.
                for (int counter = 0; counter < Math.Min(expectedOutput.Length, output.Length); counter++)
                {
                    if (expectedOutput[counter] != output[counter]) // TODO: The message is invalid when using wild cards.
                    {
                        result += Environment.NewLine
                            + $"Character {counter} did not match: "
                            + $"'{CSharpStringEncode(expectedOutput[counter])}' != '{CSharpStringEncode(output[counter])})'";

                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Convets text into a C# escaped string.
        /// </summary>
        /// <param name="text">The text to encode with C# escape characters.</param>
        /// <returns>The C# encoded value of <paramref name="text"/></returns>
        /// <example>
        /// <code>Console.WriteLine(CSharpStringEncode("    "));</code>
        /// Will display "\t". 
        /// </example>
        private static string CSharpStringEncode(string text)
        {
            return text;
            // TODO: Can we recreate this in .Net Core?
            //string result = "";
            //using (var stringWriter = new StringWriter())
            //{

            //    using (var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp"))
            //    {
            //        provider.GenerateCodeFromExpression(
            //            new System.CodeDom.CodePrimitiveExpression(text), stringWriter, null);
            //        result = stringWriter.ToString();
            //    }
            //}
            //return result;
        }

        private static string CSharpStringEncode(char character) =>
            CSharpStringEncode(character.ToString());

        /// <summary>
        /// This parses a "view" string into two separate strings, one
        /// representing virtual input and the other as expected output
        /// </summary>
        /// <param name="view">
        /// What a user would see in the console, but with input/output tokens.
        /// </param>
        /// <returns>[0] Input, and [1] Output</returns>
        private static string[] Parse(string view)  // TODO: Return Tuple instead.
        {
            // Note: This could definitely be optimized, wanted to try it for experience. RegEx perhaps?
            bool isInput = false;
            char[] viewTemp = view.ToCharArray();

            string input = "";
            string output = "";

            // using the char array, categorize each entry as belonging to "input" or "output"
            for (int i = 0; i < viewTemp.Length; i++)
            {
                if (i != viewTemp.Length - 1)
                {
                    // find "<<" tokens which indicate beginning of input
                    if (viewTemp[i] == '<' && viewTemp[i + 1] == '<')
                    {
                        i++;    // skip the other character in token
                        isInput = true;
                        continue;
                    }
                    // find ">>" tokens which indicate end of input
                    else if (viewTemp[i] == '>' && viewTemp[i + 1] == '>')
                    {
                        i++;    // skip the other character in token
                        isInput = false;
                        continue;
                    }
                }
                if (isInput)
                {
                    input += viewTemp[i].ToString();
                }
                else
                {
                    output += viewTemp[i].ToString();
                }
            }

            return new string[] { input, output };
        }

        // TODO: Should not use LikeOperator by default.  Add a ConsoleTestsComparisonOptions enum 
        //       with support for LikeOperator and AvoidNormalizedCRLF in addition to supporting
        //       the comparison operator.
        /// <summary>
        /// Performs a unit test on a console-based executable. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="fileName">Path to the process to be started.</param>
        /// <param name="args">Arguments string to be passed to the process.</param>
        /// <param name="workingDirectory">Working directory to start the process in.</param>
        public static Process ExecuteProcess(string expected, string fileName, string args, string workingDirectory = null)
        {
            return ExecuteProcess(expected, fileName, args, out _, out _, workingDirectory);
        }

        /// <summary>
        /// Performs a unit test on a console-based executable. A "view" of
        /// what a user would see in their console is provided as a string,
        /// where their input (including line-breaks) is surrounded by double
        /// less-than/greater-than signs, like so: "Input please: &lt;&lt;Input&gt;&gt;"
        /// </summary>
        /// <param name="expected">Expected "view" to be seen on the console,
        /// including both input and output</param>
        /// <param name="fileName">Path to the process to be started.</param>
        /// <param name="args">Arguments string to be passed to the process.</param>
        /// <param name="standardOutput">Full contents of stdout that was written by the process.</param>
        /// <param name="standardError">Full contents of stderr that was written by the process.</param>
        /// <param name="workingDirectory">Working directory to start the process in.</param>
        public static Process ExecuteProcess(string expected, string fileName, string args,
            out string standardOutput, out string standardError, string workingDirectory = null)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName, args)
            {
                //processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
            standardOutput = process.StandardOutput.ReadToEnd();
            standardError = process.StandardError.ReadToEnd();
            AssertExpectation(expected, standardOutput, (left, right) => LikeOperator(left, right));
            return process;
        }
    }
}
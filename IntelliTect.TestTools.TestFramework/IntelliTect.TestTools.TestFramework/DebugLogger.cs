﻿namespace IntelliTect.TestTools.TestFramework
{
    public class DebugLogger : ILogger
    {
        public string TestCaseKey { get; set; }
        public string CurrentTestBlock { get; set; }

        public void Debug(string message)
        {
            LogToDebug($"{TestCaseKey} - {CurrentTestBlock} - Debug: {message}");
        }

        public void Critical(string message)
        {
            LogToDebug($"{TestCaseKey} - {CurrentTestBlock} - Error: {message}");
        }

        public void Info(string message)
        {
            LogToDebug($"{TestCaseKey} - {CurrentTestBlock} - Info: {message}");
        }

        public void TestBlockInput(string input)
        {
            LogToDebug($"{TestCaseKey} - {CurrentTestBlock} - Input arguments: {input}");
        }

        public void TestBlockOutput(string output)
        {
            LogToDebug($"{TestCaseKey} - {CurrentTestBlock} - Output returns: {output}");
        }

        private void LogToDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
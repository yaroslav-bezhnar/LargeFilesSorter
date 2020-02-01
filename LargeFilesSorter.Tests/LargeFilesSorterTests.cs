using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LargeFilesSorter.Tests
{
    [TestClass]
    public class LargeFilesSorterTests
    {
        [TestMethod]
        public void AreSortedFilesEquals()
        {
            var testFile = "_fileTest.txt";
            var sortedTestFile = "_sortedFileTest.txt";
            var sortedFile = "_fileTest_Sorted.txt";

            var sorter = new LargeFileSorter( testFile );
            sorter.BeginSort();

            var text1 = File.ReadAllText( sortedTestFile );
            var text2 = File.ReadAllText( sortedFile );

            Assert.AreEqual( text1, text2 );
        }
    }
}
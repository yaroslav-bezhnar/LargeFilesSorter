using System;
using System.IO;
using FilesGenerator;

namespace LargeFilesSorter
{
    internal class Program
    {
        #region Private Methods

        private static void Main( string[] args )
        {
            string fileName = null;

            if ( args.Length == 1 && !string.IsNullOrWhiteSpace( args[0] ) )
            {
                fileName = args[0];
            }

            if ( string.IsNullOrWhiteSpace( fileName ) )
            {
                Console.WriteLine( "Select action:" );
                Console.WriteLine( "\t 1. Create a file with content for sorting." );
                Console.WriteLine( "\t 2. Use existing file for sorting." );

                switch ( Console.ReadLine() )
                {
                    case "1":
                        fileName = LargeFileGenerator.CreateLargeFile();
                        break;
                    case "2":
                        Console.WriteLine( "Enter path to file:" );
                        fileName = Console.ReadLine();
                        break;
                    default:
                        Console.WriteLine( "Entered incorrect number." );
                        Console.ReadKey();
                        return;
                }
            }

            if ( !string.IsNullOrWhiteSpace( fileName ) && File.Exists( fileName ) )
            {
                var fileSorter = new LargeFileSorter( fileName );
                fileSorter.BeginSort();
            }
            else
            {
                Console.WriteLine( $"File '{fileName}' not found." );
            }

            Console.ReadKey();
        }

        #endregion
    }
}
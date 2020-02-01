using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FilesGenerator
{
    /// <summary>
    ///     Represents a class to create large file with data.
    /// </summary>
    public static class LargeFileGenerator
    {
        #region Constants

        private const string CHARACTERS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int MEGABYTE = 1024 * 1024; // 1 MB
        private static readonly Random _random = new Random();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Create large file.
        /// </summary>
        /// <returns>Created file name.</returns>
        public static string CreateLargeFile()
        {
            var fileName = readFileName();
            if ( fileName != null )
            {
                var fileSizeInMegabytes = readFileSizeInMegabytes();
                if ( fileSizeInMegabytes.HasValue )
                {
                    var fileSizeInBytes = fileSizeInMegabytes.Value * MEGABYTE;
                    return createFile( fileName, fileSizeInBytes );
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Read file name from console.
        /// </summary>
        /// <returns>File name.</returns>
        private static string readFileName()
        {
            Console.WriteLine( "Enter the file name to be created:" );
            var fileName = Console.ReadLine();
            if ( isFileNameCorrected( fileName ) )
            {
                return fileName;
            }

            Console.WriteLine( "The entered file name is invalid." );
            Console.WriteLine( "Do you want to enter another file name? (y/n)" );

            return Console.ReadLine()?.ToLower().Equals( "y" ) == true ? readFileName() : null;
        }

        /// <summary>
        ///     Read file size from console.
        /// </summary>
        /// <returns>File size.</returns>
        private static long? readFileSizeInMegabytes()
        {
            Console.WriteLine( "Enter the file size in megabytes:" );
            var stringSize = Console.ReadLine();
            if ( long.TryParse( stringSize, out var size ) )
            {
                return size;
            }

            Console.WriteLine( "The entered file size is invalid." );
            Console.WriteLine( "Do you want to enter another file size? (y/n)" );

            return Console.ReadLine()?.ToLower().Equals( "y" ) == true ? readFileSizeInMegabytes() : null;
        }

        /// <summary>
        ///     Gets random word.
        /// </summary>
        /// <param name="wordLength">Length of word.</param>
        /// <returns>Random word.</returns>
        private static string getRandomWord( int wordLength )
        {
            return new string( Enumerable.Repeat( CHARACTERS, wordLength )
                                         .Select( s => s[_random.Next( s.Length )] ).ToArray() );
        }

        /// <summary>
        ///     Check if file name is corrected.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>'True' if file name is corrected; otherwise 'False'.</returns>
        private static bool isFileNameCorrected( string fileName )
        {
            if ( string.IsNullOrWhiteSpace( fileName ) )
            {
                return false;
            }

            return !fileName.Any( f => Path.GetInvalidFileNameChars().Contains( f ) );
        }

        /// <summary>
        ///     Create file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="fileSize">File size.</param>
        private static string createFile( string fileName, long fileSize )
        {
            try
            {
                using ( var fileStream = new FileStream( fileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
                {
                    while ( fileStream.Length < fileSize )
                    {
                        var bytes = Encoding.UTF8.GetBytes( getRandomWord( _random.Next( 5, 10 ) ) );
                        fileStream.Write( bytes, 0, bytes.Length );

                        var newline = Encoding.UTF8.GetBytes( Environment.NewLine );
                        fileStream.Write( newline, 0, newline.Length );
                    }
                }

                Console.WriteLine( $"File '{fileName}' successfully created." );

                return fileName;
            }
            catch ( IOException ex )
            {
                Console.WriteLine( $"Cannot create file '{fileName}'. I/O Error: {ex.Message}" );
            }
            catch ( SystemException ex )
            {
                Console.WriteLine( $"Cannot create file '{fileName}'. System Error: {ex.Message}" );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Cannot create file '{fileName}'. Error: {ex.Message}" );
            }

            return null;
        }

        #endregion
    }
}
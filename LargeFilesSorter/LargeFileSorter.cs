using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LargeFilesSorter
{
    /// <summary>
    ///     Represents a class to sort large file.
    /// </summary>
    public sealed class LargeFileSorter
    {
        #region Constants

        private const long MAX_FILE_SIZE = 100 * 1024 * 1024; // 100 MB

        #endregion

        #region Constructors

        /// <summary>
        ///     Creates a new instance of the <see cref="LargeFileSorter" /> class.
        /// </summary>
        /// <param name="fileName">File to sort.</param>
        public LargeFileSorter( string fileName ) => FileName = fileName;

        #endregion

        #region Properties

        /// <summary>
        ///     File name.
        /// </summary>
        public string FileName { get; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Start sorting.
        /// </summary>
        public void BeginSort()
        {
            if ( !string.IsNullOrWhiteSpace( FileName ) && File.Exists( FileName ) )
            {
                Console.WriteLine( "Sorting started." );

                if ( splitFile() && sortChunks() && mergeChunks() )
                {
                    Console.WriteLine( "Sorting complete successfully." );
                }
            }
            else
            {
                Console.WriteLine( $"Cannot start sorting. File '{FileName}' not found." );
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Split the file into smaller chunks.
        /// </summary>
        /// <returns></returns>
        private bool splitFile()
        {
            Console.WriteLine( $"Splitting '{FileName}' file." );

            try
            {
                var splitNumber = 1;
                var streamWriter = new StreamWriter( $"splitted{splitNumber:D3}.tmp" );
                using ( var streamReader = new StreamReader( FileName ) )
                {
                    while ( streamReader.Peek() >= 0 )
                    {
                        streamWriter.WriteLine( streamReader.ReadLine() );

                        // Create new split file if size is 100 MB
                        if ( streamWriter.BaseStream.Length > MAX_FILE_SIZE && streamReader.Peek() >= 0 )
                        {
                            streamWriter.Close();
                            splitNumber++;
                            streamWriter = new StreamWriter( $"splitted{splitNumber:D3}.tmp" );
                        }
                    }
                }

                streamWriter.Close();

                Console.WriteLine( "Splitting completed successfully." );
                return true;
            }
            catch ( IOException ex )
            {
                Console.WriteLine( $"Cannot split file '{FileName}'. I/O Error: {ex.Message}" );
            }
            catch ( SystemException ex )
            {
                Console.WriteLine( $"Cannot split file '{FileName}'. System Error: {ex.Message}" );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Cannot split file '{FileName}'. Error: {ex.Message}" );
            }

            return false;
        }

        /// <summary>
        ///     Sort chunks of the file.
        /// </summary>
        /// <returns></returns>
        private bool sortChunks()
        {
            Console.WriteLine( $"Sorting chunks of '{FileName}' file." );

            try
            {
                foreach ( var file in Directory.GetFiles( Environment.CurrentDirectory, "splitted*.tmp" ) )
                {
                    var lines = File.ReadAllLines( file );
                    Array.Sort( lines );
                    var path = file.Replace( "splitted", "sorted" );
                    File.WriteAllLines( path, lines );
                    File.Delete( file );

                    // release array from memory
                    lines = null;
                    GC.Collect();
                }

                Console.WriteLine( "Sorting chunks completed successfully." );
                return true;
            }
            catch ( IOException ex )
            {
                Console.WriteLine( $"Cannot sort chunks of file '{FileName}'. I/O Error: {ex.Message}" );
            }
            catch ( SystemException ex )
            {
                Console.WriteLine( $"Cannot sort chunks of file '{FileName}'. System Error: {ex.Message}" );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Cannot sort chunks of file '{FileName}'. Error: {ex.Message}" );
            }

            return false;
        }

        /// <summary>
        ///     Merge chunks of the file.
        /// </summary>
        /// <returns></returns>
        private bool mergeChunks()
        {
            Console.WriteLine( $"Merging chunks of '{FileName}' file together." );

            try
            {
                var paths = Directory.GetFiles( Environment.CurrentDirectory, "sorted*.tmp" );
                var chunks = paths.Length;
                var recordSize = 100;
                var maxUsage = 500 * 1024 * 1024;
                var bufferSize = maxUsage / chunks;
                var recordOverhead = 7.5;
                var bufferLength = (int) ( bufferSize / recordSize / recordOverhead );

                // Open files
                var readers = new StreamReader[chunks];
                for ( var i = 0; i < chunks; i++ )
                {
                    readers[i] = new StreamReader( paths[i] );
                }

                // Create queues
                var queues = new Queue<string>[chunks];
                for ( var i = 0; i < chunks; i++ )
                {
                    queues[i] = new Queue<string>( bufferLength );
                }

                // Load queues
                for ( var i = 0; i < chunks; i++ )
                {
                    loadQueue( queues[i], readers[i], bufferLength );
                }

                // Merging
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension( FileName );
                var resultFile = FileName.Replace( fileNameWithoutExtension, $"{fileNameWithoutExtension}_Sorted" );
                var streamWriter = new StreamWriter( resultFile );
                while ( true )
                {
                    // Find lowest value in chunk
                    var lowestIndex = -1;
                    var lowestValue = string.Empty;

                    for ( var i = 0; i < chunks; i++ )
                    {
                        if ( queues[i] != null )
                        {
                            if ( lowestIndex < 0 ||
                                 string.Compare( queues[i].Peek(), lowestValue, StringComparison.Ordinal ) < 0 )
                            {
                                lowestIndex = i;
                                lowestValue = queues[i].Peek();
                            }
                        }
                    }

                    if ( lowestIndex < 0 )
                    {
                        break;
                    }

                    // Write to file
                    streamWriter.WriteLine( lowestValue );

                    // Remove from queue
                    queues[lowestIndex].Dequeue();

                    // Lift it up if queue empty
                    if ( !queues[lowestIndex].Any() )
                    {
                        loadQueue( queues[lowestIndex], readers[lowestIndex], bufferLength );

                        if ( !queues[lowestIndex].Any() )
                        {
                            queues[lowestIndex] = null;
                        }
                    }
                }

                streamWriter.Close();

                // Close and delete the files
                for ( var i = 0; i < chunks; i++ )
                {
                    readers[i].Close();
                    File.Delete( paths[i] );
                }

                Console.WriteLine( "Merging completed successfully." );
                return true;
            }
            catch ( IOException ex )
            {
                Console.WriteLine( $"Cannot merge chunks of file '{FileName}'. I/O Error: {ex.Message}" );
            }
            catch ( SystemException ex )
            {
                Console.WriteLine( $"Cannot merge chunks of file '{FileName}'. System Error: {ex.Message}" );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( $"Cannot merge chunks of file '{FileName}'. Error: {ex.Message}" );
            }

            return false;
        }

        /// <summary>
        ///     Load queue.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="streamReader"></param>
        /// <param name="records"></param>
        private static void loadQueue( Queue<string> queue, StreamReader streamReader, int records )
        {
            for ( var i = 0; i < records; i++ )
            {
                if ( streamReader.Peek() < 0 )
                {
                    break;
                }

                queue.Enqueue( streamReader.ReadLine() );
            }
        }

        #endregion
    }
}
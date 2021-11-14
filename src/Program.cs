using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

/*---------------------------------------------------------------------------*/
//  Bit Stream Generator - bsgen_tmct
//  ©2021 tmct All rights reserved.
/*---------------------------------------------------------------------------*/
//  (Description)
//    This software is used to convert binary files to include file format 
//    such as c language.
//    It can also perform run-length encoding at the same time during the 
//    conversion.
//
//  (License)
//    ©2021, tmct All rights reserved.
//    Redistribution and use in source and binary forms, with or without
//    modification, are permitted provided that the following conditions are met:
//
//    1. Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer. 
//    2. Redistributions in binary form must reproduce the above copyright notice,
//       this list of conditions and the following disclaimer in the documentation
//       and/or other materials provided with the distribution.
//
//    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//    ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//    WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//    ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//    (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//    LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//    ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//    (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//    SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//    The views and conclusions contained in the software and documentation are those
//    of the authors and should not be interpreted as representing official policies, 
//    either expressed or implied, of the FreeBSD Project.
/*---------------------------------------------------------------------------*/

namespace bsgen
{

    public enum EndianDefinition : int
    {
        Big = 0,
        Little
    }

    public enum ByteWidthDefinition : int
    {
        Byte1 = 0,
        Byte2,
        Byte4
    }

    public enum FileFormatDefinition : int
    {
        Binary = 0,
        Include
    }

    public enum CompressTypeDefinition : int
    {
        NoCompression = 0,
        RunLengthCompression,
        RunLengthDeCompression
    }

    static class Program
    {
        
        const int returnValue_NoError = 0x0000;
        const int returnValue_OptionInvalid = 0x0001;
        const int returnValue_ArgumentInvalid = 0x0002;
        const int returnValue_SourceEndOfStream = 0x0010;
        const int returnValue_SourceBytesNotEnough = 0x0011;
        const int returnValue_SourceFatalError = 0x0012;
        const int returnValue_DestFatalError = 0x0021;
        const int returnValue_Unknown = 0xffff;

        const string message_error = "[ERROR] ";
        const string message_warning = "[WARNING] ";
        const string message_information = "[INFO] ";

        const string optionName_SourceEndian = "ie";
        const string optionName_DestEndian = "oe";
        const string optionName_SourceByteWidth = "iw";
        const string optionName_SourceFilePath = "src";
        const string optionName_DestFilePath = "dst";
        const string optionName_DestFileFormat = "df";
        const string optionName_CompressType = "enc";
        const string option_BigEndian = "b";
        const string option_LittleEndian = "l";
        const string option_Byte = "b";
        const string option_Word = "w";
        const string option_Long = "l";
        const string option_Binary = "bin";
        const string option_Include = "inc";
        const string option_Raw = "raw";
        const string option_Rlc = "rle";
        const string option_Rld = "rld";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            List<string> optionHeadChar = new List<string>(){"-","--","/"};
            string sourceFilePath = "";
            string destFilePath = "";
            EndianDefinition sourceEndian = EndianDefinition.Big;
            EndianDefinition destEndian = EndianDefinition.Big;
            ByteWidthDefinition dataWidth = ByteWidthDefinition.Byte1;
            CompressTypeDefinition compressType = CompressTypeDefinition.NoCompression;
            FileFormatDefinition destFormat = FileFormatDefinition.Include;


            bool optionInvalid = false;
            bool argumentInvalid = false;
            int returnValue = returnValue_Unknown;

            /*---------------------------------------------------------------*/
            //  Show startup message
            //  DO NOT REMOVE OR EDIT THE COPYRIGHT NOTICE AND URLs.
            /*---------------------------------------------------------------*/
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Console.Write(assembly.GetCustomAttribute<AssemblyProductAttribute>().Product);
                Console.Write(" - Version ");
                Console.WriteLine(assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
                Console.Write(assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);
                Console.WriteLine(" All rights reserved.");
                Console.WriteLine("https://ss1.xrea.com/tmct.s1009.xrea.com/");
                Console.WriteLine(' ');
            }

            /*---------------------------------------------------------------*/
            //  Expand command line options
            /*---------------------------------------------------------------*/
            {
                string currentOption = "";
                foreach (string s in args)
                {
                    if (currentOption == "")
                    {
                        // When the option name is undefined
                        // Determine if s is an option name
                        bool isOption = false;
                        foreach (string o in optionHeadChar)
                        {
                            if (s.IndexOf(o) == 0) 
                            {
                                isOption = true;
                                currentOption = s.Replace(o, "").ToLower();
                                break;
                            }
                        }
                        // If it is not an option name, the option string is invalid 
                        if (isOption == false)
                        {
                            optionInvalid = true;
                            break;
                        }
                    }
                    else
                    {
                        // When the option name is defined
                        // Interpret the content according to the option type
                        string sl = s.ToLower();
                        switch (currentOption)
                        {
                            case optionName_SourceEndian:
                                // Specify the endian of the source file 
                                if (sl == option_BigEndian)
                                {
                                    sourceEndian = EndianDefinition.Big;
                                }
                                else if (sl == option_LittleEndian)
                                {
                                    sourceEndian = EndianDefinition.Little;
                                }
                                else
                                {
                                    optionInvalid = true;
                                }
                                break;
                            case optionName_DestEndian:
                                // Specify the endian of the destination file 
                                if (sl == option_BigEndian)
                                {
                                    destEndian = EndianDefinition.Big;
                                }
                                else if (sl == option_LittleEndian)
                                {
                                    destEndian = EndianDefinition.Little;
                                }
                                else
                                {
                                    optionInvalid = true;
                                }
                                break;
                            case optionName_SourceByteWidth:
                                // Specify the byte width of the source/destination file 
                                if (sl == option_Byte)
                                {
                                    dataWidth = ByteWidthDefinition.Byte1;
                                }
                                else if (sl == option_Word)
                                {
                                    dataWidth = ByteWidthDefinition.Byte2;
                                }
                                else if (sl == option_Long)
                                {
                                    dataWidth = ByteWidthDefinition.Byte4;
                                }
                                else
                                {
                                    optionInvalid = true;
                                }
                                break;
                            case optionName_SourceFilePath:
                                // Specify the source file path 
                                sourceFilePath = s;
                                break;
                            case optionName_DestFilePath:
                                // Specify the destination file path 
                                destFilePath = s;
                                break;
                            case optionName_DestFileFormat:
                                // Specify the destination file format
                                if (sl == option_Binary)
                                {
                                    destFormat = FileFormatDefinition.Binary;
                                }
                                else if (sl == option_Include)
                                {
                                    destFormat = FileFormatDefinition.Include;
                                }
                                else
                                {
                                    optionInvalid = true;
                                }
                                break;
                            case optionName_CompressType:
                                // Specify compression type
                                if (sl == option_Raw)
                                {
                                    compressType = CompressTypeDefinition.NoCompression;
                                }
                                else if (sl == option_Rlc)
                                {
                                    compressType = CompressTypeDefinition.RunLengthCompression;
                                }
                                else if (sl == option_Rld)
                                {
                                    compressType = CompressTypeDefinition.RunLengthDeCompression;
                                }
                                else
                                {
                                    optionInvalid = true;
                                }
                                break;
                            default:
                                // Option name unknown
                                optionInvalid = true;
                                break;
                        }
                        if (optionInvalid == true) break;
                        currentOption = "";
                    }
                }
            }

            /*---------------------------------------------------------------*/
            //  Validate arguments
            /*---------------------------------------------------------------*/
            {
                argumentInvalid = false;
                // Existence of source file
                Console.Write("Source file: ");
                Console.WriteLine(sourceFilePath);
                if (sourceFilePath == "")
                {
                    Console.Write(message_error);
                    Console.WriteLine("No source file is specified.");
                    argumentInvalid = true;
                }
                else if (File.Exists(sourceFilePath) == false)
                {
                    Console.Write(message_error);
                    Console.WriteLine("The source file does not exist.");
                    argumentInvalid = true;
                }
                // Existence of dest file
                Console.Write("Destination file: ");
                Console.WriteLine(destFilePath);
                if (destFilePath == "")
                {
                    Console.Write(message_error);
                    Console.WriteLine("No destination file is specified.");
                    argumentInvalid = true;
                }
                else if (File.Exists(destFilePath))
                {
                    Console.Write(message_warning);
                    Console.WriteLine("This file already exists and will be overwritten.");
                }
                // Data width
                Console.Write("Data byte width: ");
                if (dataWidth == ByteWidthDefinition.Byte1)
                {
                    Console.WriteLine("1 (Single byte)");
                }
                else if (dataWidth == ByteWidthDefinition.Byte2)
                {
                    Console.WriteLine("2 (Word)");
                    Console.Write(" - Source endian: ");
                    Console.WriteLine(sourceEndian.ToString());
                    Console.Write(" - Destination endian: ");
                    Console.WriteLine(destEndian.ToString());
                }
                else
                {
                    Console.WriteLine("4 (Long)");
                    Console.Write(" - Source endian: ");
                    Console.WriteLine(sourceEndian.ToString());
                    Console.Write(" - Destination endian: ");
                    Console.WriteLine(destEndian.ToString());
                }
                // Destination format
                Console.Write(" - Destination format: ");
                Console.WriteLine(destFormat.ToString());
                // Compression type
                Console.Write("Compression type: ");
                if (compressType == CompressTypeDefinition.NoCompression)
                {
                    Console.WriteLine("No encode");
                }
                else if (compressType == CompressTypeDefinition.RunLengthCompression)
                {
                    Console.WriteLine("Run-length encode");
                }
                else
                {
                    Console.WriteLine("Run-length decode");
                }

            }

            /*---------------------------------------------------------------*/
            //  Execute processing
            /*---------------------------------------------------------------*/
            if (optionInvalid)
            {
                Console.Write(message_error);
                Console.WriteLine("The specified option is invalid.");
                showHelpMessage();
                returnValue = returnValue_OptionInvalid;
            }
            else if (argumentInvalid)
            {
                Console.Write(message_error);
                Console.WriteLine("The specified argument is invalid.");
                showHelpMessage();
                returnValue = returnValue_ArgumentInvalid;
            }
            else
            {
                List<uint> sourceDataArray = null;
                List<uint> destDataArray = null;
                int r = returnValue_NoError;
                
                // Read
                r = ReadSource(sourceFilePath, dataWidth, sourceEndian, out sourceDataArray);
                if (r == returnValue_SourceFatalError) returnValue = r; else returnValue = returnValue_NoError;

                // Compress
                if (returnValue == returnValue_NoError)
                {
                    switch (compressType)
                    {
                        case CompressTypeDefinition.RunLengthCompression:
                            EncodeRL(in sourceDataArray, out destDataArray, dataWidth);
                            break;
                        case CompressTypeDefinition.RunLengthDeCompression:
                            DecodeRL(in sourceDataArray, out destDataArray);
                            break;
                        default:
                            Console.Write(message_information);
                            Console.WriteLine("No data will be encoded.");
                            destDataArray = sourceDataArray;
                            break;
                    }
                }
                if (r == returnValue_SourceFatalError) returnValue = r; else returnValue = returnValue_NoError;

                // Write
                if (returnValue == returnValue_NoError)
                {
                    if (destFormat == FileFormatDefinition.Binary)
                    {
                        returnValue = WriteDestinationBinary(destFilePath,dataWidth,destEndian, ref destDataArray);
                    }
                    else
                    {
                        returnValue = WriteDestinationInclude(destFilePath,dataWidth,destEndian, ref destDataArray);
                    }
                }
            }
            
            return returnValue;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void showHelpMessage()
        {
            Console.WriteLine("");
            Console.WriteLine("bsgen -<option> <parameter> -<option> <parameter> ...");
            Console.WriteLine("Options:");
            Console.Write(" -"); Console.Write(optionName_SourceFilePath); Console.WriteLine(" <Source file path>");
            Console.WriteLine("   Specify the path of the source file. This parameter cannot be omitted.");
            Console.Write(" -"); Console.Write(optionName_DestFilePath); Console.WriteLine(" <Destination file path>");
            Console.WriteLine("   Specify the path of the destination file.　This parameter cannot be omitted.");
            Console.Write(" -"); Console.Write(optionName_SourceByteWidth); Console.WriteLine(" <Byte width of the source file>");
            Console.WriteLine("   Specifies the byte width of the source file:");
            Console.WriteLine("    b ... Single byte (*Default)");
            Console.WriteLine("    w ... Double(2) bytes");
            Console.WriteLine("    l ... Quad(4) bytes");
            Console.WriteLine("     *The destination file will also be output with the same byte width.");
            Console.Write(" -"); Console.Write(optionName_SourceEndian); Console.WriteLine(" <Endian of the source file>");
            Console.WriteLine("   Specifies the endian of the source file:");
            Console.WriteLine("    b ... Big endian (*Default)");
            Console.WriteLine("    l ... Little endian");
            Console.WriteLine("     *If the byte width is specified as single byte, this setting will be ignored.");
            Console.Write(" -"); Console.Write(optionName_CompressType); Console.WriteLine(" <Compression type>");
            Console.WriteLine("   Specifies the processing type of the destination file:");
            Console.WriteLine("    raw ... Through (*Default)");
            Console.WriteLine("    rle ... Run-length encoding");
            Console.WriteLine("    rld ... Run-length decoding");
            Console.Write(" -"); Console.Write(optionName_DestEndian); Console.WriteLine(" <Endian of the destination file>");
            Console.WriteLine("   Specifies the endian of the destination file:");
            Console.WriteLine("    b ... Big endian (*Default)");
            Console.WriteLine("    l ... Little endian");
            Console.WriteLine("     *If the byte width is specified as single byte, this setting will be ignored.");
            Console.Write(" -"); Console.Write(optionName_DestFileFormat); Console.WriteLine(" <Format of the destination file>");
            Console.WriteLine("   Specifies the file format of the destination file:");
            Console.WriteLine("    inc ... Includeable file (*Default)");
            Console.WriteLine("    bin ... Binary file");
            Console.WriteLine("     *The destination file format is affected by the byte width setting of the source file.");

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcFilePath"></param>
        /// <param name="bytWidth"></param>
        /// <param name="iendian"></param>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        private static int ReadSource(string srcFilePath, ByteWidthDefinition bytWidth, EndianDefinition iendian, out List<uint> dataArray)
        {
            int returnValue = returnValue_NoError;
            FileStream sourceStream = null;
            BinaryReader binaryReader = null;
            dataArray = new List<uint>();
            dataArray.Clear();

            try
            {
                /*-----------------------------------------------------------*/
                //  Open the binary file stream
                /*-----------------------------------------------------------*/
                sourceStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read);
                binaryReader = new BinaryReader(sourceStream);
                Console.Write(message_information);
                Console.WriteLine("The source file has been opened.");

                /*-----------------------------------------------------------*/
                //  Read loop
                /*-----------------------------------------------------------*/
                int r = returnValue_NoError;
                uint data = 0;
                while (r == returnValue_NoError)
                {
                    /*-------------------------------------------------------*/
                    //  Read one data
                    /*-------------------------------------------------------*/
                    switch (bytWidth)
                    {
                        case ByteWidthDefinition.Byte1:
                            // Read one byte
                            byte[] b = binaryReader.ReadBytes(1);
                            if (b.Length == 0)
                            {
                                // Reached the end of the stream
                                r = returnValue_SourceEndOfStream;
                            }
                            else
                            {
                                data = (uint)b[0];
                            }
                            break;
                        case ByteWidthDefinition.Byte2:
                            // Read 2 bytes as word
                            byte[] w = binaryReader.ReadBytes(2);
                            if (w.Length == 0)
                            {
                                // Reached the end of the stream
                                r = returnValue_SourceEndOfStream;
                            }
                            else if (w.Length < 2)
                            {
                                // Not enough bytes of data
                                r = returnValue_SourceBytesNotEnough;
                                Console.Write(message_warning);
                                Console.WriteLine("The number of bytes of data in the source file is insufficient.");
                                Console.Write(message_warning);
                                Console.Write("The last ");
                                Console.Write(w.Length.ToString());
                                Console.WriteLine(" byte(s) of the source file will not be read.");
                            }
                            else
                            {
                                if (iendian == EndianDefinition.Big)
                                {
                                    data = (uint)(w[0] << 8) | (uint)w[1];
                                }
                                else
                                {
                                    data = (uint)(w[1] << 8) | (uint)w[0];
                                }
                            }
                            break;
                        default:
                            // Read 4 bytes as long
                            byte[] l = binaryReader.ReadBytes(4);
                            if (l.Length == 0)
                            {
                                // Reached the end of the stream
                                r = returnValue_SourceEndOfStream;
                            }
                            else if (l.Length < 4)
                            {
                                // Not enough bytes of data
                                r = returnValue_SourceBytesNotEnough;
                                Console.Write(message_warning);
                                Console.WriteLine("The number of bytes of data in the source file is insufficient.");
                                Console.Write(message_warning);
                                Console.Write("The last ");
                                Console.Write(l.Length.ToString());
                                Console.WriteLine(" byte(s) of the source file will not be read.");
                            }
                            else
                            {
                                if (iendian == EndianDefinition.Big)
                                {
                                    data = (uint)(l[0] << 24) | (uint)(l[1] << 16) | (uint)(l[2] << 8) |(uint)l[3];
                                }
                                else
                                {
                                    data = (uint)(l[3] << 24) | (uint)(l[2] << 16) | (uint)(l[1] << 8) |(uint)l[0];
                                }
                            }
                            break;
                    }

                    /*-------------------------------------------------------*/
                    //  Add to dataArray
                    /*-------------------------------------------------------*/
                    if (r == returnValue_NoError)
                    {
                        dataArray.Add(data);
                    }
                    else
                    {
                        returnValue = r;
                    }
                }
                Console.Write(message_information);
                Console.Write(dataArray.Count.ToString());
                Console.WriteLine(" piece(s) of data have been read.");
            }
            catch (System.Exception e)
            {
                Console.Write(message_error);
                Console.WriteLine("A fatal error has occurred.");
                Console.WriteLine(e.Message);
                returnValue = returnValue_SourceFatalError;
            }
            finally
            {
                if (binaryReader != null)
                {
                    binaryReader.Close();
                    binaryReader.Dispose();
                }
                if (sourceStream != null)
                {
                    sourceStream.Close();
                    sourceStream.Dispose();
                }
            }
            return returnValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dstFilePath"></param>
        /// <param name="bytWidth"></param>
        /// <param name="oendian"></param>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        private static int WriteDestinationBinary(string dstFilePath, ByteWidthDefinition bytWidth, EndianDefinition oendian, ref List<uint> dataArray)
        {
            int returnValue = returnValue_NoError;
            FileStream destStream = null;
            BinaryWriter binaryWriter = null;

            try
            {
                /*-----------------------------------------------------------*/
                //  Open the binary file stream
                /*-----------------------------------------------------------*/
                destStream = new FileStream(dstFilePath, FileMode.Create, FileAccess.Write);
                binaryWriter = new BinaryWriter(destStream);
                Console.Write(message_information);
                Console.WriteLine("The destination file has been opened.");

                /*-----------------------------------------------------------*/
                //  Write to the destination file
                /*-----------------------------------------------------------*/
                foreach (uint data in dataArray)
                {
                    byte b = 0;
                    switch (bytWidth)
                    {
                        case ByteWidthDefinition.Byte1:
                            // Write one byte
                            binaryWriter.Write((byte)data);
                            break;
                        case ByteWidthDefinition.Byte2:
                            // Read 2 bytes as word
                            if (oendian == EndianDefinition.Big)
                            {
                                b = (byte)((data >> 8) & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)(data & 0xff);
                                binaryWriter.Write(b);
                            }
                            else
                            {
                                b = (byte)(data & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)((data >> 8) & 0xff);
                                binaryWriter.Write(b);
                            }
                            break;
                        default:
                            // Read 4 bytes as long
                            if (oendian == EndianDefinition.Big)
                            {
                                b = (byte)((data >> 24) & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)((data >> 16) & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)((data >> 8) & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)(data & 0xff);
                                binaryWriter.Write(b);
                            }
                            else
                            {
                                b = (byte)(data & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)((data >> 8) & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)((data >> 16) & 0xff);
                                binaryWriter.Write(b);
                                b = (byte)((data >> 24) & 0xff);
                                binaryWriter.Write(b);
                            }
                            break;
                    }
                }
                Console.Write(message_information);
                Console.Write(dataArray.Count.ToString());
                Console.WriteLine(" piece(s) of data have been written.");
            }
            catch (System.Exception e)
            {
                Console.Write(message_error);
                Console.WriteLine("A fatal error has occurred.");
                Console.WriteLine(e.Message);
                returnValue = returnValue_DestFatalError;
            }
            finally
            {
                if (binaryWriter != null)
                {
                    binaryWriter.Flush();
                    binaryWriter.Close();
                    binaryWriter.Dispose();
                }
                if (destStream != null)
                {
                    destStream.Close();
                    destStream.Dispose();
                }
            }
            return returnValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dstFilePath"></param>
        /// <param name="bytWidth"></param>
        /// <param name="oendian"></param>
        /// <param name="dataArray"></param>
        /// <returns></returns>
        private static int WriteDestinationInclude(string dstFilePath, ByteWidthDefinition bytWidth, EndianDefinition oendian, ref List<uint> dataArray)
        {
            int returnValue = returnValue_NoError;
            FileStream destStream = null;
            StreamWriter streamWriter = null;

            try
            {
                /*-----------------------------------------------------------*/
                //  Open the binary file stream
                /*-----------------------------------------------------------*/
                destStream = new FileStream(dstFilePath, FileMode.Create, FileAccess.Write);
                streamWriter = new StreamWriter(destStream);
                Console.Write(message_information);
                Console.WriteLine("The destination file has been opened.");

                /*-----------------------------------------------------------*/
                //  Write to the destination file
                /*-----------------------------------------------------------*/
                int x = 1;
                int xLimit = 0;
                ulong byteCount = 0;
                switch (bytWidth)
                {
                    case ByteWidthDefinition.Byte1:
                        xLimit = 15;
                        break;
                    case ByteWidthDefinition.Byte2:
                        xLimit = 11;
                        break;
                    default:
                        xLimit = 7;
                        break;
                }

                foreach (uint data in dataArray)
                {
                    /*-------------------------------------------------------*/
                    //  Add a comma
                    /*-------------------------------------------------------*/
                    if (x > xLimit)
                    {
                        streamWriter.Write(',');
                        streamWriter.Write(Environment.NewLine);
                        x = 1;
                    }
                    else
                    {
                        if (byteCount != 0)
                        {
                            streamWriter.Write(',');
                            x++;
                        }
                    }

                    /*-------------------------------------------------------*/
                    //  Add data
                    /*-------------------------------------------------------*/
                    byte b = 0;
                    streamWriter.Write("0x");
                    switch (bytWidth)
                    {
                        case ByteWidthDefinition.Byte1:
                            // Write one byte
                            streamWriter.Write(((byte)data).ToString("x2"));
                            break;
                        case ByteWidthDefinition.Byte2:
                            // Read 2 bytes as word
                            if (oendian == EndianDefinition.Big)
                            {
                                b = (byte)((data >> 8) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)(data & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                            }
                            else
                            {
                                b = (byte)(data & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)((data >> 8) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                            }
                            break;
                        default:
                            // Read 4 bytes as long
                            if (oendian == EndianDefinition.Big)
                            {
                                b = (byte)((data >> 24) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)((data >> 16) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)((data >> 8) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)(data & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                            }
                            else
                            {
                                b = (byte)(data & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)((data >> 8) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)((data >> 16) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                                b = (byte)((data >> 24) & 0xff);
                                streamWriter.Write(b.ToString("x2"));
                            }
                            break;
                    }
                    byteCount++;
                }
                Console.Write(message_information);
                Console.Write(dataArray.Count.ToString());
                Console.WriteLine(" piece(s) of data have been written.");
            }
            catch (System.Exception e)
            {
                Console.Write(message_error);
                Console.WriteLine("A fatal error has occurred.");
                Console.WriteLine(e.Message);
                returnValue = returnValue_DestFatalError;
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Flush();
                    streamWriter.Close();
                    streamWriter.Dispose();
                }
                if (destStream != null)
                {
                    destStream.Close();
                    destStream.Dispose();
                }
            }
            return returnValue;
        }


        /// <summary>
        /// Run-length encoding
        /// </summary>
        /// <param name="sourceDataArray"></param>
        /// <param name="destDataArray"></param>
        /// <param name="bytWidth"></param>
        private static void EncodeRL(in List<uint> sourceDataArray, out List<uint> destDataArray, ByteWidthDefinition bytWidth)
        {
            destDataArray = new List<uint>();
            destDataArray.Clear();
            ulong dataCount = 0;
            uint runData = 0;
            uint runLength = 0;
            uint maxRunLength = 0;

            Console.Write(message_information);
            Console.WriteLine("Begin Run-length encoding.");

            if (bytWidth == ByteWidthDefinition.Byte1)
            {
                maxRunLength = 0xff;
            }
            else if (bytWidth == ByteWidthDefinition.Byte2)
            {
                maxRunLength = 0xffff;
            }
            else
            {
                maxRunLength = 0xffffffff;
            }

            foreach (uint s in sourceDataArray)
            {
                if ((dataCount == 0) || (runLength == 0))
                {
                    runData = s;
                    runLength = 1;
                }
                else if (runData == s)
                {
                    runLength++;
                }
                if ((runData != s) || (runLength == maxRunLength))
                {
                    destDataArray.Add(runLength);
                    destDataArray.Add(runData);
                    runData = s;
                    if (runLength == maxRunLength) runLength = 0; else runLength = 1;
                }
                dataCount++;
            }
            destDataArray.Add(runLength);
            destDataArray.Add(runData);

            Console.Write(message_information);
            Console.Write(dataCount.ToString());
            Console.WriteLine(" piece(s) of data have been encoded.");
            Console.Write(message_information);
            Console.Write("The data has been encoded into ");
            Console.Write(destDataArray.Count.ToString());
            Console.WriteLine(" piece(s).");
            if (sourceDataArray.Count < destDataArray.Count)
            {
                Console.Write(message_warning);
                Console.WriteLine("Encoding has increased the number of pieces of data.");
            }
        }


        /// <summary>
        /// Run-length decoding
        /// </summary>
        /// <param name="sourceDataArray"></param>
        /// <param name="destDataArray"></param>
        private static void DecodeRL(in List<uint> sourceDataArray, out List<uint> destDataArray)
        {
            destDataArray = new List<uint>();
            destDataArray.Clear();
            uint runData = 0;
            uint runLength = 0;
            int dataPointer = 0;

            Console.Write(message_information);
            Console.WriteLine("Begin Run-length decoding.");

            while (sourceDataArray.Count > dataPointer)
            {
                runLength = sourceDataArray[dataPointer];
                dataPointer++;
                if (sourceDataArray.Count == dataPointer)
                {
                    Console.Write(message_warning);
                    Console.WriteLine("The number of bytes of data in the source file is insufficient.");
                    Console.Write(message_warning);
                    Console.WriteLine("The last data will be missing.");
                }
                else
                {
                    runData = sourceDataArray[dataPointer];
                    dataPointer++;
                    for (uint i = 0; i < runLength; i++)
                    {
                        destDataArray.Add(runData);
                    }
                }
            }
            Console.Write(message_information);
            Console.Write(destDataArray.Count.ToString());
            Console.WriteLine(" piece(s) of data have been decoded.");
        }


    }
}

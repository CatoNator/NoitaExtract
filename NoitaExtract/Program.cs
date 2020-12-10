using System;
using System.IO;
using System.Collections.Generic;

namespace NoitaExtract
{
    class Program
    {
        /*DirectorySecurity securityRules = new DirectorySecurity();
        securityRules.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));*/

        
        static void Main(string[] args)
        {
            /*
                struct header
                4 bytes - null, presumably filemagic
                4 bytes - v0, looks like a version number, followed by 2 null bytes to leave space
                8 bytes - ulong start of file data

                file info lump
                4 bytes - offset of file data
                4 bytes - file size?
                4 bytes - uint filename length
                N bytes - filename
            */

            /*if (args.Length >= 2)
            {
                //Console.WriteLine("ArcEnemy - Convert directory trees into CatEngine archives (.cat)\nInput the folder.");
                string input = args[0]; //Console.ReadLine();
                //Console.WriteLine("Input the output file.");
                string output = args[1]; // Console.ReadLine();

                UnpackArc(input, output);
            }
            else*/
                Console.WriteLine("NoitaExtract - Unpack .wak file into a directory\nUsage: NoitaExtract.exe sourcefile destfolder");

            string input = Console.ReadLine();
                                    //Console.WriteLine("Input the output file.");
            string output = Console.ReadLine();

            UnpackArc(input, output);
        }

        static bool UnpackArc(string input, string output)
        {
            List<NoitaFile> FileList = new List<NoitaFile>();

            using (FileStream stream = new FileStream(input, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    if (reader.ReadUInt32() != 0x00) //filemagic
                        return false;

                    if (reader.ReadUInt32() != 0x3076) //version
                        return false;

                    long dataoffset = reader.ReadInt64();

                    while (stream.Position < dataoffset)
                    {
                        FileList.Add(ReadFile(reader));
                    }

                    foreach (NoitaFile f in FileList)
                    {
                        stream.Seek((long)f.FileOffset, SeekOrigin.Begin);

                        byte[] data = reader.ReadBytes((int)f.FileSize);

                        string fullpath = f.Path;
                        string[] directorypathhack = fullpath.Split('/'); //C# fucking sucks

                        string[] filepathparts = f.Path.Split('/');

                        //string fullpath = output + '\\' + filepathparts[filepathparts.Length - 1];//output + "/" + 

                        string path = "";

                        for (int i = 0; i < directorypathhack.Length-1; i++)
                        {
                            path += directorypathhack[i]+'/';
                        }

                        Console.WriteLine("creating dir " + path);
                        DirectoryInfo di = Directory.CreateDirectory(path);

                        using (FileStream outstream = File.Create(fullpath))
                        {
                            using (BinaryWriter writer = new BinaryWriter(outstream))
                            {
                                writer.Write(data);
                            }
                        }
                    }
                }
            }

            return true;
        }

        static NoitaFile ReadFile(BinaryReader reader)
        {
            //File returnfile;

            uint offset = reader.ReadUInt32();
            uint filesize = reader.ReadUInt32();
            uint namelength = reader.ReadUInt32();

            string name = "";

            for (int i = 0; i < namelength; i++)
            {
                name += (char)reader.ReadByte();
            }

            return new NoitaFile(offset, filesize, name);
        }

        /*static void ExtractFile(string outpath, Stream BinaryReader reader, File file)
        {
            
        }*/

        struct NoitaFile
        {
            public uint FileOffset;
            public uint FileSize;
            public string Path;

            public NoitaFile(uint offs, uint size, string path)
            {
                FileOffset = offs;
                FileSize = size;
                Path = path;

                Console.WriteLine("File " + Path + " offset " + FileOffset + " FileSize " + FileSize + " bytes.");
            }
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;

namespace NoitaExtract
{
    class Program
    {
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

            if (args.Length >= 2)
            {
                //Console.WriteLine("ArcEnemy - Convert directory trees into CatEngine archives (.cat)\nInput the folder.");
                string input = args[0]; //Console.ReadLine();
                //Console.WriteLine("Input the output file.");
                string output = args[1]; // Console.ReadLine();

                UnpackArc(input, output);
            }
            else
                Console.WriteLine("NoitaExtract - Unpack .wak file into a directory\nUsage: NoitaExtract.exe sourcefile destfolder");
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

                    //this uint64 signifies the byte from which the file data starts
                    long dataoffset = reader.ReadInt64();

                    //looping through the list of files in the header
                    while (stream.Position < dataoffset)
                    {
                        FileList.Add(ReadFile(reader));
                    }

                    //the file info has been managed; let's extract the stuff
                    foreach (NoitaFile f in FileList)
                    {
                        //seeking to file start in stream
                        stream.Seek((long)f.FileOffset, SeekOrigin.Begin);

                        //creating a byte array from the file data within
                        byte[] data = reader.ReadBytes((int)f.FileSize);

                        //this is a massive hack to convert the path, because Path.Join didn't work somehow
                        string fullpath = f.Path;
                        string[] directorypathhack = fullpath.Split('/');

                        string path = "";

                        for (int i = 0; i < directorypathhack.Length-1; i++)
                        {
                            path += directorypathhack[i]+'/';
                        }

                        //logging...
                        Console.WriteLine("creating dir " + path);

                        //if the directory we're sending data to doesn't exist, we create it
                        DirectoryInfo di = Directory.CreateDirectory(path);

                        //let's write the file
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

        //file info reader; produces a NoitaFile with info filled out
        static NoitaFile ReadFile(BinaryReader reader)
        {
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

        //the file info struct
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

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FileSpliter
{
    public class Chunk
    {
        public string hash { get; set; }
        public string location { get; set; }
    }
    class Program
    {
        public static int GetNC(int fs)
        {
            if(fs > 99999 && fs < 9000000)
            {
                return (int)(fs/1000000) * 2;
            }

            return 16;
        }
        static void Main(string[] args)
        {
            string file_name = "";

            string folder_destination = "";

            if (File.Exists(file_name))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(file_name, FileMode.Open)))
                {
                    int file_size = (int) reader.BaseStream.Length;

                    int number_of_chunks = GetNC(file_size);

                    int chunk_size = file_size / number_of_chunks;

                    int rest_of_bytes = file_size - chunk_size * number_of_chunks;

                    byte[][] files;

                    if (rest_of_bytes > 0)
                    {

                        files = new byte[number_of_chunks + 1][];
                    }
                    else
                    {
                        files = new byte[number_of_chunks][];
                    }

                    for (int i = 0; i < number_of_chunks; i++)
                    {
                        files[i] = reader.ReadBytes(chunk_size);
                    }

                    if(rest_of_bytes > 0)
                    {
                        number_of_chunks++;
                        files[files.Length - 1] = new byte[chunk_size];
                        reader.ReadBytes(rest_of_bytes).CopyTo(files[files.Length - 1], 0);
                    }

                    Chunk[] hashes = new Chunk[files.Length];

                    Random r = new Random();
                    for (int i = 0; i < files.Length; i++)
                    {
                        hashes[i] = new Chunk();
                        SHA1Managed sha1 = new SHA1Managed();
                    
                        byte[] hash = sha1.ComputeHash(files[i]);
                        string sb = string.Concat(hash.Select(b => b.ToString("x2")));

                        string location = folder_destination + r.Next(1, 5) + "\\" + sb.ToString() + ".cnk";

                        hashes[i].location = location;
                        hashes[i].hash = sb.ToString();

                        using (BinaryWriter writer = new BinaryWriter(File.Open(location, FileMode.Create)))
                        {
                            writer.Write(files[i]);
                        }
                    }

                    using (BinaryWriter writer = new BinaryWriter(File.Open(folder_destination + "main.ref", FileMode.Create)))
                    {
                        writer.Write(Path.GetFileName(file_name));
                        writer.Write(file_size);
                        writer.Write(number_of_chunks);
                        writer.Write(chunk_size);
                        writer.Write((rest_of_bytes > 0 ? rest_of_bytes : chunk_size) );

                        for (int i = 0; i < hashes.Length; i++)
                        {
                            writer.Write(hashes[i].location);
                            writer.Write(hashes[i].hash);
                        }
                    }
                }
            }

        }
    }
}

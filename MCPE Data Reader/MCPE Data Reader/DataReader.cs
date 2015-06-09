using System.IO.Compression;
using System.IO;
using LevelDB;
using System.Collections.Generic;
using System;

namespace MCPE_Data_Reader
{
	public class DataReader
    {
		private DB MCPEFile;
		private string uncPath;
        DataReader(string path)
		{
			FileStream file = new FileStream(path, FileMode.Open);
			DeflateStream Unpacker = new DeflateStream(file, CompressionMode.Decompress);
			List<byte> UncompressedFile = new List<byte>();
			int currByte;
			do
			{
				currByte = Unpacker.ReadByte();
				if (currByte == -1)
					break;
				UncompressedFile.Add(Convert.ToByte(currByte));
			} while (currByte != -1); //yes I am lazy. "Double check for security reasons" is the official explanation
			file.Close();
			uncPath = path + ".unpacked";
            file = new FileStream(uncPath, FileMode.Create);
			foreach(var Byte in UncompressedFile)
			{
				file.WriteByte(Byte);
			}
			file.Close();
			Options set = new Options();
			set.BlockSize = 83200;
			set.Compression = CompressionType.NoCompression;
			MCPEFile = DB.Open(set, uncPath);
		}

		public List<byte> getChunkData(int x, int z)
		{
			string key = x.ToString() + z.ToString() + "0";
			var TempBytes = Convert.ToByte(MCPEFile.Get(key));
			List<byte> Response = new List<byte>(TempBytes);
			var BlockData = Response.GetRange(32767, 16384);
			return Response;
		}

		public List<byte> getChunkID(int x, int z)
		{
			string key = x.ToString() + z.ToString() + "0";
			var TempBytes = Convert.ToByte(MCPEFile.Get(key));
			List<byte> Response = new List<byte>(TempBytes);
			var BlockData = Response.GetRange(0, 32768);
			return Response;
		}

		public int getBlockData(List<byte> chunk, int x, int y, int z)
		{
			var RealChunk = convertBytesToNibble(chunk);
			return Convert.ToInt32(RealChunk[x + y + z]);

		}

		public int getBlockID(List<byte> chunk, int x, int y, int z)
		{
			
			return Convert.ToInt32(chunk[x + y + z]);
					
		}

		public List<byte> convertBytesToNibble(List<byte> Bytes)
		{
			List<byte> nibblesConverted = new List<byte>();
			foreach(var Byte in Bytes)
			{
				byte nibble1 = (byte)(Byte & 0x0F);
				byte nibble2 = (byte)((Byte & 0xF0) >> 4);
				nibblesConverted.Add(nibble1);
				nibblesConverted.Add(nibble2);
			}
			return nibblesConverted;
		}
    }
}

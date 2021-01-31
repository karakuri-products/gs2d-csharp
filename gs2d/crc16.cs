using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace gs2d
{
	class CRC16
	{
		public static ushort calculate(byte[] data, ushort length)
		{
			ushort crc = 0;
			for (ushort i = 0; i < length; i++)
			{
				crc ^= (ushort)(data[i] << 8);
				for (byte j = 0; j < 8; j++)
				{
					if ((crc & 0x8000) != 0)
					{
						crc = (ushort)((crc << 1) ^ 0x8005);
					}
					else
					{
						crc <<= 1;
					}
				}
			}

			return crc;
		}
	}
}

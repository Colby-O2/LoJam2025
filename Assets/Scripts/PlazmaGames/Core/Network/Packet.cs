using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PlazmaGames.Core.Network
{
    public sealed class Packet
    {
        public readonly byte[] data;

        public Packet(byte[] data)
        {
            this.data = data.ToArray();
        }
    }

    public sealed class PacketWriter
    {
        private List<byte> _buffer;
        private int _id;

        public PacketWriter(int id)
        {
            _id = id;
            _buffer = new List<byte>();
        }

        public PacketWriter(int id, params object[] vals)
        {
            _id = id;
            _buffer = new List<byte>();
            Write(vals);
        }

        private byte[] GetDataWithHeader()
        {
            List<byte> buffer = new List<byte>(_buffer);
            buffer.InsertRange(0, BitConverter.GetBytes(_id));
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count + sizeof(int)));
            return buffer.ToArray();
        }

        public void Write(byte val) => _buffer.Add(val);

        public void Write(byte[] val)
        {
            if (val != null) _buffer.AddRange(val);
        }

        public void Write(int val) => _buffer.AddRange(BitConverter.GetBytes(val));

        public void Write(short val) => _buffer.AddRange(BitConverter.GetBytes(val));

        public void Write(long val) => _buffer.AddRange(BitConverter.GetBytes(val));

        public void Write(float val) => _buffer.AddRange(BitConverter.GetBytes(val));

        public void Write(double val) => _buffer.AddRange(BitConverter.GetBytes(val));

        public void Write(bool val) => _buffer.AddRange(BitConverter.GetBytes(val));

        public void Write(string val, bool writeLength = true)
        {
            if (val != null)
            {
                if (writeLength) Write(val.Length);
                _buffer.AddRange(Encoding.ASCII.GetBytes(val));
            }
            else Write(string.Empty, writeLength);
        }

        public void WriteJSON<T>(T val, bool writeLength = true) => Write(JsonConvert.SerializeObject(val), writeLength);

        public void Write(params object[] vals)
        {
            foreach (object val in vals)
            {
                if (val is short)
                {
                    short castedVal = (short)val;
                    Write(castedVal);
                }
                else if (val is int)
                {
                    int castedVal = (int)val;
                    Write(castedVal);
                }
                else if (val is long)
                {
                    long castedVal = (long)val;
                    Write(castedVal);
                }
                else if (val is float)
                {
                    float castedVal = (float)val;
                    Write(castedVal);
                }
                else if (val is double)
                {
                    double castedVal = (double)val;
                    Write(castedVal);
                }
                else if (val is bool)
                {
                    bool castedVal = (bool)val;
                    Write(castedVal);
                }
                else if (val is byte)
                {
                    byte castedVal = (byte)val;
                    Write(castedVal);
                }
                else if (val is byte[])
                {
                    byte[] castedVal = (byte[])val;
                    Write(castedVal);
                }
                else if (val is string)
                {
                    string castedVal = (string)val;
                    Write(castedVal);
                }
                else
                {
                    WriteJSON(Convert.ChangeType(val, val.GetType()));
                }
            }
        }

        public Packet GetPacket()
        {
            return new Packet(GetDataWithHeader());
        }

        public Packet GetPacketWithoutHeader()
        {
            return new Packet(_buffer.ToArray());
        }
    }

    public sealed class PacketReader
    {

        private int _readPos;
        private Packet _packet;
        private byte[] _buffer => _packet.data;

        private int _id;
        private int _length;

        public PacketReader(Packet packet)
        {
            _packet = new Packet(packet.data);
            (_id, _length) = ReadHeader();
        }

        public Packet GetPacket() => _packet;

        public int GetLength() => _buffer.Length;

        public int GetUnreadLength() => GetLength() - _readPos;

        public int GetID() => _id;

        public TRequest GetID<TRequest>() where TRequest : System.Enum => (TRequest)System.Enum.ToObject(typeof(TRequest), _id);

        private (int, int) ReadHeader()
        {
            int length = ReadInt();
            int id = ReadInt();
            return (id, length);
        }

        public (int, int) GetHeaderContent()
        {
            return (_id, _length);
        }

        public (TRequest, int) GetHeaderContent<TRequest>() where TRequest : System.Enum
        {
            return ((TRequest)System.Enum.ToObject(typeof(TRequest), _id), _length);
        }

        public byte[] ReadBytes(int length, bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                byte[] val = new ArraySegment<byte>(_buffer, _readPos, length).ToArray();
                if (moveReadPos) _readPos += length;
                return val;
            }
            else
            {
                throw new Exception($"Could not read value of type 'byte[]' of size {length}!");
            }
        }

        public byte ReadByte(bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                byte val = _buffer[_readPos];
                if (moveReadPos) _readPos += sizeof(byte);
                return val;
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        public short ReadShort(bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                short val = BitConverter.ToInt16(_buffer, _readPos);
                if (moveReadPos) _readPos += sizeof(short);
                return val;
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        public int ReadInt(bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                int val = BitConverter.ToInt32(_buffer, _readPos);
                if (moveReadPos) _readPos += sizeof(int);
                return val;
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        public long ReadLong(bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                long val = BitConverter.ToInt64(_buffer, _readPos);
                if (moveReadPos) _readPos += sizeof(long);
                return val;
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        public float ReadFloat(bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                float val = BitConverter.ToSingle(_buffer, _readPos);
                if (moveReadPos) _readPos += sizeof(float);
                return val;
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        public double ReadDouble(bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                double val = BitConverter.ToDouble(_buffer, _readPos);
                if (moveReadPos) _readPos += sizeof(double);
                return val;
            }
            else
            {
                throw new Exception("Could not read value of type 'double'!");
            }
        }

        public bool ReadBool(bool moveReadPos = true)
        {
            if (_buffer.Length > _readPos)
            {
                bool val = BitConverter.ToBoolean(_buffer, _readPos);
                if (moveReadPos) _readPos += sizeof(bool);
                return val;
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        public string ReadStrng(bool moveReadPos = true)
        {
            try
            {
                int length = ReadInt(moveReadPos);
                string val = Encoding.ASCII.GetString(_buffer, _readPos + (moveReadPos ? 0 : sizeof(int)), length);
                if (moveReadPos && val.Length > 0) _readPos += length;
                return val;
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }

        public T ReadJSON<T>(bool moveReadPos = true)
        {
            return JsonConvert.DeserializeObject<T>(ReadStrng(moveReadPos));
        }
    }
}

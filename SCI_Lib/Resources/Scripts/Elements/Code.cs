using SCI_Lib.Resources.Scripts.Sections;
using SCI_Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCI_Lib.Resources.Scripts.Elements
{
    // https://wiki.scummvm.org/index.php?title=SCI/Specifications/SCI_virtual_machine/The_Sierra_PMachine#The_instruction_set

    public class Code : BaseElement
    {
        private string _name;
        private CodeSection _section;

        public Code(CodeSection section, ushort address, Code prev)
            : base(section.Script, address)
        {
            _section = section;
            Prev = prev;
            if (prev != null)
                prev.Next = this;
        }


        public byte Type { get; set; }

        public List<object> Arguments { get; private set; }

        public Code Prev { get; private set; }
        public Code Next { get; private set; }

        public string Name
        {
            get
            {
                if (_name != null) return _name;
                return _name = Script.GetOpCodeName((byte)(Type >> 1));
            }
        }

        public string ArgsToString()
        {
            StringBuilder sb = new();

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (Type == 0x43 && i == 0)
                {
                    sb.Append(Script.Package.GetFuncName((byte)Arguments[i]));
                }
                else
                {
                    object a = Arguments[i];
                    switch (a)
                    {
                        case byte b:
                            sb.Append($"${b:x2}");
                            break;
                        case ushort s:
                            sb.Append($"${s:x4}");
                            break;
                        default:
                            sb.Append(a.ToString());
                            break;
                    }
                }

                if (i != Arguments.Count - 1)
                    sb.Append(", ");
            }

            return sb.ToString().TrimEnd();
        }

        public string ArgsToHex()
        {
            StringBuilder sb = new();

            for (int i = 0; i < Arguments.Count; i++)
            {
                object a = Arguments[i];
                switch (a)
                {
                    case byte b:
                        sb.Append($"{b:x2}");
                        break;
                    case ushort s:
                        sb.Append($"{s:x4}");
                        break;
                    case RefToElement r:
                        sb.Append(r.ToHex(Address + Size));
                        break;

                    default:
                        sb.Append(a.ToString());
                        break;
                }

                if (i != Arguments.Count - 1)
                    sb.Append(" ");
            }

            return sb.ToString();
        }

        public override string ToString() => $"{Address:x4}: {Type:x2} {ArgsToHex(),-8} \t {Name} {ArgsToString(),-25}";
        public string ASM => $"{Name} {ArgsToString()}";

        public void Read(byte[] data, ref ushort offset)
        {
            Arguments = new List<object>();
            Type = data[offset++];

            if (Type > 0x7F)
            {
                if ((Type & 0x01) == 0)
                    Arguments.Add((ushort)(data[offset++] + (data[offset++] << 8)));
                else
                    Arguments.Add(data[offset++]);
                return;
            }

            ushort addr = offset;

            switch (Type)
            {
                // 1 byte
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x36:
                case 0x37:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x48:
                case 0x49:
                case 0x5c:
                case 0x5d:
                case 0x60:
                case 0x61:
                case 0x76:
                case 0x77:
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                    break;

                // B
                case 0x2f:
                case 0x31:
                case 0x35:
                case 0x39:
                case 0x3f:
                case 0x4a:
                case 0x4b:
                case 0x51:
                case 0x54:
                case 0x55:
                case 0x59:
                case 0x63:
                case 0x65:
                case 0x67:
                case 0x68:
                case 0x6b:
                case 0x6d:
                case 0x6f: // ipTos
                case 0x71: // dpTos
                case 0x73:
                case 0x75:
                    Arguments.Add(data[offset++]);
                    break;

                case 0x33:
                    {
                        var a1 = data[offset++];
                        Arguments.Add(new RefToElement(Script, addr, a1, (ushort)(offset + 1 + a1), 1) { Source = this });
                    }
                    break;

                // B B
                case 0x3e:
                case 0x43: // callk
                case 0x45:
                case 0x50:
                case 0x57: // ???  super B class, B stackframe (3 bytes)
                case 0x58:
                case 0x5b:
                case 0x62:
                case 0x64:
                case 0x66:
                case 0x6a:
                case 0x6c:
                case 0x74:
                    Arguments.Add(data[offset++]);
                    Arguments.Add(data[offset++]);
                    break;

                // W
                case 0x34:
                case 0x38:
                    Arguments.Add((ushort)(data[offset++] + (data[offset++] << 8)));
                    break;

                // Relative offset
                case 0x41: // call B
                    {
                        var a1 = data[offset++];
                        Arguments.Add(new CodeRef(this, addr, a1, (ushort)(offset + a1), 1));
                        Arguments.Add(data[offset++]);
                    }
                    break;

                case 0x2e: // bt
                case 0x30: // bnt
                case 0x32: // jmp
                    {
                        var a1 = ReadUShort(data, ref offset);
                        Arguments.Add(new CodeRef(this, addr, a1, (ushort)(offset + a1), 2));
                    }
                    break;

                case 0x72: // lofsa
                    {
                        var a1 = ReadUShort(data, ref offset);
                        if (Script.Package.ViewFormat == ViewFormat.EGA)
                            //Arguments.Add(new CodeRef(this, addr, val, (ushort)(offset + val), 2));
                            Arguments.Add(new RefToElement(Script, addr, (ushort)(offset + a1)) { Source = this });
                        else
                            Arguments.Add(new RefToElement(Script, addr, a1) { Source = this });
                    }
                    break;

                // W B
                case 0x42:
                case 0x44:
                case 0x56: // ???  super W class, B stackframe (4 bytes)
                    Arguments.Add(ReadUShort(data, ref offset));
                    Arguments.Add(data[offset++]);
                    break;

                // B B B
                case 0x47:
                    Arguments.Add(data[offset++]);
                    Arguments.Add(data[offset++]);
                    Arguments.Add(data[offset++]);
                    break;

                case 0x40: // call W B
                    {
                        var a1 = ReadUShort(data, ref offset);
                        var a2 = data[offset++];
                        Arguments.Add(new CodeRef(this, addr, a1, (ushort)(offset + a1), 2));
                        Arguments.Add(a2);
                    }
                    break;

                // W W
                case 0x5a: // lea
                    Arguments.Add(ReadUShort(data, ref offset));
                    Arguments.Add(ReadUShort(data, ref offset));
                    break;

                // W W B
                case 0x46: // calle
                    {
                        Arguments.Add(ReadUShort(data, ref offset));
                        Arguments.Add(ReadUShort(data, ref offset));
                        Arguments.Add(data[offset++]);
                    }
                    break;

                case 0x4c:
                case 0x4d:
                case 0x4e:
                case 0x4f:
                case 0x52:
                case 0x53:
                case 0x5e:
                case 0x5f:
                case 0x7e:
                case 0x7f:
                    throw new Exception("This operator is not exists");

                default: throw new NotImplementedException($"OpCode {Type:X02} '{Name}' arg's unknown");
            }
        }

        static ushort ReadUShort(byte[] data, ref ushort offset)
        {
            var l = data[offset++];
            var h = data[offset++];
            return (ushort)(l | (h << 8));
        }

        public int Size
        {
            get
            {
                if (Type > 0x7F)
                {
                    if ((Type & 0x01) == 0)
                        return 3;
                    else
                        return 2;
                }

                if (Type < 0x2e) return 1;

                switch (Type)
                {
                    case 0x36:
                    case 0x37:
                    case 0x3a:
                    case 0x3b:
                    case 0x3c:
                    case 0x3d:
                    case 0x48:
                    case 0x49:
                    case 0x5c:
                    case 0x5d:
                    case 0x60:
                    case 0x61:
                    case 0x76:
                    case 0x77:
                    case 0x78:
                    case 0x79:
                    case 0x7a:
                    case 0x7b:
                    case 0x7c:
                    case 0x7d:
                        return 1;

                    // 2 bytes
                    case 0x2f:
                    case 0x31:
                    case 0x35:
                    case 0x39:
                    case 0x3f:
                    case 0x4a:
                    case 0x4b:
                    case 0x51:
                    case 0x54:
                    case 0x55:
                    case 0x59:
                    case 0x63:
                    case 0x65:
                    case 0x67:
                    case 0x68:
                    case 0x6b:
                    case 0x6d:
                    case 0x73:
                    case 0x75:
                    case 0x33:
                        return 2;

                    // 3 bytes
                    case 0x2e:
                    case 0x34:
                    case 0x3e:
                    case 0x43:
                    case 0x45:
                    case 0x50:
                    case 0x57:
                    case 0x58:
                    case 0x5b:
                    case 0x62:
                    case 0x64:
                    case 0x66:
                    case 0x6a:
                    case 0x6c:
                    case 0x74:
                    case 0x38:
                    case 0x41:
                    case 0x30:
                    case 0x32:
                    case 0x72:
                        return 3;

                    // 4 bytes
                    case 0x42:
                    case 0x44:
                    case 0x47:
                    case 0x56: // ???  super W class, B stackframe (4 bytes)
                    case 0x40:
                        return 4;

                    // 5 bytes
                    case 0x5a:
                        return 5;

                    // 6 bytes
                    case 0x46:
                        return 6;

                    case 0x4c:
                    case 0x4d:
                    case 0x4e:
                    case 0x4f:
                    case 0x52:
                    case 0x53:
                    case 0x5e:
                    case 0x5f:
                    case 0x7e:
                    case 0x7f:
                        throw new Exception("This operator is not exists");

                    default: throw new NotImplementedException();
                }
            }
        }

        public bool IsCall => Name switch
        {
            "self" or "send" or "super" or "call" or "calle" => true,
            _ => false,
        };

        public bool IsReturn => Name switch
        {
            "ret" => true,
            _ => false,
        };

        public bool IsJump => Name switch
        {
            "bt" or "bnt" or "jmp" => true,
            _ => false,
        };

        public override void SetupByOffset()
        {
            foreach (object a in Arguments)
                if (a is RefToElement r)
                    r.SetupByOffset();
        }

        protected override void WriteData(ByteBuilder bb)
        {
            Address = (ushort)bb.Position;
            bb.AddByte(Type);
            foreach (object arg in Arguments)
            {
                switch (arg)
                {
                    case byte b:
                        bb.AddByte(b);
                        break;
                    case ushort s:
                        bb.AddUShortBE(s);
                        break;
                    case RefToElement r:
                        r.Write(bb);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public override void WriteOffset(ByteBuilder bb)
        {
            foreach (object arg in Arguments)
            {
                switch (arg)
                {
                    case byte _:
                    case ushort _:
                        continue;
                    case RefToElement r:
                        r.WriteOffset(Address + Size, bb);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void InjectNext(byte type, params object[] args)
        {
            var ind = _section.Operators.IndexOf(this);
            var oldNext = Next;
            var code = new Code(_section, 0, this);
            code.Type = type;
            code.Arguments = new List<object>(args);
            _section.Operators.Insert(ind + 1, code);
            code.Next = oldNext;
        }
    }
}

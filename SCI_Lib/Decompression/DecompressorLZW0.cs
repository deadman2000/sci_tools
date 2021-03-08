using SCI_Lib.Utils;
using System;
using System.IO;

namespace SCI_Lib.Decompression
{
    class DecompressorLZW0 : Decompressor
    {
        protected override void GoUnpack()
        {
            var reader = new BitReaderLSB(_stream);
         
            ushort numbits;
            ushort curtoken, endtoken;

            numbits = 9;
            curtoken = 0x102;
            endtoken = 0x1ff;

            ushort token; // The last received value

            ushort[] tokenlist = new ushort[4096];
            ushort[] tokenlengthlist = new ushort[4096];

            while (!IsFinished())
            {
                token = (ushort)reader.GetBits((byte)numbits);

                if (token == 0x101)
                {
                    return; // terminator
                }

                if (token == 0x100)
                { // reset command
                    numbits = 9;
                    curtoken = 0x0102;
                    endtoken = 0x1FF;
                }
                else
                {
                    ushort tokenlastlength;
                    if (token > 0xff)
                    {
                        if (token >= curtoken)
                            throw new IOException(String.Format("unpackLZW: Bad token {0:X4}", token));

                        tokenlastlength = (ushort)(tokenlengthlist[token] + 1);
                        if (_dwWrote + tokenlastlength > _szUnpacked)
                        {
                            // For me this seems a normal situation, It's necessary to handle it
                            Console.WriteLine("unpackLZW: Trying to write beyond the end of array(len={0}, destctr={1}, tok_len={2})", _szUnpacked, _dwWrote, tokenlastlength);
                            for (int i = 0; _dwWrote < _szUnpacked; i++)
                                PutByte(_dest[tokenlist[token] + i]);
                        }
                        else
                            for (int i = 0; i < tokenlastlength; i++)
                                PutByte(_dest[tokenlist[token] + i]);
                    }
                    else
                    {
                        tokenlastlength = 1;
                        if (_dwWrote >= _szUnpacked)
                            Console.WriteLine("unpackLZW: Try to write single byte beyond end of array");
                        else
                            PutByte((byte)token);
                    }
                    if (curtoken > endtoken && numbits < 12)
                    {
                        numbits++;
                        endtoken = (ushort)((endtoken << 1) + 1);
                    }
                    if (curtoken <= endtoken)
                    {
                        tokenlist[curtoken] = (ushort)(_dwWrote - tokenlastlength);
                        tokenlengthlist[curtoken] = tokenlastlength;
                        curtoken++;
                    }

                }
            }
        }

    }
}

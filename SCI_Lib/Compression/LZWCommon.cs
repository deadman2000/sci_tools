namespace SCI_Lib.Compression
{
    /// <summary>
    /// Constants for LZW compression algorithm
    /// </summary>
    public static class LZWConstants
    {
        public const ushort ClearCode = 0x100;
        public const ushort EndCode = 0x101;
        public const ushort FirstCode = 0x102;
        public const ushort InitialEndToken = 0x1FF;
        public const ushort MaxTokenIndex = 0x1004;
        public const byte InitialBits = 9;
        public const byte MaxBits = 12;
    }

    /// <summary>
    /// Token structure for LZW compression/decompression
    /// </summary>
    public struct LZWToken
    {
        public byte data;
        public ushort next;
        public override readonly string ToString() => $"{data:X02} => {next:X04}";
    }

    /// <summary>
    /// Helper class for managing LZW token table state
    /// </summary>
    public class LZWState
    {
        protected readonly LZWToken[] tokens;
        protected ushort curtoken;
        protected ushort endtoken;
        protected byte numbits;

        public LZWState() : this(new LZWToken[LZWConstants.MaxTokenIndex])
        {
        }

        protected LZWState(LZWToken[] existingTokens)
        {
            tokens = existingTokens;
            Reset();
        }

        public void Reset()
        {
            numbits = LZWConstants.InitialBits;
            curtoken = LZWConstants.FirstCode;
            endtoken = LZWConstants.InitialEndToken;
        }

        public bool ShouldIncreaseBits()
        {
            return curtoken == endtoken && numbits < LZWConstants.MaxBits;
        }

        public void IncreaseBits()
        {
            numbits++;
            endtoken = (ushort)((endtoken << 1) + 1);
        }

        public bool CanAddToken()
        {
            return curtoken <= endtoken;
        }

        public ushort CurrentToken => curtoken;
        public byte NumBits => numbits;
        public ushort EndToken => endtoken;

        protected void AddTokenInternal(byte data, ushort next)
        {
            if (!CanAddToken()) return;
            tokens[curtoken].data = data;
            tokens[curtoken].next = next;
            curtoken++;
        }

        protected LZWToken GetToken(ushort index) => tokens[index];
        protected bool IsToken(ushort val) => (val > 0xff) && (val < LZWConstants.MaxTokenIndex);
        
        protected LZWToken[] Tokens => tokens;
        protected ushort CurToken => curtoken;
    }
}

namespace Chip8
{
    internal class CPU
    {
        public byte[] Memory { get; set; } // 4kb RAM
        public byte[] Vram { get; set; } // 32 * 64 pixel display
        public byte[] V { get; set; } // 16 general purpose 8-bit registers

        public ushort[] Stack { get; set; }

        public ushort Opcode { get; set; }
        public ushort PC { get; set; }
        public ushort I { get; set; } // This register is generally used to store memory addresses

        public byte SP { get; set; } // Stack pointer can be 8-bit

        public byte DelayTimer { get; set; }
        public byte SoundTimer { get; set; }

        public CPU()
        {
            Memory = new byte[4096];
            Vram = new byte[32 * 64];
            V = new byte[16];
            Stack = new ushort[16];

            Opcode = 0;
            SP = 0;
            I = 0;

            PC = 0x200; // Most Chip-8 programs start at location 0x200 
        }

        public void Cycle()
        {

        }
    }
}

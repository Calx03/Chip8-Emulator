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

        private readonly byte[] fonts =
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

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
            Opcode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            switch (Opcode & 0xF000)
            {
                case 0x1000:
                    Ins_1xNNN(Opcode);
                    break;

                case 0x6000:
                    Ins_6xKK(Opcode);
                    break;
            }
        }

        private void Ins_1xNNN(ushort opcode)
        {
            PC = (ushort)(opcode & 0x0FFF);
        }

        private void Ins_6xKK(ushort opcode)
        {
            var register = (opcode & 0x0F00) >> 8;
            V[register] = (byte)(opcode & 0x00FF);
            PC += 2;
        }
    }
}

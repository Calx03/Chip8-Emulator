using System.Text;

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

        public void DebugGraphics()
        {
            var output = new StringBuilder();

            output.AppendLine(" ----------------------------------------------------------------");
            for (int i = 0; i < 32; i++)
            {
                output.Append('|');
                for (int j = 0; j < 64; j++)
                {
                    output.Append(Vram[i * 64 + j] > 0 ? "█" : " ");
                }
                output.AppendLine("|");
            }
            output.AppendLine(" ----------------------------------------------------------------");

            Console.WriteLine(output);
        }

        public void Cycle()
        {
            Opcode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            switch (Opcode & 0xF000)
            {
                case 0x0000 when Opcode == 0x00E0:
                    Ins_00E0();
                    break;

                case 0x0000 when Opcode == 0x00EE:
                    Ins_00EE();
                    break;

                case 0x1000:
                    Ins_1NNN();
                    break;

                case 0x2000:
                    Ins_2NNN();
                    break;

                case 0x3000:
                    Ins_3XKK();
                    break;

                case 0x4000:
                    Ins_4XKK();
                    break;

                case 0x5000:
                    Ins_5XY0();
                    break;

                case 0x6000:
                    Ins_6XKK();
                    break;

                case 0x7000:
                    Ins_7XKK();
                    break;

                case 0x8000 when (Opcode & 0x000F) == 0:
                    Ins_8XY0();
                    break;

                case 0x8000 when (Opcode & 0x000F) == 1:
                    Ins_8XY1();
                    break;

                case 0x8000 when (Opcode & 0x000F) == 2:
                    Ins_8XY2();
                    break;

                case 0x8000 when (Opcode & 0x000F) == 3:
                    Ins_8XY3();
                    break;
            }
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        private void Ins_00E0()
        {
            // TODO
        }

        /// <summary>
        /// Returns from a subroutine.
        /// </summary>
        private void Ins_00EE()
        {
            PC = Stack[SP--];
        }

        /// <summary>
        /// Jumps to address NNN.
        /// </summary>
        private void Ins_1NNN()
        {
            PC = (ushort)(Opcode & 0x0FFF);
        }

        /// <summary>
        /// Calls subroutine at NNN.
        /// </summary>
        private void Ins_2NNN()
        {
            Stack[++SP] = PC;
            PC = (ushort)(Opcode & 0x0FFF);
        }

        /// <summary>
        /// Skips the next instruction if VX equals KK
        /// </summary>
        private void Ins_3XKK()
        {
            byte register = (byte)((Opcode & 0x0F00) >> 8);

            if (V[register] == (Opcode & 0x00FF))
            {
                PC += 2;
            }
        }


        /// <summary>
        /// Skips the next instruction if VX does not equal KK
        /// </summary>
        private void Ins_4XKK()
        {
            byte register = (byte)((Opcode & 0x0F00) >> 8);

            if (V[register] != (Opcode & 0x00FF))
            {
                PC += 2;
            }
        }

        /// <summary>
        /// Skips the next instruction if VX equals VY
        /// </summary>
        private void Ins_5XY0()
        {
            byte x = (byte)((Opcode & 0x0F00) >> 8);
            byte y = (byte)((Opcode & 0x00F0) >> 4);

            if (V[x] == V[y])
            {
                PC += 2;
            }
        }

        /// <summary>
        /// Sets VX to KK.
        /// </summary>
        private void Ins_6XKK()
        {
            var register = (Opcode & 0x0F00) >> 8;
            V[register] = (byte)(Opcode & 0x00FF);
            PC += 2;
        }

        /// <summary>
        /// Adds KK to VX (carry flag is not changed)
        /// </summary>
        private void Ins_7XKK()
        {
            byte register = (byte)((Opcode & 0x0F00) >> 8);
            byte value = (byte)(Opcode & 0x00FF);

            V[register] += value;
            PC += 2;
        }

        /// <summary>
        /// Stores the value of register Vy in register Vx.
        /// </summary>
        private void Ins_8XY0()
        {
            V[(Opcode & 0x0F00) >> 8] = V[(Opcode & 0x00F0) >> 4];

            PC += 2;
        }

        /// <summary>
        /// Sets VX to VX or VY. (bitwise OR operation)
        /// </summary>
        private void Ins_8XY1()
        {
            byte x = (byte)((Opcode & 0x0F00) >> 8);
            byte y = (byte)((Opcode & 0x00F0) >> 4);

            V[x] = (byte)(V[x] | V[y]);

            PC += 2;
        }

        /// <summary>
        /// Sets VX to VX and VY. (bitwise AND operation)
        /// </summary>
        private void Ins_8XY2()
        {
            byte x = (byte)((Opcode & 0x0F00) >> 8);
            byte y = (byte)((Opcode & 0x00F0) >> 4);

            V[x] = (byte)(V[x] & V[y]);

            PC += 2;
        }

        /// <summary>
        /// Sets VX to VX xor VY.
        /// </summary>
        private void Ins_8XY3()
        {
            byte x = (byte)((Opcode & 0x0F00) >> 8);
            byte y = (byte)((Opcode & 0x00F0) >> 4);

            V[x] = (byte)(V[x] ^ V[y]);

            PC += 2;
        }
    }
}

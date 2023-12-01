namespace Chip8
{
    internal class CPU
    {
        public byte[] Memory { get; set; } // 4kb RAM
        public byte[] Vram { get; set; } // 32 * 64 pixel display
        public byte[] V { get; set; } // 16 general purpose 8-bit registers

        public ushort[] Stack { get; set; }

        private bool[] keys;
        private byte counter;

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

        public CPU(string filePath)
        {
            Memory = new byte[4096];
            Vram = new byte[32 * 64];
            V = new byte[16];
            Stack = new ushort[16];
            keys = new bool[16];

            Opcode = 0;
            SP = 0;
            I = 0;

            PC = 0x200; // Most Chip-8 programs start at location 0x200

            byte[] buffer = File.ReadAllBytes(filePath);
            for (int i = 0; i < buffer.Length; i++)
            {
                Memory[0x200 + i] = buffer[i];
            }

            LoadFonts();
        }

        private void LoadFonts()
        {
            fonts.CopyTo(Memory, 0x0);
        }

        public void KeyUp(byte key)
        {
            keys[key] = false;
        }

        public void KeyDown(byte key)
        {
            keys[key] = true;
        }

        public void Cycle()
        {
            Opcode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            PC += 2;

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

                case 0x9000:
                    Ins_9XY0();
                    break;

                case 0xA000:
                    Ins_ANNN();
                    break;

                case 0xB000:
                    Ins_BNNN();
                    break;

                default:
                    Console.WriteLine($"error: Invalid OpCode: {Opcode:X4} @ PC = 0x{PC:X3}");
                    break;
            }

            if ((counter % 10) == 10)
            {
                if (DelayTimer > 0) { DelayTimer--; }
            }

            counter++;
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        private void Ins_00E0()
        {
            Vram = new byte[32 * 64];
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

        /// <summary>
        /// Skips the next instruction if VX does not equal VY.
        /// </summary>
        private void Ins_9XY0()
        {
            byte x = (byte)((Opcode & 0x0F00) >> 8);
            byte y = (byte)((Opcode & 0x00F0) >> 4);

            if (V[x] != V[y])
            {
                PC += 2;
            }
        }

        /// <summary>
        /// Sets I to the address NNN.
        /// </summary>
        private void Ins_ANNN()
        {
            I = (ushort)(Opcode & 0x0FFF);
            PC += 2;
        }

        /// <summary>
        /// Jumps to the address NNN plus V0.
        /// </summary>
        private void Ins_BNNN()
        {
            PC = (ushort)((ushort)(Opcode & 0x0FFF) + V[0]);
        }

        /// <summary>
        /// Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and KK.
        /// </summary>
        private void Ins_CXKK()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void Ins_DXYN()
        {

        }

        /// <summary>
        /// Sets the delay timer to VX.
        /// </summary>
        private void Ins_FX15()
        {
            DelayTimer = V[(Opcode & 0x0F00) >> 8];
            PC += 2;
        }

        /// <summary>
        /// Sets the sound timer to VX.
        /// </summary>
        private void Ins_FX18()
        {
            SoundTimer = V[(Opcode & 0x0F00) >> 8];
            PC += 2;
        }

        /// <summary>
        /// Adds VX to I. VF is not affected.
        /// </summary>
        private void Ins_FX1E()
        {
            I += V[(Opcode & 0x0F00) >> 8];
            PC += 2;
        }
    }
}

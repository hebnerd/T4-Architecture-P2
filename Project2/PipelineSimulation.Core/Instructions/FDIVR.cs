﻿namespace PipelineSimulation.Core.Instructions
{
    public class FDIVR : Instruction
    {
        public override ushort OpCode => 0x1D;
        public override int Cycles { get; set; } = 11;
        public override bool WritesToRegister => false;
        public override bool UsesRegister => false;

        public FDIVR(CPU cpuref) : base(cpuref)
        {

        }

        public override ushort Execute(ushort operand)
        {
            var i1 = GetRegister1Code(operand);
            var i2 = GetRegister2Code(operand);

            cpu.FPU.Div(i1, i2);

            return 0;
        }

        public override string ToText(ushort operand)
        {
            return $"FDIVR ST({GetRegister1Code(operand)}) ST({GetRegister2Code(operand)})";
        }

        public override string ToString()
        {
            return "FDIVR";
        }
    }
}
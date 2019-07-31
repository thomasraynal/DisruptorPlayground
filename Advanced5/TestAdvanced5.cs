using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DisruptorPlayground.Advanced5
{
    [StructLayout(LayoutKind.Auto)]
    public struct AlignedDoubleAuto
    {
        public byte B;
        public double D;
        public int I;
    }

    public struct AlignedDoubleManaged
    {
        public byte B;
        public string C;
        public double D;
        public int I;
    }


    public struct AlignedDouble
    {
        //1
        public byte B;
        //7 (padding)
        //8
        public double D;
        //4
        public int I;
        //4 (padding)

    }

    public struct AlignedInt
    {
        public int I;
        public byte B;
        //3 (padding)
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DiscriminatedUnion
    {
        [FieldOffset(0)]
        public bool Bool;
        [FieldOffset(0)]
        public byte Byte;
        [FieldOffset(0)]
        public int Integer;
    }

    [TestFixture]
    public class TestAdvanced5
    {
        [Test]
        public void TestStruct()
        {
            var s = new DiscriminatedUnion();
            s.Byte = 1;
            s.Integer = 5;

            var d = Unsafe.SizeOf<AlignedInt>();

            var a = Unsafe.SizeOf<AlignedDouble>();
            var b = Unsafe.SizeOf<AlignedDoubleAuto>();
            var c = Unsafe.SizeOf<AlignedDoubleManaged>();
        }
    }
}

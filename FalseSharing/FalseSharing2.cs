using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DisruptorPlayground.FalseSharing
{
    public interface IMessage
    {
        void Do();
    }

    [ProtoContract]
    [ProtoInclude(7, typeof(MessageA))]
    public abstract class Base : IMessage
    {
        //protected Base()
        //{
        //    Id = Guid.NewGuid();
        //}

        public void Do()
        {

        }

        [ProtoMember(2)]
        public string Data1 { get; set; }

        [ProtoMember(1)]
        public Guid Id { get; set; }
    }

    [ProtoContract]
    public class MessageA : Base
    {

        [ProtoMember(3)]
        public string Data2 { get; set; }
    }

    public struct MessageB : IMessage
    {
        public void Do()
        {

        }
    }

    [TestFixture]
    public class TestFalseSharing3
    {
        [Params(100_000_000)]
        public int N;
        private List<MessageA> _objects;
        private List<MessageB> _structs;
        private List<IMessage> _interfacesClass;
        private List<IMessage> _interfacesStruct;

        [GlobalSetup]
        public void Setup()
        {

            
            _structs = Enumerable.Range(0, N).Select(_ => new MessageB()).ToList();
            _interfacesClass = _objects.Cast<IMessage>().ToList();
            _interfacesClass = _objects.Cast<IMessage>().ToList();
        }


        public readonly struct S
        {

            public S(int i)
            {
                I = i;
            }

            public int I { get; }
        }

        [Test]
        public void ReadOnly()
        {


            var s = new S(10);
          
            var S2 = s;

            Console.WriteLine(S2.I);

            //Your code goes here



        }

        [Test]
        public void Class()
        {
            var msg = new MessageA
            {
                Data1 = "1",
                Data2 = "2"
            };

            using (var stream = new MemoryStream())
            {
          
                    Serializer.Serialize(stream, msg);

                var msg2 = Serializer.Deserialize<MessageA>(stream);

            }
     
        }

    }
}

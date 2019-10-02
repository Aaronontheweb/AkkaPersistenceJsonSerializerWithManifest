using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Serialization;
using Akka.Util;

namespace Petabridge.App
{
    public class JsonSerializerWithManifest : SerializerWithStringManifest
    {
        private readonly NewtonSoftJsonSerializer _underlying;

        public JsonSerializerWithManifest(ExtendedActorSystem system) : base(system)
        {
            _underlying = new NewtonSoftJsonSerializer(system, NewtonSoftJsonSerializerSettings.Default);
        }

        public override object FromBinary(byte[] bytes, string manifest)
        {
            var type = Type.GetType(manifest);
            return _underlying.FromBinary(bytes, type);
        }

        public override string Manifest(object o)
        {
            return o.GetType().TypeQualifiedName();
        }

        public override byte[] ToBinary(object obj)
        {
            return _underlying.ToBinary(obj);
        }
    }
}

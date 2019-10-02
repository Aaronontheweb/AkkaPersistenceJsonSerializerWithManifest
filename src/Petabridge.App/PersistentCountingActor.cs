// -----------------------------------------------------------------------
// <copyright file="PersistentCountingActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015 - 2019 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Persistence;

namespace Petabridge.App
{
    public class PersistentCountingActor : ReceivePersistentActor
    {
        public class MyVal
        {
            public MyVal(int val)
            {
                Val = val;
            }

            public int Val { get; }
        }

        public class GetSum
        {
            public static GetSum Instance = new GetSum();

            private GetSum()
            {
            }
        }

        public PersistentCountingActor(string persistenceId)
        {
            PersistenceId = persistenceId;

            Recover<MyVal>(v => { _currentSum += v.Val; });
            Recover<SnapshotOffer>(s =>
            {
                if (s.Snapshot is int i)
                {
                    _currentSum = i;
                }
            });

            Command<MyVal>(v =>
            {
                Persist(v, val => { _currentSum += val.Val; });
            });

            Command<GetSum>(_ => Sender.Tell(_currentSum));
        }

        private int _currentSum = 0;

        public override string PersistenceId { get; }
    }
}
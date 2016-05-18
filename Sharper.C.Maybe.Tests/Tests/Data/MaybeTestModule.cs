using Sharper.C.Data;
using Sharper.C.Testing;
using Sharper.C.Testing.Laws;
using Sharper.C.Testing.Xunit;
using Sharper.C.Instances;
using static Sharper.C.Testing.Arbitraries.SystemArbitrariesModule;
using static Sharper.C.Testing.Arbitraries.MaybeArbitrariesModule;

namespace Sharper.C.Tests.Data
{
    public static class MaybeTestModule
    {
        [Invariant]
        public static Invariant Obeys_EqualityLaws()
        =>  EqualityLaws.For
              ( AnyMaybe(AnyBool)
              , default(EqMaybe<bool, EqBool>)
              );

        [Invariant]
        public static Invariant Obeys_HashingLaws()
        =>  HashingLaws.For(AnyMaybe(AnyBool));

        [Invariant]
        public static Invariant Obeys_MonadLaws()
        =>  MonadLaws.For
              ( Maybe.Just<bool>
              , f => m => m.Map(f)
              , f => m => m.FlatMap(f)
              , (x, y) => x == y
              , AnyMaybe(AnyBool)
              , AnyFunc1<bool, Maybe<bool>>(AnyMaybe(AnyBool))
              , AnyFunc1<bool, bool>(AnyBool)
              , AnyBool
              );
    }
}

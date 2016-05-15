using System;
using FsCheck;
using Sharper.C.Data;

namespace Sharper.C.Testing.Arbitraries
{
    public static class MaybeArbitraryModule
    {
        public static Arbitrary<Maybe<A>> AnyMaybe<A>(Arbitrary<A> arbA)
        =>  Arb.From
              ( Gen.Frequency
                  ( Tuple.Create(9, AnyJust(arbA).Generator)
                  , Tuple.Create(1, Gen.Constant(Maybe.Nothing<A>()))
                  )
              , x => x.IsJust ? new[] {Maybe.Nothing<A>()} : new Maybe<A>[] {}
              );

        public static Arbitrary<Maybe<A>> AnyJust<A>(Arbitrary<A> arbA)
        =>  Arb.From(arbA.Generator.Select(Maybe.Just));
    }
}

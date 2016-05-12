using Sharper.C.Data;

namespace Sharper.C.Instances
{
    public struct EqMaybe<A, EqA>
      : Eq<Maybe<A>>
      where EqA : Eq<A>
    {
        public EqMaybe(EqA _eqA = default(EqA))
        {
        }

        public bool Equal(Maybe<A> x, Maybe<A> y)
        =>  (x.IsNothing && y.IsNothing)
            ||
            ( from a0 in x
              from a1 in y
              select default(EqA).Equal(a0, a1)
            )
            .ValueOr(() => false);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharper.C.Data
{

using static EqModule;
using static ProductModule;

public static class MaybeModule
{
    public struct Maybe<A>
    {
        private readonly A value;

        internal Maybe(A value)
        {
            IsJust = true;
            this.value = value;
        }

        public B Cata<B>(Func<B> nothing, Func<A, B> just)
        =>  IsJust ? just(value) : nothing();

        // --

        public bool IsJust { get; }

        public bool IsNothing
        =>  !IsJust;

        public Maybe<B> Map<B>(Func<A, B> f)
        =>  IsJust ? new Maybe<B>(f(value)) : default(Maybe<B>);

        public Maybe<B> FlatMap<B>(Func<A, Maybe<B>> f)
        =>  IsJust ? f(value) : default(Maybe<B>);

        public Maybe<A> FlatMapForEffect<B>(Func<A, Maybe<B>> f)
        =>  FlatMap(a => f(a).Map(b => a));

        public Maybe<B> Apply<B>(Maybe<Func<A, B>> mf)
        =>  mf.FlatMap<B>(Map);

        public Maybe<C> ZipWith<B, C>(Maybe<B> mb, Func<A, B, C> f)
        =>  FlatMap(a => mb.Map(b => f(a, b)));

        public Maybe<And<A, B>> Zip<B>(Maybe<B> mb)
        =>  ZipWith(mb, MkAnd);

        public IEnumerable<Maybe<B>> Traverse<B>(Func<A, IEnumerable<B>> f)
        =>  IsJust
            ? f(value).Select(Just)
            : new Maybe<B>[] {};

        public IEnumerable<A> ToSeq()
        =>  IsJust ? new[] {value} : new A[] {};

        public Maybe<A> Where(Func<A, bool> f)
        =>  FlatMap(a => f(a) ? new Maybe<A>(a) : default(Maybe<A>));

        public Maybe<B> Select<B>(Func<A, B> f)
        =>  Map(f);

        public Maybe<C> SelectMany<B ,C>(Func<A, Maybe<B>> f, Func<A, B, C> g)
        =>  FlatMap(a => f(a).Map(b => g(a, b)));

        public Maybe<C> Join<B, C, _>
        ( Maybe<B> mb
        , Func<A, _> __
        , Func<B, _> ___
        , Func<A, B, C> f
        )
        =>  ZipWith(mb, f);

        public Maybe<A> Or(Func<Maybe<A>> ma)
        =>  IsJust ? this : ma();

        public A ValueOr(Func<A> a)
        =>  IsJust ? value : a();

        public bool Equal(Maybe<A> ma, Eq<A> eqA)
        =>  EqMaybe(eqA).Equal(this, ma);
    }

    private sealed class InstanceEqMaybe<A>
    : Eq<Maybe<A>>
    {
        private readonly Eq<A> EqA;

        public InstanceEqMaybe(Eq<A> eqA)
        {
            EqA = eqA;
        }

        public bool Equal(Maybe<A> x, Maybe<A> y)
        =>  x.IsNothing && y.IsNothing
            || x.FlatMap
               ( a0 => y.Map(a1 => EqA.Equal(a0, a1))
               )
               . ValueOr(() => false);
    }

    public static Maybe<A> Just<A>(A a)
    =>  new Maybe<A>(a);

    public static Maybe<A> Nothing<A>()
    =>  default(Maybe<A>);

    public static Maybe<A> Pure<A>(A a)
    =>  Just(a);

    public static Maybe<B> Ap<A, B>(this Maybe<Func<A, B>> mf, Maybe<A> ma)
    =>  mf.FlatMap(ma.Map);

    public static IEnumerable<Maybe<A>> Sequence<A>(Maybe<IEnumerable<A>> msa)
    =>  msa.Traverse(x => x);

    public static Eq<Maybe<A>> EqMaybe<A>(Eq<A> eqA)
    =>  new InstanceEqMaybe<A>(eqA);

    public static Maybe<B> MaybeGet<A, B>(this IDictionary<A, B> d, A a)
    {
        B b;
        return d.TryGetValue(a, out b) ? Just(b) : Nothing<B>();
    }
}

}

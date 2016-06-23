using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharper.C.Data
{

    public struct Maybe<A>
      : IEquatable<Maybe<A>>
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

        public IEnumerable<Maybe<B>> Traverse<B>(Func<A, IEnumerable<B>> f)
        =>  IsJust
            ? f(value).Select(Maybe.Just)
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

        public Maybe<A> Or(Maybe<A> ma)
        =>  IsJust ? this : ma;

        public A ValueOr(Func<A> a)
        =>  IsJust ? value : a();

        public A ValueOr(A a)
        =>  IsJust ? value : a;

        public A ValueOrThrow
        {   get
            {   if (IsJust) return value;
                throw new NullReferenceException("Maybe.Nothing");
            }
        }

        public bool Equals(Maybe<A> x)
        =>  (IsNothing && x.IsNothing)
            || (IsJust && x.IsJust && value.Equals(x.value));

        public override bool Equals(object obj)
        =>  obj is Maybe<A> && Equals((Maybe<A>)obj);

        public override string ToString()
        =>  IsJust ? $"Just({value})" : "Nothing";

        public override int GetHashCode()
        =>  IsJust ? value.GetHashCode() ^ 379 : 0;

        public static bool operator==(Maybe<A> x, Maybe<A> y)
        =>  x.Equals(y);

        public static bool operator!=(Maybe<A> x, Maybe<A> y)
        =>  !x.Equals(y);

        public static Maybe<A> operator|(Maybe<A> x, Maybe<A> y)
        =>  x.Or(y);
    }

    public static class Maybe
    {
        public static Maybe<A> Just<A>(A a)
        =>  new Maybe<A>(a);

        public static Maybe<A> Nothing<A>()
        =>  default(Maybe<A>);

        public static Maybe<A> Pure<A>(A a)
        =>  Just(a);

        public static Maybe<A> When<A>(bool when, Func<A> value)
        =>  when ? Just(value()) : Nothing<A>();

        public static Maybe<A> When<A>(bool when, A value)
        =>  when ? Just(value) : Nothing<A>();

        public static Maybe<A> FromNullable<A>(A? a)
          where A : struct
        =>  When(a.HasValue, () => a.Value);

        public static Maybe<A> FromReference<A>(A a)
          where A : class
        =>  When(a != null, a);

        public static Maybe<A> WhenType<A>(object x)
        =>  When(x is A, (A) x);

        public static A? ToNullable<A>(this Maybe<A> ma)
          where A : struct
        =>  ma.Cata(() => new A?(), a => a);

        public static A ToUnsafeReference<A>(this Maybe<A> ma)
          where A : class
        =>  ma.ValueOr((A)null);

        public static Maybe<B> Ap<A, B>(this Maybe<Func<A, B>> mf, Maybe<A> ma)
        =>  mf.FlatMap(ma.Map);

        public static Maybe<A> Join<A>(this Maybe<Maybe<A>> m)
        =>  m.ValueOr(Nothing<A>());

        public static IEnumerable<Maybe<A>> Sequence<A>
          ( this Maybe<IEnumerable<A>> msa
          )
        =>  msa.Traverse(x => x);

        public static Maybe<IEnumerable<A>> Sequence<A>
          ( this IEnumerable<Maybe<A>> sma
          )
        =>  sma.Aggregate
              ( Just(Enumerable.Empty<A>())
              , (msa, ma) => ma.ZipWith(msa, (a, sa) => new[] {a}.Concat(sa))
              );

        public static Maybe<IEnumerable<B>> Traverse<A, B>
          ( this IEnumerable<A> ma
          , Func<A, Maybe<B>> f
          )
        =>  ma.Select(f).Sequence();

        public static Maybe<B> MaybeGet<A, B>(this IDictionary<A, B> d, A a)
        {
            B b;
            return d.TryGetValue(a, out b) ? Just(b) : Nothing<B>();
        }

        public static Maybe<A> MaybeFirst<A>(this IEnumerable<A> xs)
        {
            try
            {   return Just(xs.First());
            }
            catch (InvalidOperationException)
            {   return Nothing<A>();
            }
        }

        public static Maybe<A> Recover<A>
          ( Func<A> run
          , Func<Exception, bool> ex = null
          )
        {   try
            {   return Just(run());
            }
            catch (Exception e)
            {   if (ex == null || ex(e))
                {   return Nothing<A>();
                }
                else
                {   throw;
                }
            }
        }
    }
}
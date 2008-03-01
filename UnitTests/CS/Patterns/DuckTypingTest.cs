using System;

using NUnit.Framework;

using BLToolkit.TypeBuilder;
using BLToolkit.Patterns;

namespace Patterns
{
	[TestFixture]
	public class DuckTypingTest
	{
		public DuckTypingTest()
		{
			TypeFactory.SaveTypes = true;
		}

		public interface TestInterface
		{
			int  Method(int value);
			void Method(int v1, out int v2);

			int Prop { get; }

			event EventHandler Event;

			void CallEvent();
		}

		public interface TestInterface2
		{
			int  Method(int value);
			void I2Method(int v1, out int v2);
		}

		public class TestClass
		{
			public static int Field;

			public int Method(int value)
			{
				return value + 2;
			}

			public void Method(int v1, out int v2)
			{
				v2 = v1;
			}

			public void I2Method(int v1, out int v2)
			{
				v2 = v1;
			}

			public int Prop
			{
				get { return 22; }
			}

			public event EventHandler Event;

			public void CallEvent()
			{
				Event(this, EventArgs.Empty);
			}
		}

		[Test]
		public void Test()
		{
			TestInterface   duck = DuckTyping.Implement<TestInterface> (new TestClass());
			TestInterface   same = DuckTyping.Implement<TestInterface> (duck);
			TestInterface2 duck2 = DuckTyping.Implement<TestInterface2>(same);

			Assert.AreSame(duck, same);

			int value;
			duck.Method(33, out value);

			Assert.AreEqual(33, value);
			Assert.AreEqual(42, duck.Method(40));
			Assert.AreEqual(22, duck.Prop);

			duck.Event += duck_Event;

			duck.CallEvent();

			Assert.AreEqual(55, eventValue);

			duck2.I2Method(33, out value);

			Assert.AreEqual(33, value);
			Assert.AreEqual(42, duck2.Method(40));
		}

		int eventValue;

		void duck_Event(object sender, EventArgs e)
		{
			eventValue = 55;
		}

		public class Child1 : TestClass
		{
			public new int Method(int value)
			{
				return value + 5;
			}
			
		}

		public class Child2 : TestClass
		{
			public new int Method(int value)
			{
				return value + 10;
			}
			
		}

		[Test]
		public void BulkTest()
		{
			TestInterface[] ducks  = DuckTyping.Implement<TestInterface, TestClass> (new Child1(), new Child2());

			Assert.IsNotEmpty(ducks);
			Assert.AreEqual(42, ducks[0].Method(40));
			Assert.AreEqual(42, ducks[1].Method(40));
		}

		[Test]
		public void BulkTest2()
		{
			TestInterface[] ducks = DuckTyping.Implement<TestInterface>(new Child1(), new Child2());

			Assert.IsNotEmpty(ducks);
			Assert.AreEqual(45, ducks[0].Method(40));
			Assert.AreEqual(50, ducks[1].Method(40));
		}
		
		[Test]
		public void InheritanceTest()
		{
			TestInterface duck1 = DuckTyping.Implement<TestInterface> (new Child1());
			TestInterface duck2 = DuckTyping.Implement<TestInterface> (new Child2());

			Assert.AreNotSame(duck1, duck2);
			Assert.AreEqual(45, duck1.Method(40));
			Assert.AreEqual(50, duck2.Method(40));

		}

		[Test]
		public void InheritanceTest2()
		{
			TestInterface duck1 = DuckTyping.Implement<TestInterface, TestClass> (new Child1());
			TestInterface duck2 = DuckTyping.Implement<TestInterface, TestClass> (new Child2());

			Assert.AreNotSame(duck1, duck2);
			Assert.AreEqual(42, duck1.Method(40));
			Assert.AreEqual(42, duck2.Method(40));
		}

		public class StaticClass
		{
			public static int Method(int value)
			{
				return value + 3;
			}
		}

		[Test]
		public void StaticTest()
		{
			DuckTyping.AllowStaticMembers = true;
			TestInterface duck = DuckTyping.Implement<TestInterface, StaticClass> (new StaticClass());

			Assert.AreEqual(43, duck.Method(40));
		}

		public struct TestStruct
		{
			public int Method(int value)
			{
				return value + 3;
			}
		}

		[Test]
		public void StructTest()
		{
			DuckTyping.AllowStaticMembers = true;
			TestInterface duck = DuckTyping.Implement<TestInterface> (new TestStruct());

			Assert.AreEqual(43, duck.Method(40));
		}


		public interface GenericInterface<T>
		{
			T Method(T value);
		}

		public class GenericClass<T>
		{
			public T Method(T value)
			{
				return value;
			}

			public void I2Method(int v1, out int v2)
			{
				v2 = v1 + 2;
			}
		}

		[Test]
		public void GenericInterfaceTest()
		{
			GenericClass<int> o = new GenericClass<int>();
			GenericInterface<int> duck  = DuckTyping.Implement<GenericInterface<int>> (o);
			TestInterface2        duck2 = DuckTyping.Implement<TestInterface2> (o);

			Assert.AreEqual(40, duck .Method(40));
			Assert.AreEqual(40, duck2.Method(40));

			int value;
			duck2.I2Method (33, out value);
			Assert.AreEqual(35, value);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void InvalidArgTest1()
		{
			TestInterface o = null;

			TestInterface duck1 = DuckTyping.Implement<TestInterface>(o);
			TestInterface duck2 = (TestInterface)DuckTyping.Implement(typeof(TestInterface), o);
		}

		interface NonPublicInterface
		{
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void InvalidArgTest2()
		{
			NonPublicInterface duck  = (NonPublicInterface) DuckTyping.Implement(typeof(NonPublicInterface), new TestClass());
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void InvalidArgTest3()
		{
			Child1 duck  = (Child1) DuckTyping.Implement(typeof(Child1), new Child2());
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void InvalidArgTest4()
		{
			TestInterface duck  = (TestInterface)DuckTyping.Implement(typeof(TestInterface), typeof(TestInterface), new TestClass());
		}

	}
}
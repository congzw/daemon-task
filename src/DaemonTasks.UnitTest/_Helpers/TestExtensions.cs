using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Common
{
    public static class TestExtensions
    {
        public static void ShouldThrows<T>(this Action action) where T : Exception
        {
            AssertHelper.ShouldThrows<T>(action);
        }

        public static object ShouldNull(this object value)
        {
            AssertHelper.WriteLineForShouldBeNull(value);
            Assert.IsNull(value);
            return value;
        }

        public static object ShouldNotNull(this object value)
        {
            AssertHelper.WriteLineForShouldBeNotNull(value);
            Assert.IsNotNull(value);
            return value;
        }
        
        public static object ShouldEqual(this object value, object expectedValue)
        {
            string message = string.Format("Should {0} equals {1}", value, expectedValue);
            Assert.AreEqual(expectedValue, value, message.WithKoPrefix());
            AssertHelper.WriteLineOk(message);
            return value;
        }

        public static object ShouldNotEqual(this object value, object expectedValue)
        {
            string message = string.Format("Should {0} not equals {1}", value, expectedValue);
            Assert.AreNotEqual(expectedValue, value, message.WithKoPrefix());
            AssertHelper.WriteLineOk(message);
            return value;
        }
        
        public static object ShouldSame(this object value, object expectedValue)
        {
            if (value == null || expectedValue == null)
            {
                Assert.AreNotSame(expectedValue, value);
                return value;
            }
            string message = string.Format("Should Same [{0}] => <{1}> : <{2}>", value.GetType().Name, value.GetHashCode(), expectedValue.GetHashCode());
            Assert.AreSame(expectedValue, value, message.WithKoPrefix());
            AssertHelper.WriteLine(message.WithOkPrefix());
            return value;
        }

        public static object ShouldNotSame(this object value, object expectedValue)
        {
            if (value == null || expectedValue == null)
            {
                Assert.AreNotSame(expectedValue, value);
                return value;
            }
            string message = string.Format("Should Not Same [{0}] => <{1}> : <{2}>", value.GetType().Name, value.GetHashCode(), expectedValue.GetHashCode());
            Assert.AreNotSame(expectedValue, value, message.WithKoPrefix());
            AssertHelper.WriteLine(message.WithOkPrefix());
            return value;
        }

        public static void ShouldTrue(this bool result)
        {
            AssertHelper.WriteLineForShouldBeTrue(result);
            Assert.IsTrue(result);
        }

        public static void ShouldFalse(this bool result)
        {
            AssertHelper.WriteLineForShouldBeFalse(result);
            Assert.IsFalse(result);
        }
        
        public static IEnumerable<T> ShouldEmpty<T>(this IEnumerable<T> values)
        {
            Assert.IsNotNull(values);
            Assert.AreEqual(0, values.Count());
            return values;
        }

        public static object LogHashCode(this object value)
        {
            string message = string.Format("{0} <{1}>", value.GetHashCode(), value.GetType().Name);
            AssertHelper.WriteLine(message);
            return value;
        }

        public static object LogHashCodeWiths(this object value, object value2)
        {
            string message = string.Format("{0} <{1}> {2} {3}<{4}>", value.GetHashCode(), value.GetType().Name, value == value2 ? "==" : "!=", value2.GetHashCode(), value2.GetType().Name);
            AssertHelper.WriteLine(message);
            return value;
        }

        public static object Log(this object value)
        {
            if (value == null)
            {
                Debug.WriteLine("null");
            }

            if (value is string)
            {
                Debug.WriteLine(value);
                return value;
            }

            var items = value as IEnumerable;
            if (items != null)
            {
                foreach (var item in items)
                {
                    Debug.WriteLine(item);
                }
                return value;
            }
            Debug.WriteLine(value);
            return value;
        }

        public static string WithOkPrefix(this string value)
        {
            return AssertHelper.PrefixOk(value);
        }
        public static string WithKoPrefix(this string value)
        {
            return AssertHelper.PrefixKo(value);
        }
        public static string WithPrefix(this string value, bool isOk = true)
        {
            return AssertHelper.PrefixKo(value);
        }
        public static string ObjectInfo(this object obj)
        {
            return string.Format("<{0},{1}>", obj.GetType().Name, obj.GetHashCode());
        }
    }
}

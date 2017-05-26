using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Diagnostics;
using Abarsoft.Helpers;

namespace Helpers
{
    public class UnitTest
    {
        public static long Convertidor()
        {
            var stopWatch = new Stopwatch();
            var convertidor = new Convertidor();

            stopWatch.Start();
            Expression<Func<int>> e2 = () => 1;
            XElement xml2 = convertidor.ToXElement(e2);
            Expression result2 = convertidor.ToExpression(xml2);

            Expression<Func<ExpressionType>> e3 = () => ExpressionType.Add;
            XElement xml3 = convertidor.ToXElement(e3);
            Expression result3 = convertidor.ToExpression(xml3);

            Expression<Func<bool>> e4 = () => true;
            XElement xml4 = convertidor.ToXElement(e4);
            Expression result4 = convertidor.ToExpression(xml4);

            Expression<Func<decimal, decimal>> e5 = d => d + 1m;
            XElement xml5 = convertidor.ToXElement(e5);
            Expression result5 = convertidor.ToExpression(xml5);

            Expression<Func<decimal, decimal>> e6 = d => d + 1m;
            XElement xml6 = convertidor.ToXElement(e6);
            Expression result6 = convertidor.ToExpression(xml6);

            Expression<Func<string, int>> e7 = s => int.Parse(s);
            XElement xml7 = convertidor.ToXElement(e7);
            Expression result7 = convertidor.ToExpression(xml7);

            Expression<Func<string, string>> e8 = s => s.PadLeft(4);
            XElement xml8 = convertidor.ToXElement(e8);
            Expression result8 = convertidor.ToExpression(xml8);

            Expression<Func<string, int>> e9 = s => Foo<string, int>(s, 1);
            XElement xml9 = convertidor.ToXElement(e9);
            Expression result9 = convertidor.ToExpression(xml9);

            Expression<Func<string, char[]>> e10 = s => s.Where(c => c != 'a').ToArray();
            XElement xml10 = convertidor.ToXElement(e10);
            Expression result10 = convertidor.ToExpression(xml10);

            Expression<Func<string, char[]>> e11 =
                s =>
                    (from c in s
                     where c != 'a'
                     select (char)(c + 1)).ToArray();
            XElement xml11 = convertidor.ToXElement(e11);
            Expression result11 = convertidor.ToExpression(xml11);

            Expression<Func<int, IEnumerable<Order[]>>> e12 =
             n =>
                 from c in GetCustomers()
                 where c.ID < n
                 select c.Orders.ToArray();
            XElement xml12 = convertidor.ToXElement(e12);
            Expression result12 = convertidor.ToExpression(xml12);

            Expression<Func<List<int>>> e13 = () => new List<int>() { 1, 2, 3 };
            XElement xml13 = convertidor.ToXElement(e13);
            Expression result13 = convertidor.ToExpression(xml13);

            Expression<Func<List<List<int>>>> e14 = () => new List<List<int>>() { new List<int>() { 1, 2, 3 }, new List<int>() { 2, 3, 4 }, new List<int>() { 3, 4, 5 } };
            XElement xml14 = convertidor.ToXElement(e14);
            Expression result14 = convertidor.ToExpression(xml14);

            Expression<Func<Customer>> e15 = () => new Customer() { Name = "Bob", Orders = { new Order() { OrderInfo = { TrackingNumber = 123 }, ID = "12", Quantity = 2 } } };
            XElement xml15 = convertidor.ToXElement(e15);
            Expression result15 = convertidor.ToExpression(xml15);

            Expression<Func<bool, int>> e16 = b => b ? 1 : 2;
            XElement xml16 = convertidor.ToXElement(e16);
            Expression result16 = convertidor.ToExpression(xml16);

            Expression<Func<int, int[]>> e17 = n => new[] { n };
            XElement xml17 = convertidor.ToXElement(e17);
            Expression result17 = convertidor.ToExpression(xml17);

            Expression<Func<int, int[]>> e18 = n => new int[n];
            XElement xml18 = convertidor.ToXElement(e18);
            Expression result18 = convertidor.ToExpression(xml18);

            Expression<Func<object, string>> e19 = o => o as string;
            XElement xml19 = convertidor.ToXElement(e19);
            Expression result19 = convertidor.ToExpression(xml19);

            Expression<Func<object, bool>> e20 = o => o is string;
            XElement xml20 = convertidor.ToXElement(e20);
            Expression result20 = convertidor.ToExpression(xml20);

            Expression<Func<IEnumerable<string>>> e21 = () => from m in typeof(string).GetMethods()
                                                              where !m.IsStatic
                                                              group m by m.Name into g
                                                              select g.Key + g.Count().ToString();

            XElement xml21 = convertidor.ToXElement(e21);
            Expression result21 = convertidor.ToExpression(xml21);
            
            Expression<Func<IEnumerable<int>>> e22 = () => from a in Enumerable.Range(1, 13)
                                                           join b in Enumerable.Range(1, 13) on 4 * a equals b
                                                           from c in Enumerable.Range(1, 13)
                                                           join d in Enumerable.Range(1, 13) on 5 * c equals d
                                                           from e in Enumerable.Range(1, 13)
                                                           join f in Enumerable.Range(1, 13) on 3 * e equals 2 * f
                                                           join g in Enumerable.Range(1, 13) on 2 * (c + d) equals 3 * g
                                                           from h in Enumerable.Range(1, 13)
                                                           join i in Enumerable.Range(1, 13) on 3 * h - 2 * (e + f) equals 3 * i
                                                           from j in Enumerable.Range(1, 13)
                                                           join k in Enumerable.Range(1, 13) on 3 * (a + b) + 2 * j - 2 * (g + c + d) equals k
                                                           from l in Enumerable.Range(1, 13)
                                                           join m in Enumerable.Range(1, 13) on (h + i + e + f) - l equals 4 * m
                                                           where (4 * (l + m + h + i + e + f) == 3 * (j + k + g + a + b + c + d))
                                                           select a + b + c + d + e + f + g + h + i + j + k + l + m;
            XElement xml22 = convertidor.ToXElement(e22);
            Expression result22 = convertidor.ToExpression(xml22);

            Expression<Func<int, int>> e23 = n => ((Func<int, int>)(x => x + 1))(n);
            XElement xml23 = convertidor.ToXElement(e23);
            Expression result23 = convertidor.ToExpression(xml23);

            Expression<Func<IEnumerable<int>>> e24 = () => from x in Enumerable.Range(1, 10)
                                                           from y in Enumerable.Range(1, 10)
                                                           where x < y
                                                           select x * y;
            XElement xml24 = convertidor.ToXElement(e24);
            Expression result24 = convertidor.ToExpression(xml24);

            Expression<Func<DateTime>> e25 = () => new DateTime(10000);
            XElement xml25 = convertidor.ToXElement(e25);
            Expression result25 = convertidor.ToExpression(xml25);

            stopWatch.Stop();

            return stopWatch.ElapsedMilliseconds;
        }

        public static long ExpressionSerializer()
        {
            var stopWatch = new Stopwatch();
            ExpressionSerializer serializer = new ExpressionSerializer();

            stopWatch.Start();
            Expression<Func<int>> e2 = () => 1;
            XElement xml2 = serializer.Serialize(e2);
            Expression result2 = serializer.Deserialize(xml2);

            Expression<Func<ExpressionType>> e3 = () => ExpressionType.Add;
            XElement xml3 = serializer.Serialize(e3);
            Expression result3 = serializer.Deserialize(xml3);

            Expression<Func<bool>> e4 = () => true;
            XElement xml4 = serializer.Serialize(e4);
            Expression result4 = serializer.Deserialize(xml4);

            Expression<Func<decimal, decimal>> e5 = d => d + 1m;
            XElement xml5 = serializer.Serialize(e5);
            Expression result5 = serializer.Deserialize(xml5);

            Expression<Func<decimal, decimal>> e6 = d => d + 1m;
            XElement xml6 = serializer.Serialize(e6);
            Expression result6 = serializer.Deserialize(xml6);

            Expression<Func<string, int>> e7 = s => int.Parse(s);
            XElement xml7 = serializer.Serialize(e7);
            Expression result7 = serializer.Deserialize(xml7);

            Expression<Func<string, string>> e8 = s => s.PadLeft(4);
            XElement xml8 = serializer.Serialize(e8);
            Expression result8 = serializer.Deserialize(xml8);

            Expression<Func<string, int>> e9 = s => Foo<string, int>(s, 1);
            XElement xml9 = serializer.Serialize(e9);
            Expression result9 = serializer.Deserialize(xml9);

            Expression<Func<string, char[]>> e10 = s => s.Where(c => c != 'a').ToArray();
            XElement xml10 = serializer.Serialize(e10);
            Expression result10 = serializer.Deserialize(xml10);

            Expression<Func<string, char[]>> e11 =
                s =>
                    (from c in s
                     where c != 'a'
                     select (char)(c + 1)).ToArray();
            XElement xml11 = serializer.Serialize(e11);
            Expression result11 = serializer.Deserialize(xml11);

            Expression<Func<int, IEnumerable<Order[]>>> e12 =
             n =>
                 from c in GetCustomers()
                 where c.ID < n
                 select c.Orders.ToArray();
            XElement xml12 = serializer.Serialize(e12);
            Expression result12 = serializer.Deserialize(xml12);

            Expression<Func<List<int>>> e13 = () => new List<int>() { 1, 2, 3 };
            XElement xml13 = serializer.Serialize(e13);
            Expression result13 = serializer.Deserialize(xml13);

            Expression<Func<List<List<int>>>> e14 = () => new List<List<int>>() { new List<int>() { 1, 2, 3 }, new List<int>() { 2, 3, 4 }, new List<int>() { 3, 4, 5 } };
            XElement xml14 = serializer.Serialize(e14);
            Expression result14 = serializer.Deserialize(xml14);

            Expression<Func<Customer>> e15 = () => new Customer() { Name = "Bob", Orders = { new Order() { OrderInfo = { TrackingNumber = 123 }, ID = "12", Quantity = 2 } } };
            XElement xml15 = serializer.Serialize(e15);
            Expression result15 = serializer.Deserialize(xml15);

            Expression<Func<bool, int>> e16 = b => b ? 1 : 2;
            XElement xml16 = serializer.Serialize(e16);
            Expression result16 = serializer.Deserialize(xml16);

            Expression<Func<int, int[]>> e17 = n => new[] { n };
            XElement xml17 = serializer.Serialize(e17);
            Expression result17 = serializer.Deserialize(xml17);

            Expression<Func<int, int[]>> e18 = n => new int[n];
            XElement xml18 = serializer.Serialize(e18);
            Expression result18 = serializer.Deserialize(xml18);

            Expression<Func<object, string>> e19 = o => o as string;
            XElement xml19 = serializer.Serialize(e19);
            Expression result19 = serializer.Deserialize(xml19); ;

            Expression<Func<object, bool>> e20 = o => o is string;
            XElement xml20 = serializer.Serialize(e20);
            Expression result20 = serializer.Deserialize(xml20);

            Expression<Func<IEnumerable<string>>> e21 = () => from m in typeof(string).GetMethods()
                                                              where !m.IsStatic
                                                              group m by m.Name into g
                                                              select g.Key + g.Count().ToString();

            XElement xml21 = serializer.Serialize(e21);
            Expression result21 = serializer.Deserialize(xml21);
            
            Expression<Func<IEnumerable<int>>> e22 = () => from a in Enumerable.Range(1, 13)
                                                           join b in Enumerable.Range(1, 13) on 4 * a equals b
                                                           from c in Enumerable.Range(1, 13)
                                                           join d in Enumerable.Range(1, 13) on 5 * c equals d
                                                           from e in Enumerable.Range(1, 13)
                                                           join f in Enumerable.Range(1, 13) on 3 * e equals 2 * f
                                                           join g in Enumerable.Range(1, 13) on 2 * (c + d) equals 3 * g
                                                           from h in Enumerable.Range(1, 13)
                                                           join i in Enumerable.Range(1, 13) on 3 * h - 2 * (e + f) equals 3 * i
                                                           from j in Enumerable.Range(1, 13)
                                                           join k in Enumerable.Range(1, 13) on 3 * (a + b) + 2 * j - 2 * (g + c + d) equals k
                                                           from l in Enumerable.Range(1, 13)
                                                           join m in Enumerable.Range(1, 13) on (h + i + e + f) - l equals 4 * m
                                                           where (4 * (l + m + h + i + e + f) == 3 * (j + k + g + a + b + c + d))
                                                           select a + b + c + d + e + f + g + h + i + j + k + l + m;
            XElement xml22 = serializer.Serialize(e22);
            Expression result22 = serializer.Deserialize(xml22);

            Expression<Func<int, int>> e23 = n => ((Func<int, int>)(x => x + 1))(n);
            XElement xml23 = serializer.Serialize(e23);
            Expression result23 = serializer.Deserialize(xml23);

            Expression<Func<IEnumerable<int>>> e24 = () => from x in Enumerable.Range(1, 10)
                                                           from y in Enumerable.Range(1, 10)
                                                           where x < y
                                                           select x * y;
            XElement xml24 = serializer.Serialize(e24);
            Expression result24 = serializer.Deserialize(xml24);

            Expression<Func<DateTime>> e25 = () => new DateTime(10000);
            XElement xml25 = serializer.Serialize(e25);
            Expression result25 = serializer.Deserialize(xml25);

            stopWatch.Stop();

            return stopWatch.ElapsedMilliseconds;
        }

        public static int Foo<T>(T t)
        {
            return 1;
        }
        public static int Foo<T, U>(T t, U u)
        {
            return 2;
        }

        public static IEnumerable<Customer> GetCustomers()
        {
            return new[] {
                new Customer() { 
                    ID = 0,
                    Name = "Bob",
                    Orders = {
                        new Order() {
                            ID = "0",
                            Quantity = 5
                        },
                        new Order() {
                            ID = "1",
                            Quantity = 123
                        }}},
                new Customer() { 
                    ID = 1,
                    Name = "Dave",
                    Orders = {
                        new Order() {
                            ID = "0",
                            Quantity = 5
                        },
                        new Order() {
                            ID = "2",
                            Quantity = 199
                        }
                    }
                 } 
            };
        }

        public class Customer
        {
            public int ID;
            public string Name { get; set; }
            public List<Order> Orders { get; private set; }
            public Customer()
            {
                Orders = new List<Order>();
            }
        }

        public class Order
        {
            public string ID { get; set; }
            public int Quantity { get; set; }
            public OrderInfo OrderInfo { get; set; }
            public Order()
            {
                OrderInfo = new OrderInfo();
            }
        }

        public class OrderInfo
        {
            public int TrackingNumber { get; set; }
        }
    }
}

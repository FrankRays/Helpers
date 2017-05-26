# Helpers

C# Helpers classes to help you in your projects

##Convertidor

If you want convert Expressions to XElement is easy:

```
var convertidor = new Convertidor();
Expression<Func<int>> e = () => 1;
XElement x = convertidor.ToXElement(e);
Expression result2 = convertidor.ToExpression(x);

or this one ...

```
Expression<Func<int, IEnumerable<Order[]>>> e =
 n =>
	 from c in GetCustomers()
	 where c.ID < n
	 select c.Orders.ToArray();
XElement x = convertidor.ToXElement(e);
Expression result12 = convertidor.ToExpression(x);
```

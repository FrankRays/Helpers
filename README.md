# Helpers

C# Helpers classes to help you in your projects

## Convertidor

If you want convert Expressions to XElement is easy:

```
var convertidor = new Convertidor();
Expression<Func<int>> e = () => 1;
XElement x = convertidor.ToXElement(e);
Expression result2 = convertidor.ToExpression(x);
```

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

## ColeccionSet<T>

```
public class ColeccionSet<T> : IList<T>, IList, ICollection<T>, ICollection, IEnumerable<T>, IEnumerable, IBindingListView, IBindingList, ICancelAddNew, IRaiseItemChangedEvents, INotifyCollectionChanged, IQueryable<T>, IQueryable, IOrderedQueryable<T>, IOrderedQueryable
```

All collections in one, you can use it in Windows Forms inside DataGridView, in WPF in your controls, etc.

```
var c = new ColeccionSet<int>(new List<int> { 1, 2, 3 }).Where(a => a == 1).FirstOrDefault();
Console.WriteLine($@"must be 1 = {c}");
```

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Helpers
{
  public class ExpressionSerializer
  {
      private static readonly Type[] attributeTypes = new[] { typeof(string), typeof(int), typeof(bool), typeof(ExpressionType) };
      private Dictionary<string, ParameterExpression> _parameters = new Dictionary<string, ParameterExpression>();
      private ExpressionSerializationTypeResolver resolver;
      public List<CustomExpressionXmlConverter> Converters { get; private set; }

      public ExpressionSerializer(ExpressionSerializationTypeResolver resolver)
      {
          this.resolver = resolver;
          Converters = new List<CustomExpressionXmlConverter>();
      }

      public ExpressionSerializer()
      {
          this.resolver = new ExpressionSerializationTypeResolver();
          Converters = new List<CustomExpressionXmlConverter>();
      }



      /*
       * SERIALIZATION 
       */

      public XElement Serialize(Expression e)
      {
          return GenerateXmlFromExpressionCore(e);
      }

      private XElement GenerateXmlFromExpressionCore(Expression e)
      {
          if (e == null)
              return null;
          XElement replace = ApplyCustomConverters(e);
          if (replace != null)
              return replace;
          return new XElement(
                      GetNameOfExpression(e),
                      from prop in e.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                      select GenerateXmlFromProperty(prop.PropertyType, prop.Name, prop.GetValue(e, null)));
      }

      private XElement ApplyCustomConverters(Expression e)
      {
          foreach (var converter in Converters)
          {
              XElement result = converter.Serialize(e);
              if (result != null)
                  return result;
          }
          return null;
      }

      private string GetNameOfExpression(Expression e)
      {
          if (e is LambdaExpression)
              return "LambdaExpression";
          return XmlConvert.EncodeName(e.GetType().Name);
      }

      private object GenerateXmlFromProperty(Type propType, string propName, object value)
      {
          if (attributeTypes.Contains(propType))
              return GenerateXmlFromPrimitive(propName, value);
          if (propType.Equals(typeof(object)))
              return GenerateXmlFromObject(propName, value);
          if (typeof(Expression).IsAssignableFrom(propType))
              return GenerateXmlFromExpression(propName, value as Expression);
          if (value is MethodInfo || propType.Equals(typeof(MethodInfo)))
              return GenerateXmlFromMethodInfo(propName, value as MethodInfo);
          if (value is PropertyInfo || propType.Equals(typeof(PropertyInfo)))
              return GenerateXmlFromPropertyInfo(propName, value as PropertyInfo);
          if (value is FieldInfo || propType.Equals(typeof(FieldInfo)))
              return GenerateXmlFromFieldInfo(propName, value as FieldInfo);
          if (value is ConstructorInfo || propType.Equals(typeof(ConstructorInfo)))
              return GenerateXmlFromConstructorInfo(propName, value as ConstructorInfo);
          if (propType.Equals(typeof(Type)))
              return GenerateXmlFromType(propName, value as Type);
          if (IsIEnumerableOf<Expression>(propType))
              return GenerateXmlFromExpressionList(propName, AsIEnumerableOf<Expression>(value));
          if (IsIEnumerableOf<MemberInfo>(propType))
              return GenerateXmlFromMemberInfoList(propName, AsIEnumerableOf<MemberInfo>(value));
          if (IsIEnumerableOf<ElementInit>(propType))
              return GenerateXmlFromElementInitList(propName, AsIEnumerableOf<ElementInit>(value));
          if (IsIEnumerableOf<MemberBinding>(propType))
              return GenerateXmlFromBindingList(propName, AsIEnumerableOf<MemberBinding>(value));
          throw new NotSupportedException(propName);
      }

      private object GenerateXmlFromObject(string propName, object value)
      {
          object result = null;
          if (value is Type)
              result = GenerateXmlFromTypeCore((Type)value);
          if (result == null)
              result = value.ToString();
          return new XElement(propName,
              result);
      }

      private bool IsIEnumerableOf<T>(Type propType)
      {
          if (!propType.IsGenericType)
              return false;
          Type[] typeArgs = propType.GetGenericArguments();
          if (typeArgs.Length != 1)
              return false;
          if (!typeof(T).IsAssignableFrom(typeArgs[0]))
              return false;
          if (!typeof(IEnumerable<>).MakeGenericType(typeArgs).IsAssignableFrom(propType))
              return false;
          return true;
      }

      private IEnumerable<T> AsIEnumerableOf<T>(object value)
      {
          if (value == null)
              return null;
          return (value as IEnumerable).Cast<T>();
      }

      private object GenerateXmlFromElementInitList(string propName, IEnumerable<ElementInit> initializers)
      {
          if (initializers == null)
              initializers = new ElementInit[] { };
          return new XElement(propName,
              from elementInit in initializers
              select GenerateXmlFromElementInitializer(elementInit));
      }

      private object GenerateXmlFromElementInitializer(ElementInit elementInit)
      {
          return new XElement("ElementInit",
              GenerateXmlFromMethodInfo("AddMethod", elementInit.AddMethod),
              GenerateXmlFromExpressionList("Arguments", elementInit.Arguments));
      }

      private object GenerateXmlFromExpressionList(string propName, IEnumerable<Expression> expressions)
      {
          return new XElement(propName,
                  from expression in expressions
                  select GenerateXmlFromExpressionCore(expression));
      }

      private object GenerateXmlFromMemberInfoList(string propName, IEnumerable<MemberInfo> members)
      {
          if (members == null)
              members = new MemberInfo[] { };
          return new XElement(propName,
                 from member in members
                 select GenerateXmlFromProperty(member.GetType(), "Info", member));
      }

      private object GenerateXmlFromBindingList(string propName, IEnumerable<MemberBinding> bindings)
      {
          if (bindings == null)
              bindings = new MemberBinding[] { };
          return new XElement(propName,
              from binding in bindings
              select GenerateXmlFromBinding(binding));
      }

      private object GenerateXmlFromBinding(MemberBinding binding)
      {
          switch (binding.BindingType)
          {
              case MemberBindingType.Assignment:
                  return GenerateXmlFromAssignment(binding as MemberAssignment);
              case MemberBindingType.ListBinding:
                  return GenerateXmlFromListBinding(binding as MemberListBinding);
              case MemberBindingType.MemberBinding:
                  return GenerateXmlFromMemberBinding(binding as MemberMemberBinding);
              default:
                  throw new NotSupportedException(string.Format("Binding type {0} not supported.", binding.BindingType));
          }
      }

      private object GenerateXmlFromMemberBinding(MemberMemberBinding memberMemberBinding)
      {
          return new XElement("MemberMemberBinding",
              GenerateXmlFromProperty(memberMemberBinding.Member.GetType(), "Member", memberMemberBinding.Member),
              GenerateXmlFromBindingList("Bindings", memberMemberBinding.Bindings));
      }


      private object GenerateXmlFromListBinding(MemberListBinding memberListBinding)
      {
          return new XElement("MemberListBinding",
              GenerateXmlFromProperty(memberListBinding.Member.GetType(), "Member", memberListBinding.Member),
              GenerateXmlFromProperty(memberListBinding.Initializers.GetType(), "Initializers", memberListBinding.Initializers));
      }

      private object GenerateXmlFromAssignment(MemberAssignment memberAssignment)
      {
          return new XElement("MemberAssignment",
              GenerateXmlFromProperty(memberAssignment.Member.GetType(), "Member", memberAssignment.Member),
              GenerateXmlFromProperty(memberAssignment.Expression.GetType(), "Expression", memberAssignment.Expression));
      }

      private XElement GenerateXmlFromExpression(string propName, Expression e)
      {
          return new XElement(propName, GenerateXmlFromExpressionCore(e));
      }

      private object GenerateXmlFromType(string propName, Type type)
      {
          return new XElement(propName, GenerateXmlFromTypeCore(type));
      }

      private XElement GenerateXmlFromTypeCore(Type type)
      {
          //vsadov: add detection of VB anon types
          if (type.Name.StartsWith("<>f__") || type.Name.StartsWith("VB$AnonymousType"))
              return new XElement("AnonymousType",
                  new XAttribute("Name", type.AssemblyQualifiedName),
                  from property in type.GetProperties()
                  select new XElement("Property",
                      new XAttribute("Name", property.Name),
                      GenerateXmlFromTypeCore(property.PropertyType)),
                  new XElement("Constructor",
                          from parameter in type.GetConstructors().First().GetParameters()
                          select new XElement("Parameter",
                              new XAttribute("Name", parameter.Name),
                              GenerateXmlFromTypeCore(parameter.ParameterType))
                  ));

          else
          {
              //vsadov: GetGenericArguments returns args for nongeneric types 
              //like arrays no need to save them.
              if (type.IsGenericType)
              {
                  return new XElement("Type",
                                          new XAttribute("Name", type.GetGenericTypeDefinition().AssemblyQualifiedName),
                                          from genArgType in type.GetGenericArguments()
                                          select GenerateXmlFromTypeCore(genArgType));
              }
              else
              {
                  return new XElement("Type", new XAttribute("Name", type.AssemblyQualifiedName));
              }

          }
      }

      private object GenerateXmlFromPrimitive(string propName, object value)
      {
          return new XAttribute(propName, (value == null) ? string.Empty : value);
      }

      private object GenerateXmlFromMethodInfo(string propName, MethodInfo methodInfo)
      {
          if (methodInfo == null)
              return new XElement(propName);
          return new XElement(propName,
                      new XAttribute("MemberType", methodInfo.MemberType),
                      new XAttribute("MethodName", methodInfo.Name),
                      GenerateXmlFromType("DeclaringType", methodInfo.DeclaringType),
                      new XElement("Parameters",
                          from param in methodInfo.GetParameters()
                          select GenerateXmlFromType("Type", param.ParameterType)),
                      new XElement("GenericArgTypes",
                          from argType in methodInfo.GetGenericArguments()
                          select GenerateXmlFromType("Type", argType)));
      }

      private object GenerateXmlFromPropertyInfo(string propName, PropertyInfo propertyInfo)
      {
          if (propertyInfo == null)
              return new XElement(propName);
          return new XElement(propName,
                      new XAttribute("MemberType", propertyInfo.MemberType),
                      new XAttribute("PropertyName", propertyInfo.Name),
                      GenerateXmlFromType("DeclaringType", propertyInfo.DeclaringType),
                      new XElement("IndexParameters",
                          from param in propertyInfo.GetIndexParameters()
                          select GenerateXmlFromType("Type", param.ParameterType)));
      }

      private object GenerateXmlFromFieldInfo(string propName, FieldInfo fieldInfo)
      {
          if (fieldInfo == null)
              return new XElement(propName);
          return new XElement(propName,
                      new XAttribute("MemberType", fieldInfo.MemberType),
                      new XAttribute("FieldName", fieldInfo.Name),
                      GenerateXmlFromType("DeclaringType", fieldInfo.DeclaringType));
      }

      private object GenerateXmlFromConstructorInfo(string propName, ConstructorInfo constructorInfo)
      {
          if (constructorInfo == null)
              return new XElement(propName);
          return new XElement(propName,
                      new XAttribute("MemberType", constructorInfo.MemberType),
                      new XAttribute("MethodName", constructorInfo.Name),
                      GenerateXmlFromType("DeclaringType", constructorInfo.DeclaringType),
                      new XElement("Parameters",
                          from param in constructorInfo.GetParameters()
                          select new XElement("Parameter",
                              new XAttribute("Name", param.Name),
                              GenerateXmlFromType("Type", param.ParameterType))));
      }


      /*
       * DESERIALIZATION 
       */


      public Expression Deserialize(XElement xml)
      {
          _parameters.Clear();
          return ParseExpressionFromXmlNonNull(xml);
      }

      public Expression<TDelegate> Deserialize<TDelegate>(XElement xml)
      {
          Expression e = Deserialize(xml);
          if (e is Expression<TDelegate>)
              return e as Expression<TDelegate>;
          throw new Exception("xml must represent an Expression<TDelegate>");
      }

      private Expression ParseExpressionFromXml(XElement xml)
      {
          if (xml.IsEmpty)
              return null;

          return ParseExpressionFromXmlNonNull(xml.Elements().First());
      }

      private Expression ParseExpressionFromXmlNonNull(XElement xml)
      {
          Expression expression = ApplyCustomDeserializers(xml);
          if (expression != null)
              return expression;
          switch (xml.Name.LocalName)
          {
              case "BinaryExpression":
              case "SimpleBinaryExpression":
              case "LogicalBinaryExpression":
              case "MethodBinaryExpression":
                  return ParseBinaryExpresssionFromXml(xml);
              case "ConstantExpression":
              case "TypedConstantExpression":
                  return ParseConstatExpressionFromXml(xml);
              case "ParameterExpression":
              case "PrimitiveParameterExpressionx00601":
              case "PrimitiveParameterExpression_x0060_1":
              case "TypedParameterExpression":
                  return ParseParameterExpressionFromXml(xml);
              case "LambdaExpression":
                  return ParseLambdaExpressionFromXml(xml);
              case "MethodCallExpression":
              case "MethodCallExpressionN":
              case "InstanceMethodCallExpressionN":
                  return ParseMethodCallExpressionFromXml(xml);
              case "UnaryExpression":
                  return ParseUnaryExpressionFromXml(xml);
              case "MemberExpression":
              case "PropertyExpression":
              case "FieldExpression":
                  return ParseMemberExpressionFromXml(xml);
              case "NewExpression":
                  return ParseNewExpressionFromXml(xml);
              case "ListInitExpression":
                  return ParseListInitExpressionFromXml(xml);
              case "MemberInitExpression":
                  return ParseMemberInitExpressionFromXml(xml);
              case "ConditionalExpression":
              case "FullConditionalExpression":
                  return ParseConditionalExpressionFromXml(xml);
              case "NewArrayExpression":
              case "NewArrayInitExpression":
              case "NewArrayBoundsExpression":
                  return ParseNewArrayExpressionFromXml(xml);
              case "TypeBinaryExpression":
                  return ParseTypeBinaryExpressionFromXml(xml);
              case "InvocationExpression":
                  return ParseInvocationExpressionFromXml(xml);
              default:
                  throw new NotSupportedException(xml.Name.LocalName);
          }
      }

      private Expression ApplyCustomDeserializers(XElement xml)
      {
          foreach (var converter in Converters)
          {
              Expression result = converter.Deserialize(xml);
              if (result != null)
                  return result;
          }
          return null;
      }

      private Expression ParseInvocationExpressionFromXml(XElement xml)
      {
          Expression expression = ParseExpressionFromXml(xml.Element("Expression"));
          var arguments = ParseExpressionListFromXml<Expression>(xml, "Arguments");
          return Expression.Invoke(expression, arguments);
      }

      private Expression ParseTypeBinaryExpressionFromXml(XElement xml)
      {
          Expression expression = ParseExpressionFromXml(xml.Element("Expression"));
          Type typeOperand = ParseTypeFromXml(xml.Element("TypeOperand"));
          return Expression.TypeIs(expression, typeOperand);
      }

      private Expression ParseNewArrayExpressionFromXml(XElement xml)
      {
          Type type = ParseTypeFromXml(xml.Element("Type"));
          if (!type.IsArray)
              throw new Exception("Expected array type");
          Type elemType = type.GetElementType();
          var expressions = ParseExpressionListFromXml<Expression>(xml, "Expressions");
          switch (xml.Attribute("NodeType").Value)
          {
              case "NewArrayInit":
                  return Expression.NewArrayInit(elemType, expressions);
              case "NewArrayBounds":
                  return Expression.NewArrayBounds(elemType, expressions);
              default:
                  throw new Exception("Expected NewArrayInit or NewArrayBounds");
          }
      }

      private Expression ParseConditionalExpressionFromXml(XElement xml)
      {
          Expression test = ParseExpressionFromXml(xml.Element("Test"));
          Expression ifTrue = ParseExpressionFromXml(xml.Element("IfTrue"));
          Expression ifFalse = ParseExpressionFromXml(xml.Element("IfFalse"));
          return Expression.Condition(test, ifTrue, ifFalse);
      }

      private Expression ParseMemberInitExpressionFromXml(XElement xml)
      {
          NewExpression newExpression = ParseNewExpressionFromXml(xml.Element("NewExpression").Element("NewExpression")) as NewExpression;
          var bindings = ParseBindingListFromXml(xml, "Bindings").ToArray();
          return Expression.MemberInit(newExpression, bindings);
      }



      private Expression ParseListInitExpressionFromXml(XElement xml)
      {
          NewExpression newExpression = ParseExpressionFromXml(xml.Element("NewExpression")) as NewExpression;
          if (newExpression == null) throw new Exception("Expceted a NewExpression");
          var initializers = ParseElementInitListFromXml(xml, "Initializers").ToArray();
          return Expression.ListInit(newExpression, initializers);
      }

      private Expression ParseNewExpressionFromXml(XElement xml)
      {
          ConstructorInfo constructor = ParseConstructorInfoFromXml(xml.Element("Constructor"));
          var arguments = ParseExpressionListFromXml<Expression>(xml, "Arguments").ToArray();
          var members = ParseMemberInfoListFromXml<MemberInfo>(xml, "Members").ToArray();
          if (members.Length == 0)
              return Expression.New(constructor, arguments);
          return Expression.New(constructor, arguments, members);
      }

      private Expression ParseMemberExpressionFromXml(XElement xml)
      {
          Expression expression = ParseExpressionFromXml(xml.Element("Expression"));
          MemberInfo member = ParseMemberInfoFromXml(xml.Element("Member"));
          return Expression.MakeMemberAccess(expression, member);
      }

      private MemberInfo ParseMemberInfoFromXml(XElement xml)
      {
          MemberTypes memberType = (MemberTypes)ParseConstantFromAttribute<MemberTypes>(xml, "MemberType");
          switch (memberType)
          {
              case MemberTypes.Field:
                  return ParseFieldInfoFromXml(xml);
              case MemberTypes.Property:
                  return ParsePropertyInfoFromXml(xml);
              case MemberTypes.Method:
                  return ParseMethodInfoFromXml(xml);
              case MemberTypes.Constructor:
                  return ParseConstructorInfoFromXml(xml);
              case MemberTypes.Custom:
              case MemberTypes.Event:
              case MemberTypes.NestedType:
              case MemberTypes.TypeInfo:
              default:
                  throw new NotSupportedException(string.Format("MEmberType {0} not supported", memberType));
          }

      }

      private MemberInfo ParseFieldInfoFromXml(XElement xml)
      {
          string fieldName = (string)ParseConstantFromAttribute<string>(xml, "FieldName");
          Type declaringType = ParseTypeFromXml(xml.Element("DeclaringType"));
          return declaringType.GetField(fieldName);
      }

      private MemberInfo ParsePropertyInfoFromXml(XElement xml)
      {
          string propertyName = (string)ParseConstantFromAttribute<string>(xml, "PropertyName");
          Type declaringType = ParseTypeFromXml(xml.Element("DeclaringType"));
          var ps = from paramXml in xml.Element("IndexParameters").Elements()
                   select ParseTypeFromXml(paramXml);
          return declaringType.GetProperty(propertyName, ps.ToArray());
      }

      private Expression ParseUnaryExpressionFromXml(XElement xml)
      {
          Expression operand = ParseExpressionFromXml(xml.Element("Operand"));
          MethodInfo method = ParseMethodInfoFromXml(xml.Element("Method"));
          var isLifted = (bool)ParseConstantFromAttribute<bool>(xml, "IsLifted");
          var isLiftedToNull = (bool)ParseConstantFromAttribute<bool>(xml, "IsLiftedToNull");
          var expressionType = (ExpressionType)ParseConstantFromAttribute<ExpressionType>(xml, "NodeType");
          var type = ParseTypeFromXml(xml.Element("Type"));
          // TODO: Why can't we use IsLifted and IsLiftedToNull here?  
          // May need to special case a nodeType if it needs them.
          return Expression.MakeUnary(expressionType, operand, type, method);
      }

      private Expression ParseMethodCallExpressionFromXml(XElement xml)
      {
          Expression instance = ParseExpressionFromXml(xml.Element("Object"));
          MethodInfo method = ParseMethodInfoFromXml(xml.Element("Method"));
          var arguments = ParseExpressionListFromXml<Expression>(xml, "Arguments").ToArray();
          return Expression.Call(instance, method, arguments);
      }

      private Expression ParseLambdaExpressionFromXml(XElement xml)
      {
          var body = ParseExpressionFromXml(xml.Element("Body"));
          var parameters = ParseExpressionListFromXml<ParameterExpression>(xml, "Parameters");
          var type = ParseTypeFromXml(xml.Element("Type"));
          // We may need to 
          //var lambdaExpressionReturnType = type.GetMethod("Invoke").ReturnType;
          //if (lambdaExpressionReturnType.IsArray)
          //{

          //    type = typeof(IEnumerable<>).MakeGenericType(type.GetElementType());
          //}
          return Expression.Lambda(type, body, parameters);
      }

      private IEnumerable<T> ParseExpressionListFromXml<T>(XElement xml, string elemName) where T : Expression
      {
          return from tXml in xml.Element(elemName).Elements()
                 select (T)ParseExpressionFromXmlNonNull(tXml);
      }

      private IEnumerable<T> ParseMemberInfoListFromXml<T>(XElement xml, string elemName) where T : MemberInfo
      {
          return from tXml in xml.Element(elemName).Elements()
                 select (T)ParseMemberInfoFromXml(tXml);
      }

      private IEnumerable<ElementInit> ParseElementInitListFromXml(XElement xml, string elemName)
      {
          return from tXml in xml.Element(elemName).Elements()
                 select ParseElementInitFromXml(tXml);
      }

      private ElementInit ParseElementInitFromXml(XElement xml)
      {
          MethodInfo addMethod = ParseMethodInfoFromXml(xml.Element("AddMethod"));
          var arguments = ParseExpressionListFromXml<Expression>(xml, "Arguments");
          return Expression.ElementInit(addMethod, arguments);

      }

      private IEnumerable<MemberBinding> ParseBindingListFromXml(XElement xml, string elemName)
      {
          return from tXml in xml.Element(elemName).Elements()
                 select ParseBindingFromXml(tXml);
      }

      private MemberBinding ParseBindingFromXml(XElement tXml)
      {
          MemberInfo member = ParseMemberInfoFromXml(tXml.Element("Member"));
          switch (tXml.Name.LocalName)
          {
              case "MemberAssignment":
                  Expression expression = ParseExpressionFromXml(tXml.Element("Expression"));
                  return Expression.Bind(member, expression);
              case "MemberMemberBinding":
                  var bindings = ParseBindingListFromXml(tXml, "Bindings");
                  return Expression.MemberBind(member, bindings);
              case "MemberListBinding":
                  var initializers = ParseElementInitListFromXml(tXml, "Initializers");
                  return Expression.ListBind(member, initializers);
          }
          throw new NotImplementedException();
      }


      private Expression ParseParameterExpressionFromXml(XElement xml)
      {
          Type type = ParseTypeFromXml(xml.Element("Type"));
          string name = (string)ParseConstantFromAttribute<string>(xml, "Name");
          //vs: hack
          string id = name + type.AssemblyQualifiedName;
          if (!_parameters.ContainsKey(id))
              _parameters.Add(id, Expression.Parameter(type, name));
          return _parameters[id];
      }

      private Expression ParseConstatExpressionFromXml(XElement xml)
      {
          Type type = ParseTypeFromXml(xml.Element("Type"));
          return Expression.Constant(ParseConstantFromElement(xml, "Value", type), type);
      }

      private Type ParseTypeFromXml(XElement xml)
      {
          Debug.Assert(xml.Elements().Count() == 1);
          return ParseTypeFromXmlCore(xml.Elements().First());
      }

      private Type ParseTypeFromXmlCore(XElement xml)
      {
          switch (xml.Name.ToString())
          {
              case "Type":
                  return ParseNormalTypeFromXmlCore(xml);
              case "AnonymousType":
                  return ParseAnonymousTypeFromXmlCore(xml);
              default:
                  throw new ArgumentException("Expected 'Type' or 'AnonymousType'");
          }

      }

      private Type ParseNormalTypeFromXmlCore(XElement xml)
      {
          if (!xml.HasElements)
              return resolver.GetType(xml.Attribute("Name").Value);

          var genericArgumentTypes = from genArgXml in xml.Elements()
                                     select ParseTypeFromXmlCore(genArgXml);
          return resolver.GetType(xml.Attribute("Name").Value, genericArgumentTypes);
      }

      private Type ParseAnonymousTypeFromXmlCore(XElement xElement)
      {
          string name = xElement.Attribute("Name").Value;
          var properties = from propXml in xElement.Elements("Property")
                           select new ExpressionSerializationTypeResolver.NameTypePair
                           {
                               Name = propXml.Attribute("Name").Value,
                               Type = ParseTypeFromXml(propXml)
                           };
          var ctr_params = from propXml in xElement.Elements("Constructor").Elements("Parameter")
                           select new ExpressionSerializationTypeResolver.NameTypePair
                           {
                               Name = propXml.Attribute("Name").Value,
                               Type = ParseTypeFromXml(propXml)
                           };

          return resolver.GetOrCreateAnonymousTypeFor(name, properties.ToArray(), ctr_params.ToArray());
      }

      private Expression ParseBinaryExpresssionFromXml(XElement xml)
      {
          var expressionType = (ExpressionType)ParseConstantFromAttribute<ExpressionType>(xml, "NodeType"); ;
          var left = ParseExpressionFromXml(xml.Element("Left"));
          var right = ParseExpressionFromXml(xml.Element("Right"));
          var isLifted = (bool)ParseConstantFromAttribute<bool>(xml, "IsLifted");
          var isLiftedToNull = (bool)ParseConstantFromAttribute<bool>(xml, "IsLiftedToNull");
          var type = ParseTypeFromXml(xml.Element("Type"));
          var method = ParseMethodInfoFromXml(xml.Element("Method"));
          LambdaExpression conversion = ParseExpressionFromXml(xml.Element("Conversion")) as LambdaExpression;
          if (expressionType == ExpressionType.Coalesce)
              return Expression.Coalesce(left, right, conversion);
          return Expression.MakeBinary(expressionType, left, right, isLiftedToNull, method);
      }

      private MethodInfo ParseMethodInfoFromXml(XElement xml)
      {
          if (xml.IsEmpty)
              return null;
          string name = (string)ParseConstantFromAttribute<string>(xml, "MethodName");
          Type declaringType = ParseTypeFromXml(xml.Element("DeclaringType"));
          var ps = from paramXml in xml.Element("Parameters").Elements()
                   select ParseTypeFromXml(paramXml);
          var genArgs = from argXml in xml.Element("GenericArgTypes").Elements()
                        select ParseTypeFromXml(argXml);
          return resolver.GetMethod(declaringType, name, ps.ToArray(), genArgs.ToArray());
      }

      private ConstructorInfo ParseConstructorInfoFromXml(XElement xml)
      {
          if (xml.IsEmpty)
              return null;
          Type declaringType = ParseTypeFromXml(xml.Element("DeclaringType"));
          var ps = from paramXml in xml.Element("Parameters").Elements()
                   select ParseParameterFromXml(paramXml);
          ConstructorInfo ci = declaringType.GetConstructor(ps.ToArray());
          return ci;
      }

      private Type ParseParameterFromXml(XElement xml)
      {
          string name = (string)ParseConstantFromAttribute<string>(xml, "Name");
          Type type = ParseTypeFromXml(xml.Element("Type"));
          return type;

      }

      private object ParseConstantFromAttribute<T>(XElement xml, string attrName)
      {
          string objectStringValue = xml.Attribute(attrName).Value;
          if (typeof(Type).IsAssignableFrom(typeof(T)))
              throw new Exception("We should never be encoding Types in attributes now.");
          if (typeof(Enum).IsAssignableFrom(typeof(T)))
              return Enum.Parse(typeof(T), objectStringValue);
          return Convert.ChangeType(objectStringValue, typeof(T));
      }

      private object ParseConstantFromAttribute(XElement xml, string attrName, Type type)
      {
          string objectStringValue = xml.Attribute(attrName).Value;
          if (typeof(Type).IsAssignableFrom(type))
              throw new Exception("We should never be encoding Types in attributes now.");
          if (typeof(Enum).IsAssignableFrom(type))
              return Enum.Parse(type, objectStringValue);
          return Convert.ChangeType(objectStringValue, type);
      }

      private object ParseConstantFromElement(XElement xml, string elemName, Type type)
      {
          string objectStringValue = xml.Element(elemName).Value;
          if (typeof(Type).IsAssignableFrom(type))
              return ParseTypeFromXml(xml.Element("Value"));
          if (typeof(Enum).IsAssignableFrom(type))
              return Enum.Parse(type, objectStringValue);
          return Convert.ChangeType(objectStringValue, type);
      }
  }

  public abstract class CustomExpressionXmlConverter
  {
      public abstract Expression Deserialize(XElement expressionXml);
      public abstract XElement Serialize(Expression expression);
  }

  public class ExpressionSerializationTypeResolver
  {
      private Dictionary<AnonTypeId, Type> _anonymousTypes = new Dictionary<AnonTypeId, Type>();
      private ModuleBuilder _moduleBuilder;
      private int _anonymousTypeIndex = 0;


      public ExpressionSerializationTypeResolver()
      {
          AssemblyName asmname = new AssemblyName();
          asmname.Name = "AnonymousTypes";
          AssemblyBuilder assemblyBuilder = System.Threading.Thread.GetDomain().DefineDynamicAssembly(asmname, AssemblyBuilderAccess.RunAndSave);
          _moduleBuilder = assemblyBuilder.DefineDynamicModule("AnonymousTypes");
      }

      protected virtual Type ResolveTypeFromString(string typeString) { return null; }
      protected virtual string ResolveStringFromType(Type type) { return null; }

      public Type GetType(string typeName, IEnumerable<Type> genericArgumentTypes)
      {
          return GetType(typeName).MakeGenericType(genericArgumentTypes.ToArray());
      }

      public Type GetType(string typeName)
      {/*
            Type type;
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            // First - try all replacers
            type = ResolveTypeFromString(typeName);
            //type = typeReplacers.Select(f => f(typeName)).FirstOrDefault();
            if (type != null)
                return type;

            // If it's an array name - get the element type and wrap in the array type.
            if (typeName.EndsWith("[]"))
                return this.GetType(typeName.Substring(0, typeName.Length - 2)).MakeArrayType();

            // First - try all loaded types
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName, false, true);
                if (type != null)
                    return type;
            }

            // Second - try just plain old Type.GetType()
            type = Type.GetType(typeName, false, true);
            if (type != null)
                return type;

            throw new ArgumentException("Could not find a matching type", typeName);*/

          return Type.GetType(typeName);
      }

      public class NameTypePair
      {
          public string Name { get; set; }
          public Type Type { get; set; }

          public override int GetHashCode()
          {
              return Name.GetHashCode() + Type.GetHashCode();
          }
          public override bool Equals(object obj)
          {
              if (!(obj is NameTypePair))
                  return false;
              NameTypePair other = obj as NameTypePair;
              return Name.Equals(other.Name) && Type.Equals(other.Type);
          }
      }

      private class AnonTypeId
      {
          public string Name { get; private set; }
          public IEnumerable<NameTypePair> Properties { get; private set; }

          public AnonTypeId(string name, IEnumerable<NameTypePair> properties)
          {
              this.Name = name;
              this.Properties = properties;
          }

          public override int GetHashCode()
          {
              int result = Name.GetHashCode();
              foreach (var ntpair in Properties)
                  result += ntpair.GetHashCode();
              return result;
          }

          public override bool Equals(object obj)
          {
              if (!(obj is AnonTypeId))
                  return false;
              AnonTypeId other = obj as AnonTypeId;
              return (Name.Equals(other.Name)
                  && Properties.SequenceEqual(other.Properties));

          }

      }


      public MethodInfo GetMethod(Type declaringType, string name, Type[] parameterTypes, Type[] genArgTypes)
      {
          var methods = from mi in declaringType.GetMethods()
                        where mi.Name == name
                        select mi;
          foreach (var method in methods)
          {
              // Would be nice to remvoe the try/catch
              try
              {
                  MethodInfo realMethod = method;
                  if (method.IsGenericMethod)
                  {
                      realMethod = method.MakeGenericMethod(genArgTypes);
                  }
                  var methodParameterTypes = realMethod.GetParameters().Select(p => p.ParameterType);
                  if (MatchPiecewise(parameterTypes, methodParameterTypes))
                  {
                      return realMethod;
                  }
              }
              catch (ArgumentException)
              {
                  continue;
              }
          }
          return null;
      }

      private bool MatchPiecewise<T>(IEnumerable<T> first, IEnumerable<T> second)
      {
          T[] firstArray = first.ToArray();
          T[] secondArray = second.ToArray();
          if (firstArray.Length != secondArray.Length)
              return false;
          for (int i = 0; i < firstArray.Length; i++)
              if (!firstArray[i].Equals(secondArray[i]))
                  return false;
          return true;
      }

      //vsadov: need to take ctor parameters too as they do not 
      //necessarily match properties order as returned by GetProperties
      public Type GetOrCreateAnonymousTypeFor(string name, NameTypePair[] properties, NameTypePair[] ctr_params)
      {
          AnonTypeId id = new AnonTypeId(name, properties.Concat(ctr_params));
          if (_anonymousTypes.ContainsKey(id))
              return _anonymousTypes[id];

          //vsadov: VB anon type. not necessary, just looks better
          string anon_prefix = name.StartsWith("<>") ? "<>f__AnonymousType" : "VB$AnonymousType_";
          TypeBuilder anonTypeBuilder = _moduleBuilder.DefineType(anon_prefix + _anonymousTypeIndex++, TypeAttributes.Public | TypeAttributes.Class);

          FieldBuilder[] fieldBuilders = new FieldBuilder[properties.Length];
          PropertyBuilder[] propertyBuilders = new PropertyBuilder[properties.Length];

          for (int i = 0; i < properties.Length; i++)
          {
              fieldBuilders[i] = anonTypeBuilder.DefineField("_generatedfield_" + properties[i].Name, properties[i].Type, FieldAttributes.Private);
              propertyBuilders[i] = anonTypeBuilder.DefineProperty(properties[i].Name, PropertyAttributes.None, properties[i].Type, new Type[0]);
              MethodBuilder propertyGetterBuilder = anonTypeBuilder.DefineMethod("get_" + properties[i].Name, MethodAttributes.Public, properties[i].Type, new Type[0]);
              ILGenerator getterILGenerator = propertyGetterBuilder.GetILGenerator();
              getterILGenerator.Emit(OpCodes.Ldarg_0);
              getterILGenerator.Emit(OpCodes.Ldfld, fieldBuilders[i]);
              getterILGenerator.Emit(OpCodes.Ret);
              propertyBuilders[i].SetGetMethod(propertyGetterBuilder);
          }

          ConstructorBuilder constructorBuilder = anonTypeBuilder.DefineConstructor(MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Public, CallingConventions.Standard, ctr_params.Select(prop => prop.Type).ToArray());
          ILGenerator constructorILGenerator = constructorBuilder.GetILGenerator();
          for (int i = 0; i < ctr_params.Length; i++)
          {
              constructorILGenerator.Emit(OpCodes.Ldarg_0);
              constructorILGenerator.Emit(OpCodes.Ldarg, i + 1);
              constructorILGenerator.Emit(OpCodes.Stfld, fieldBuilders[i]);
              constructorBuilder.DefineParameter(i + 1, ParameterAttributes.None, ctr_params[i].Name);
          }
          constructorILGenerator.Emit(OpCodes.Ret);

          //TODO - Define ToString() and GetHashCode implementations for our generated Anonymous Types
          //MethodBuilder toStringBuilder = anonTypeBuilder.DefineMethod();
          //MethodBuilder getHashCodeBuilder = anonTypeBuilder.DefineMethod();

          Type anonType = anonTypeBuilder.CreateType();
          _anonymousTypes.Add(id, anonType);
          return anonType;
      }

  }
}

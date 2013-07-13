using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PetaPoco
{
	public class FluentColumnMap
	{
		public ColumnInfo ColumnInfo { get; set; }
		public Func<object, object> FromDbConverter { get; set; }
		public Func<object, object> ToDbConverter { get; set; }

		public FluentColumnMap() { }
		public FluentColumnMap(ColumnInfo columnInfo) : this(columnInfo, null) { }
		public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter) : this(columnInfo, fromDbConverter, null) { }
		public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter, Func<object, object> toDbConverter)
		{
			ColumnInfo = columnInfo;
			FromDbConverter = fromDbConverter;
			ToDbConverter = toDbConverter;
		}
	}

	public abstract class FluentMapper<T> : IMapper
	{
		public Dictionary<string, FluentColumnMap> Mappings = new Dictionary<string, FluentColumnMap>();
		public TableInfo TableInfo = new TableInfo();

		protected FluentMapper(string tableName, string primaryKey)
		{
			TableInfo.TableName = tableName;
			TableInfo.PrimaryKey = primaryKey;
		}

		public TableInfo GetTableInfo(Type pocoType)
		{
			return TableInfo;
		}

		public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
		{
			return Mappings[pocoProperty.Name].ColumnInfo;
		}

		public Func<object, object> GetFromDbConverter(PropertyInfo TargetProperty, Type SourceType)
		{
			return Mappings[TargetProperty.Name].FromDbConverter;
		}

		public Func<object, object> GetToDbConverter(PropertyInfo SourceProperty)
		{
			return Mappings[SourceProperty.Name].FromDbConverter;
		}
	}

	public static class FluentMapperExtensions
	{
		public static FluentMapper<T> Property<T, P>(this FluentMapper<T> obj, Expression<Func<T, P>> action, string column) where T : class
		{
			return obj.Property(action, column, null);
		}

		public static FluentMapper<T> Property<T, P>(this FluentMapper<T> obj, Expression<Func<T, P>> action, string column, Func<object, object> fromDbConverter) where T : class
		{
			return obj.Property(action,  column, fromDbConverter, null);
		}

		public static FluentMapper<T> Property<T, P>(this FluentMapper<T> obj, Expression<Func<T, P>> action, string column, Func<object, object> fromDbConverter, Func<object, object> toDbConverter) where T : class
		{
			var expression = (MemberExpression)action.Body;
			string name = expression.Member.Name;

			obj.Mappings.Add(name, new FluentColumnMap(new ColumnInfo() { ColumnName = column }, fromDbConverter, toDbConverter));

			return obj;
		}
	}
}

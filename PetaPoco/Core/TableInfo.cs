// PetaPoco - A Tiny ORMish thing for your POCO's.
// Copyright © 2011-2012 Topten Software.  All Rights Reserved.
 
using System;
using System.Reflection;

namespace PetaPoco
{
	/// <summary>
	/// Use by IMapper to override table bindings for an object
	/// </summary>
	public class TableInfo
	{
		/// <summary>
		/// The database table name
		/// </summary>
		public string TableName 
		{ 
			get; 
			set; 
		}

		/// <summary>
		/// The name of the primary key column of the table
		/// </summary>
		public string PrimaryKey 
		{ 
			get; 
			set; 
		}

		/// <summary>
		/// True if the primary key column is an auto-incrementing
		/// </summary>
		public bool AutoIncrement 
		{ 
			get; 
			set; 
		}

		/// <summary>
		/// The name of the sequence used for auto-incrementing Oracle primary key fields
		/// </summary>
		public string SequenceName 
		{ 
			get; 
			set; 
		}


		/// <summary>
		/// Creates and populates a TableInfo from the attributes of a POCO
		/// </summary>
		/// <param name="t">The POCO type</param>
		/// <returns>A TableInfo instance</returns>
		public static TableInfo FromPoco(Type t)
		{
			TableInfo ti = new TableInfo();

			// Get the table name
			var a = t.GetCustomAttributes(typeof(TableNameAttribute), true);
			ti.TableName = a.Length == 0 ? t.Name : (a[0] as TableNameAttribute).Value;

			// Get the primary key on class level
			a = t.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
			if (a.Length != 0)
			{
				ti.PrimaryKey = a.Length == 0 ? "ID" : (a[0] as PrimaryKeyAttribute).Value;
				ti.SequenceName = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).sequenceName;
				ti.AutoIncrement = a.Length == 0 ? false : (a[0] as PrimaryKeyAttribute).autoIncrement;
			}

			// Get the primary key on property level
			else
			{
                foreach (var property in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					a = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);

					if (a.Length != 0)
					{
						var c = t.GetCustomAttributes(typeof(ColumnAttribute), true);
						if (!string.IsNullOrEmpty(ti.PrimaryKey))
							ti.PrimaryKey += ",";
						ti.PrimaryKey += c.Length == 0 ? property.Name : (c[0] as ColumnAttribute).Name;
					}
				}

				// AutoIncrement and Sequence are not supported with multi primary key
				ti.SequenceName = null;
				ti.AutoIncrement = false;
			}

			return ti;
		}
	}

}

// PetaPoco - A Tiny ORMish thing for your POCO's.
// Copyright © 2011-2012 Topten Software.  All Rights Reserved.

using System;
using System.Data;
using PetaPoco.Internal;


namespace PetaPoco.DatabaseTypes
{
    class InformixDatabaseType : DatabaseType
	{
		public override string GetParameterPrefix(string ConnectionString)
		{
			return "?";
		}

		public override void PreExecute(IDbCommand cmd)
		{
			cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
		}

		public override string BuildPageQuery(long skip, long take, PagingHelper.SQLParts parts, ref object[] args)
		{
			if (parts.sqlSelectRemoved.StartsWith("*"))
				throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");

			// Same deal as SQL Server
			return Singleton<SqlServerDatabaseType>.Instance.BuildPageQuery(skip, take, parts, ref args);
		}

		public override string EscapeSqlIdentifier(string str)
		{
            return string.Format("\"{0}\"", str.ToUpperInvariant());
		}

		public override string GetAutoIncrementExpression(TableInfo ti)
		{
			if (!string.IsNullOrEmpty(ti.SequenceName))
				return string.Format("{0}.nextval", ti.SequenceName);

			return null;
		}

		public override object ExecuteInsert(Database db, IDbCommand cmd, string PrimaryKeyName)
		{
			if (PrimaryKeyName != null)
			{
                db.ExecuteNonQueryHelper(cmd);
                return db.ExecuteScalar<object>("SELECT dbinfo('bigserial')"); // select dbinfo('sqlca.sqlerrd1') from systable where tabid = 1 
			}
			else
			{
				db.ExecuteNonQueryHelper(cmd);
				return -1;
			}
		}

	}
}

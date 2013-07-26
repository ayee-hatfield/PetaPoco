// PetaPoco - A Tiny ORMish thing for your POCO's.
// Copyright © 2011-2012 Topten Software.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using PetaPoco.Internal;


namespace PetaPoco.DatabaseTypes
{
	class InformixDatabaseType : DatabaseType
	{
		public override string GetParameterPrefix(string ConnectionString)
		{
			return "?";
		}

		public override string BuildParameter(string prefix, int index)
		{
			return prefix;
		}

		public override bool IsNamedParamsSupported(string connectionString)
		{
			return false;
		}

		public override void PreBuildCommand(ref string sql, ref object[] param)
		{
			// Check if we have an anonymous input parameter
			if (param != null && param.Length == 1 && param[0].GetType().Namespace == null)
			{
				Regex namedParamRegex = new Regex("[@:][a-zA-Z0-9_]+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				
				if (namedParamRegex.IsMatch(sql))
				{
					var paramType = param[0];
					var parameterList = new List<object>();
					System.Type type = paramType.GetType();
					foreach (Match match in namedParamRegex.Matches(sql))
					{
						parameterList.Add(type.GetProperty(match.Value.Substring(1)).GetValue(paramType, null));
					}
					sql = namedParamRegex.Replace(sql, "?");
					param = parameterList.ToArray();
				}
			}
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
			return string.Format("{0}", str.ToUpperInvariant());
		}

		public override string GetAutoIncrementExpression(TableInfo ti)
		{
			if (!string.IsNullOrEmpty(ti.SequenceName))
				return string.Format("{0}.nextval", ti.SequenceName);

			return null;
		}

        public override object ExecuteInsert(Database db, IDbCommand cmd, string PrimaryKeyName, string tableName)
		{
			if (PrimaryKeyName != null)
			{
				db.ExecuteNonQueryHelper(cmd);
                return db.ExecuteScalar<object>(string.Format("SELECT DISTINCT dbinfo('sqlca.sqlerrd1') FROM {0};", tableName));
			}
			else
			{
				db.ExecuteNonQueryHelper(cmd);
				return -1;
			}
		}

		public override string GetExistsSql()
		{
			return "SELECT FIRST 1 1 FROM {0} WHERE {1}";
		}
	}
}

using System;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SQLite;

namespace Fu.SqlKataSimpleSample
{
	internal class Program
	{
		/// <summary>
		/// 入れ物的なクラス
		/// </summary>
		public class IDNameValue
		{
			/// <summary>
			/// ID
			/// </summary>
			public int ID { get; set; }
			/// <summary>
			/// Name
			/// </summary>
			public string Name { get; set; }
			/// <summary>
			/// Value
			/// </summary>
			public int Value { get; set; }

			/// <summary>
			/// 結果出力用
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return $"ID={ID} Name={Name} Value={Value}";
			}
		}

		static void Main(string[] args)
		{
			// SQLiteをオンメモリで作成
			var config = new SQLiteConnectionStringBuilder()
			{
				DataSource = ":memory:",
			};

			using (var conn = new SQLiteConnection(config.ConnectionString))
			{
				// DB Open
				conn.Open();

				// SqlKata お約束
				var compiler = new SqliteCompiler();
				var db = new QueryFactory(conn, compiler);

				// 作成されたクエリを標準出力に出力
				db.Logger = compiled => 
					{
						Console.WriteLine(compiled.ToString());
					};

				Console.WriteLine("■SQL発行");

				// 素のSQL実行 create発行
				db.Statement("create table IDNameValue(ID integer primary key, Name text, Value integer);");

				// insert単品
				db.Query("IDNameValue").Insert(new {ID = 1, Name = "Hoge1", Value = 101 });
				// insertまとめて
				db.Query("IDNameValue").Insert(new[] {"ID", "Name", "Value" },
														new[] {
															new object[] { 2, "Hoge2", 102},
															new object[] { 3, "Hoge3", 103},
															new object[] { 4, "Hoge4", 104}
														});
				// update
				db.Query("IDNameValue").Where("ID", 4).Update(new { Value = 904 });

				// delete
				db.Query("IDNameValue").Where("ID", "<=", 2).Delete();

				// select
				Console.WriteLine("■最終結果");
				foreach(var idnamevalue in db.Query("IDNameValue").Get<IDNameValue>())
				{
					Console.WriteLine(idnamevalue);
				}
			}
		}
	}
}

using System;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.SQLite;

// 実行時に「System.DllNotFoundException: DLL 'SQLite.Interop.dll' を読み込めません:指定されたモジュールが 見つかりません。 (HRESULT からの例外:0x8007007E)」
// が出る場合は
// packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.116.0\build\net46 の中のx64,x86フォルダーを中身(SQLite.Interop.dll)ごと
// ビルドされたbin以下にコピーしてください。

namespace Fu.SqlKataSimpleSample
{
	internal class Program
	{
		// 入れ物的なクラス
		public class IDNameValue
		{
			public int ID { get; set; }
			public string Name { get; set; }
			public int Value { get; set; }

			// 結果出力用
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

				// QueryFactory作成
				var compiler = new SqliteCompiler();
				var db = new QueryFactory(conn, compiler);

				// 作成されたクエリを標準出力に出力
				db.Logger = compiled => 
					{
						Console.WriteLine("■発行されたSQL");
						Console.WriteLine("SQL:" + compiled.Sql.ToString());
						Console.WriteLine("Parameters:" + string.Join(",",compiled.Bindings));
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
					Console.WriteLine(idnamevalue.ToString());
				}

				// close
				conn.Close();
			}
		}
	}
}

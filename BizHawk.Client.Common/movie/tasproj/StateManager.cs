using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BizHawk.Emulation.Common;
using BizHawk.Emulation.Common.IEmulatorExtensions;

namespace BizHawk.Client.Common
{
	public class StateManager : IStateManager
	{
		private readonly TasMovie _movie;

		// TODO: pass these in
		private IStatable _statableCore => Global.Emulator.AsStatable();
		private IEmulator _core => Global.Emulator;

		public StateManager(TasMovie movie)
		{
			_movie = movie;
			Settings = new TasStateManagerSettings(Global.Config.DefaultTasProjSettings);
			InitializeDb();

			if (!_movie.StartsFromSavestate)
			{
				Capture(true);
			}
		}

		public void Dispose()
		{
			// Nothing to do for now
		}

		public byte[] this[int frame] => GetState(frame);

		public TasStateManagerSettings Settings { get; set; }

		public Action<int> InvalidateCallback { private get; set; }

		public void Capture(bool force = false)
		{
			int frame = Global.Emulator.Frame;
			if (frame > 0 && frame % 2 > 0)
			{
				var bytes = (byte[])_statableCore.SaveStateBinary().Clone();
				SetState(frame, bytes);
			}
		}

		public bool Invalidate(int frame)
		{
			string sql = "DELETE FROM states WHERE ID >= @frame";
			SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);
			command.Parameters.Add("@frame", DbType.Int32).Value = frame;

			command.ExecuteNonQuery();

			// TODO: InvalidateCallback
			return false; // TODO
		}

		public bool HasState(int frame)
		{
			string sql = "SELECT COUNT(*) FROM states WHERE ID = @frame AND Invalid = 0";
			SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);
			command.Parameters.Add("@frame", DbType.Int32).Value = frame;

			var blah = command.ExecuteScalar();
			var count = (int)(long)blah;
			return count > 0;
		}

		public void Clear()
		{
			string sql = "DELETE FROM states";
			SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);
			command.ExecuteNonQuery();
		}

		public void Save(BinaryWriter bw)
		{
			// save a path to the file?
		}

		public void Load(BinaryReader br)
		{
			// load a path from the file?
		}

		public KeyValuePair<int, byte[]> GetStateClosestToFrame(int frame)
		{
			string sql = "SELECT MAX(ID), State FROM states WHERE ID < @frame";
			SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);
			command.Parameters.Add("@frame", DbType.Int32).Value = frame;

			var closestFrame = (int)(long)command.ExecuteScalar();

			Console.WriteLine($"closestFrame: {closestFrame}");

			var state = GetState(closestFrame);

			return new KeyValuePair<int, byte[]>(closestFrame, state);
		}

		public bool Any()
		{
			string sql = "SELECT COUNT(*) FROM states WHERE Invalid = 0";
			SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);

			var blah = command.ExecuteScalar();
			var count = (int)(long)blah;
			return count > 0;
		}

		public int Last
		{
			get
			{
				string sql = "SELECT MAX(ID) FROM states WHERE Invalid = 0";
				SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);

				var blah = command.ExecuteScalar();
				var max = (int)(long)blah;
				return max;
			}
		}

		public void SetState(int frame, byte[] state)
		{
			string sql = "INSERT INTO states (Id, State, Invalid) VALUES(@frame, @state, 0)";
			SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);
			command.Parameters.Add("@state", DbType.Binary, state.Length).Value = state;
			command.Parameters.Add("@frame", DbType.Int32).Value = frame;
			command.ExecuteNonQuery();
		}

		private byte[] GetState(int frame)
		{
			string sql = "SELECT state FROM States WHERE ID = @frame";
			SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);
			command.Parameters.Add("@frame", DbType.Int32).Value = frame;

			byte[] buffer = new byte[0];
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					buffer = GetBytes(reader);
				}
			}

			return buffer;
		}

		static byte[] GetBytes(SQLiteDataReader reader)
		{
			const int CHUNK_SIZE = 2 * 1024;
			byte[] buffer = new byte[CHUNK_SIZE];
			long bytesRead;
			long fieldOffset = 0;
			using (MemoryStream stream = new MemoryStream())
			{
				while ((bytesRead = reader.GetBytes(0, fieldOffset, buffer, 0, buffer.Length)) > 0)
				{
					stream.Write(buffer, 0, (int)bytesRead);
					fieldOffset += bytesRead;
				}
				return stream.ToArray();
			}
		}

		private SQLiteConnection _dbConnection;
		private string _dbName => $"{_movie.Filename}.greenzone.sqlite";

		private void InitializeDb()
		{

			if (!File.Exists(_dbName))
			{
				SQLiteConnection.CreateFile(_dbName);
			}

			_dbConnection = new SQLiteConnection($"Data Source={_dbName};Version=3;");
			_dbConnection.Open();

			if (!File.Exists(_dbName))
			{
				string sql = "CREATE TABLE states (Id INTEGER PRIMARY KEY AUTOINCREMENT, State BLOB, Invalid BIT)";
				SQLiteCommand command = new SQLiteCommand(sql, _dbConnection);
				command.ExecuteNonQuery();
			}
		}
	}
}

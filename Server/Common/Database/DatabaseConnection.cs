/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Common;

namespace Common.Database
{
    public class DatabaseConnection
    {
        private MySqlConnection mConnection = null;
        private Queue<string> mQueryQueue = new Queue<string>();
        
        public DatabaseConnection(MySqlConnection mConnection)
        {
            this.mConnection = mConnection;
        }

        ~DatabaseConnection()
        {
            this.mConnection.Close();
        }

        public ulong QueryLID(string query)
        {
            MySqlDataReader reader = null;

            try
            {
                MySqlCommand command = new MySqlCommand(query, this.mConnection);

                reader = command.ExecuteReader();
                return (ulong) reader.FieldCount;
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();
                
                Log.Error("Database", $"MySQL error: {e.Message}");
                throw;
            }
        }

        public bool Query(string query)
        {
            MySqlDataReader reader = null;

            try
            {
                MySqlCommand command = new MySqlCommand(query, this.mConnection);

                reader = command.ExecuteReader();
                return true;
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();
                
                Log.Error("Database", $"MySQL error: {e.Message}");
                throw;
            }
        }

        public bool Query(ref MySqlDataReader reader, string query)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(query, this.mConnection);

                reader = command.ExecuteReader();
                return true;
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();
                
                Log.Error("Database", $"MySQL error: {e.Message}");
                throw;
            }
        }
        public void QueueQuery(string query)
        {
            lock (this.mQueryQueue)
            {
                this.mQueryQueue.Enqueue(query);
            }
        }
        
        public static DatabaseConnection FromConfiguration(Configuration.Database configuration)
        {
            string connStr =
                $"server={configuration.Hostname};user={configuration.Username}; password={configuration.Password}; database={configuration.Name}; pooling=false";
            
            MySqlConnection connection = new MySqlConnection(connStr);
            connection.Open();

            return new DatabaseConnection(connection);
        }

        public string DoEscapeString(string input)
        {
            return MySqlHelper.EscapeString(input);
        }

        public void Update()
        {
            Log.Debug("Database", "Saving information into the DB");

            lock (this.mQueryQueue)
            {
                while (this.mQueryQueue.Count > 0)
                {
                    this.Query(this.mQueryQueue.Dequeue());
                }
            }
        }
    }
}

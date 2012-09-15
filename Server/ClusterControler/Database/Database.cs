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
using System.Data.SqlClient;
using System.Threading;

using Common;

namespace EVESharp.ClusterControler.Database
{
    public static class Database
    {
        public static evesharpDataContext context;
        private static Thread mainThread;

        // Five seconds just to test the server
        public const int UpdateTimer = 5000;

        public static bool Init()
        {
            try
            {
                context = new evesharpDataContext();
            }
            catch
            {
                return false;
            }

            // Start the update thread
            mainThread = new Thread(UpdateThread);
            mainThread.Start();

            return true;
        }

        private static void UpdateThread()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(UpdateTimer);

                    try
                    {
                        context.SubmitChanges();
                    }
                    catch
                    {

                    }
                }
            }
            catch (ThreadAbortException)
            {
                Log.Debug("Database", "Update Thread stop requested");
            }
            catch (ThreadInterruptedException)
            {
                Log.Debug("Database", "Update Thread stop requested");
            }
            catch (Exception ex)
            {
                Log.Error("Database", "Cannot update database content, the thread has exited with an unknown exception");
                Log.Error("Database", "Extra info: " + ex.ToString());
            }
        }

        public static void Stop()
        {
            mainThread.Abort();
        }
    }
}

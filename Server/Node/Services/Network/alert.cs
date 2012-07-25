using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Common;
using Common.Services;
using System.IO;

namespace EVESharp.Services.Network
{
    public class alert : Service
    {
        public alert()
            : base("alert")
        {
        }

        public PyObject BeanCount(PyTuple args, object client)
        {
            PyTuple res = new PyTuple();

            res.Items.Add(new PyNone()); // Unique error ID, None for instant stack trace
            res.Items.Add(new PyInt(0)); // logging mode, 0 = local

            return res;
        }

        public PyObject SendClientStackTraceAlert(PyTuple args, object client)
        {
            Log.Trace("SendClientStackTraceAlert", "Received client stack trace. Saving to a file");

            try
            {
                if (File.Exists("logs/stacktrace.txt") == false)
                {
                    File.Create("logs/stacktrace.txt");
                }

                File.AppendAllText("logs/stacktrace.txt", "------------------ " + args.Items[2].StringValue + " ------------------\n");
                File.AppendAllText("logs/stacktrace.txt", args.Items[0].As<PyTuple>().Items[1].StringValue);
                File.AppendAllText("logs/stacktrace.txt", args.Items[1].StringValue + "\n");
            }
            catch (Exception)
            {

            }

            // The client should receive anything to know that the stack trace arrived to the server
            return new PyNone();
        }

        public PyObject BeanDelivery(PyTuple args, object client)
        {
            // I'm not joking, send me the stack trace NOW!!!
            // :P
            return new PyNone();
        }
    }
}

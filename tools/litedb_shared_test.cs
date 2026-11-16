// F5: does LiteDB shared mode keep two processes apart under Mono?
// Build on the Linux machine (LiteDB.dll from the kiosk build folder):
//   mcs -r:$HOME/lg1/entry/LiteDB.dll -out:$HOME/lg1/f5/f5.exe litedb_shared_test.cs
// Run from the folder that holds f5.exe and a copy of LiteDB.dll:
//   mono f5.exe mutex hold & sleep 1; mono f5.exe mutex try; wait
//   mono f5.exe write test.db A 2000 & mono f5.exe write test.db B 2000; wait; mono f5.exe check test.db
using System;
using System.Collections.Generic;
using System.Threading;
using LiteDB;

class LiteDbSharedTest
{
    static int Main(string[] args)
    {
        if (args.Length >= 2 && args[0] == "mutex")
        {
            using (var m = new Mutex(false, "Global\\litedb-shared-test"))
            {
                bool got = m.WaitOne(args[1] == "hold" ? 5000 : 0);
                Console.WriteLine("mutex " + args[1] + ": acquired=" + got + (args[1] == "try" ? (got ? "  -> NOT shared between processes" : "  -> shared between processes") : ""));
                if (args[1] == "hold" && got)
                {
                    Thread.Sleep(4000);
                    m.ReleaseMutex();
                }
            }
            return 0;
        }

        if (args.Length == 4 && args[0] == "write")
        {
            string cs = "Filename=" + args[1] + ";Connection=shared;";
            int count = int.Parse(args[3]);
            int errors = 0;
            var start = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    // Pango opens the database once per operation, like this
                    using (var db = new LiteDatabase(cs))
                    {
                        var doc = new BsonDocument();
                        doc["w"] = args[2];
                        doc["i"] = i;
                        db.GetCollection("t").Insert(doc);
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    if (errors <= 5)
                        Console.WriteLine("writer " + args[2] + " error at " + i + ": " + ex.GetType().Name + ": " + ex.Message);
                }
            }
            Console.WriteLine("writer " + args[2] + ": " + count + " inserts, " + errors + " errors, " + (int)(DateTime.Now - start).TotalSeconds + " s");
            return 0;
        }

        if (args.Length == 2 && args[0] == "check")
        {
            try
            {
                var perWriter = new Dictionary<string, int>();
                int total = 0;
                using (var db = new LiteDatabase("Filename=" + args[1] + ";Connection=shared;"))
                {
                    foreach (var doc in db.GetCollection("t").FindAll())
                    {
                        string w = doc["w"].AsString;
                        int n;
                        perWriter.TryGetValue(w, out n);
                        perWriter[w] = n + 1;
                        total++;
                    }
                }
                foreach (var kv in perWriter)
                    Console.WriteLine("writer " + kv.Key + ": " + kv.Value + " documents");
                Console.WriteLine("total: " + total + " documents");
            }
            catch (Exception ex)
            {
                Console.WriteLine("check FAILED: " + ex.GetType().Name + ": " + ex.Message);
                return 1;
            }
            return 0;
        }

        Console.WriteLine("usage: f5.exe mutex hold|try | write <db> <name> <count> | check <db>");
        return 2;
    }
}

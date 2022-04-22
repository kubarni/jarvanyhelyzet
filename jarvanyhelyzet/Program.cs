using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace jarvanyhelyzet
{
    class Program
    {
        static void Main(string[] args)
        {
            var tes = Enumerable.Range(1, 5)
                .Select(i => new Terem()).ToList();
            var ds = Enumerable.Range(1, 40)
                .Select(i => new Diak()).ToList();
            var taks = Enumerable.Range(1, 2)
                .Select(i => new Takarito()).ToList();

            var ts = ds.Select(x => new Task(() =>
            {
                x.Felel(tes, taks);
            }, TaskCreationOptions.LongRunning)).ToList();
            ts.AddRange(taks.Select(x => new Task(() =>
            {
                x.Dolgozik(ds);
            }, TaskCreationOptions.LongRunning)).ToList());

            ts.Add(new Task(() =>
            {
                int ido = 0;
                while (ds.Any(p => p.Allapota != Diak.Allapot.hazamegy))
                {
                    Console.Clear();
                    Console.WriteLine("Játékosok:");
                    foreach (var d in ds)
                    {
                        Console.WriteLine(d);
                    }
                    Console.WriteLine("\nTakarítók:");
                    foreach (var t in taks)
                    {
                        Console.WriteLine(t);
                    }
                    Console.WriteLine("\nTermek:");
                    foreach (var t in tes)
                    {
                        Console.WriteLine(t);
                    }
                    ido += 200;
                    Console.WriteLine("Indítás óta eltelt idő: " + ido / 1000.0 + " perc.");
                    Thread.Sleep(200);
                }

                Console.Clear();
                Console.WriteLine("VÉGE");
                Console.WriteLine("Összesen eltelt idő: " + ido / 1000.0 + " perc.");
            }, TaskCreationOptions.LongRunning));

            ts.ForEach(t => t.Start());

            Console.ReadLine();
        }

        class Diak
        {
            public enum Allapot
            {
                var, felel, hazamegy
            }
            public Allapot Allapota { get; set; }
            public static int Nextid = 1;
            public int Id { get; set; }
            public Diak()
            {
                Allapota = Allapot.var;
                Id = Nextid++;
            }

            public override string ToString()
            {
                return $"Id: {Id} Állapot: {Allapota}";
            }

            public void Felel(List<Terem> terems, List<Takarito> takaritok)
            {
                Terem t;
                lock (Terem.teremValaszto)
                {
                    t = terems.Where(x => x.Allapota == Terem.Allapot.tiszta).FirstOrDefault();
                    if (t != null)
                    {
                        t.Allapota = Terem.Allapot.feleltetnek;
                        Allapota = Allapot.felel;
                    }
                    else
                    {
                        Monitor.Wait(Terem.teremValaszto);
                        t = terems.Where(x => x.Allapota == Terem.Allapot.tiszta).FirstOrDefault();
                        t.Allapota = Terem.Allapot.feleltetnek;
                        Allapota = Allapot.felel;
                    }
                }
                if (t != null)
                {
                    Thread.Sleep(Util.rnd.Next(7000, 13001));
                    t.Allapota = Terem.Allapot.fertotlenitesre_var;
                    Allapota = Allapot.hazamegy;

                    //takaritó ping
                    Takarito tak;
                    lock (Terem.takaritoValaszto)
                    {
                        tak = takaritok.Where(x => x.Allapota == Takarito.Allapot.szabad).FirstOrDefault();
                        if (tak != null)
                        {
                            t.Allapota = Terem.Allapot.fertotlenitik;
                            tak.Allapota = Takarito.Allapot.dolgozik;
                            Monitor.Pulse(tak.lockObject);
                        }
                        else
                        {
                            t.Allapota = Terem.Allapot.fertotlenitesre_var;
                            Monitor.Wait(Terem.takaritoValaszto);

                            tak = takaritok.Where(x => x.Allapota == Takarito.Allapot.szabad).FirstOrDefault();
                            t.Allapota = Terem.Allapot.fertotlenitik;
                            tak.Allapota = Takarito.Allapot.dolgozik;
                            Monitor.Pulse(tak.lockObject);
                        }
                    }
                }
            }
        }

        class Takarito
        {
            public object lockObject;
            public enum Allapot
            {
                szabad, dolgozik, hazamegy
            }
            public Allapot Allapota { get; set; }
            public static int Nextid = 1;
            public int Id { get; set; }
            public Takarito()
            {
                Allapota = Allapot.szabad;
                Id = Nextid++;
                lockObject = new object();
            }
            public override string ToString()
            {
                return $"Id: {Id} Állapot: {Allapota}";
            }

            public void Dolgozik(List<Diak> diakok)
            {
                while (diakok.Any(x => x.Allapota != Diak.Allapot.hazamegy))
                {
                    lock (lockObject)
                        Monitor.Wait(lockObject);
                    Allapota = Allapot.dolgozik;
                    Thread.Sleep(Util.rnd.Next(1000, 5001));
                    Allapota = Allapot.szabad;
                    lock (Terem.takaritoValaszto)
                        Monitor.Pulse(Terem.takaritoValaszto);
                    lock (Terem.teremValaszto)
                        Monitor.Pulse(Terem.teremValaszto);
                }
                Allapota = Allapot.hazamegy;
            }
        }

        class Terem
        {
            public static object teremValaszto = new object();
            public static object takaritoValaszto = new object();
            public enum Allapot
            {
                tiszta, fertotlenitik, feleltetnek, fertotlenitesre_var
            }
            public Allapot Allapota { get; set; }
            public static int Nextid = 1;
            public int Id { get; set; }
            public Terem()
            {
                Allapota = Allapot.tiszta;
                Id = Nextid++;
            }
            public override string ToString()
            {
                return $"Id: {Id} Állapot: {Allapota}";
            }
        }

        static public class Util
        {
            static public Random rnd = new Random();
        }
    }
}

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
            var ds = Enumerable.Range(1, 20)
                .Select(i => new Diak()).ToList();
            var taks = Enumerable.Range(1, 2)
                .Select(i => new Takarito()).ToList();

            var ts = tes.Select(x => new Task(() =>
            {
                x.Feleltetes(ds,taks);
            }, TaskCreationOptions.LongRunning)).ToList();

            ts.Add(new Task(() =>
            {
                int ido = 0;
                int time = 200;
                while (ds.Any(p => p.Allapota != Diak.Allapot.hazamegy) || tes.Any(t=>t.Allapota != Terem.Allapot.ENNYI))
                {
                    Console.Clear();
                    Console.WriteLine("Diákok:");
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
                    ido += time;
                    Console.WriteLine("Indítás óta eltelt idő: " + ido / 1000.0 + " perc.");
                    Thread.Sleep(time);
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

        }

        class Takarito
        {
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
            }
            public override string ToString()
            {
                return $"Id: {Id} Állapot: {Allapota}";
            }
        }

        class Terem
        {
            public static object diakokValaszto = new object();
            public static object takaritoValaszto = new object();
            public enum Allapot
            {
                tiszta, fertotlenitik, feleltetnek, fertotlenitesre_var, ENNYI
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

            public void Feleltetes(List<Diak> diakok, List<Takarito> takkerek)
            {
                Diak d = new Diak();
                while (diakok.Any(x => x.Allapota != Diak.Allapot.hazamegy))
                {
                    if (Allapota == Allapot.tiszta && diakok.Any(x => x.Allapota == Diak.Allapot.var))
                    {
                        lock (diakokValaszto)
                        {
                            d = diakok.Where(x => x.Allapota == Diak.Allapot.var).FirstOrDefault();
                            if (d != null)
                            {
                                d.Allapota = Diak.Allapot.felel;
                                Allapota = Allapot.feleltetnek;
                            }
                        }
                        if (d != null)
                        {
                            Thread.Sleep(Util.rnd.Next(7000, 13001));
                            Allapota = Allapot.fertotlenitesre_var;
                            d.Allapota = Diak.Allapot.hazamegy;
                        }
                        else
                        {
                            Thread.Sleep(Util.rnd.Next(200, 301));
                            return;
                        }
                    }
                    Takarito tak;
                    lock (takaritoValaszto)
                    {
                        tak = takkerek.Where(t => t.Allapota == Takarito.Allapot.szabad).FirstOrDefault();
                        if (tak != null)
                        {
                            tak.Allapota = Takarito.Allapot.dolgozik;
                            Allapota = Allapot.fertotlenitik;
                        }
                    }
                    if (tak != null)
                    {
                        Thread.Sleep(Util.rnd.Next(1000, 5001));
                        Allapota = Allapot.tiszta;
                        tak.Allapota = Takarito.Allapot.szabad;
                    }
                    else
                    {
                        Thread.Sleep(Util.rnd.Next(200, 301));
                    }
                }
                Allapota = Allapot.ENNYI;
            }
        }

        static public class Util
        {
            static public Random rnd = new Random();
        }
    }
}

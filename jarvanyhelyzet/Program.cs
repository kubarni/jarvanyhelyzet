using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace jarvanyhelyzet
{
    class Program
    {
        static void Main(string[] args)
        {
            var ts = Enumerable.Range(1, 5)
                .Select(i => new Terem()).ToList();
        }
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
                        Monitor.Pulse(tak.lockObject);
                    }
                    else
                    {
                        Monitor.Wait(Terem.takaritoValaszto);
                        tak = takaritok.Where(x => x.Allapota == Takarito.Allapot.szabad).FirstOrDefault();
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
            szabad, dolgozik
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

        public void Dolgozik(List<Diak> diakok)
        {
            while (diakok.Any(x=>x.Allapota != Diak.Allapot.hazamegy))
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
    }

    static public class Util
    {
        static public Random rnd = new Random();
    }
}

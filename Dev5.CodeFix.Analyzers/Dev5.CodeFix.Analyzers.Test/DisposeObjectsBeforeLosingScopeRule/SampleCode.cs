using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class DummyClass
    {
        private Stream _s;

        public DummyClass()
        {
            _s = Create();

            using(Stream a = Create(), b = Create())
            {

            }

            var sb = new StringBuilder();

            Stream c;
            using (c = Create())
            {

            }

            StringBuilder sb2;
            sb2 = new StringBuilder();

            var d = Create();
        }

        Stream Create()
        {
            return null;
        }

        public void Method()
        {
            var stream = new MemoryStream();
        }

        public void Method2()
        {
            var sb = new StringBuilder();
        }
    }
}
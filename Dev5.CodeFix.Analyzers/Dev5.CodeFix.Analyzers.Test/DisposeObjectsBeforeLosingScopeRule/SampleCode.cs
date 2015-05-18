using System.IO;
using System.Text;

class DummyClass
{
    private Stream _s;

    public DummyClass()
    {
        /* 
         * almost correct usage: 
         * value of field _s must be disposed later 
         * (maybe the rule can suggest to implement IDisposable interface) 
         */
        _s = Create();

        /* 
         * correct usage: 
         * assigning IDisposable inside using block to variables
         */
        using (Stream a = Create(), b = Create()) { }

        /* 
         * correct usage: 
         * assigning IDisposable inside using block to a previously declared variable 
         */
        Stream c;
        using (c = Create()) { }

        /* 
         * incorrect usage: 
         * not using using statement for declaration and initialization of a IDisposable variable 
         */
        var d = Create();

        /*
         * these lines were added just to prove that the rule is ignoring non-IDisposable variables
         */
        var sb = new StringBuilder(); // declaration and initialization of a non-IDisposable variable  
        StringBuilder sb2;
        sb2 = new StringBuilder(); // assigning non-IDisposable to a previously declared variable
    }

    Stream Create()
    {
        return null; // the real value is not important, return type is
    }

    public void Method()
    {
        /* 
         * incorrect usage: 
         * not using using statement for declaration and initialization of a IDisposable variable 
         */
        var stream = new MemoryStream();
    }
}
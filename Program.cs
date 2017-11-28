using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{



    //Default access modifer of internal 
    class Vector
    {

        //Declare a delgate which returns void and accepts a vector
        public delegate void OnVectorValueChange(Vector v);

        //Define an event that fires when a vectors value changes. Registered events must conform to the delgates function signature., basically a list of callbacks attached to the vector object.
        public event OnVectorValueChange VectorValueChanged;

        //Virtual getters and setters, can be overriden in subclasses.
        public virtual int X {

            get
            {
                return x;
            }
            set
            {
                x = value;
                VectorValueChanged?.Invoke(this);
            }
        }
        //They can also be hidden in subclasses using the new keyword, althought its kinda messed up using hiding
        //As the value given to the property is dependent upon type...So a dog with Name set as an Animal may not have a name..
        public virtual int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                VectorValueChanged?.Invoke(this);
            }
        }

        //Default to private
        int x;
        int y;


        public Vector(int x, int y)
        {
            X = x;
            Y = y;
        }

        //Operator overloading
        public static Vector operator +(Vector one, Vector two)
        {
            //You can use += -= *= and /= with properties
            one.X += two.X;
            one.Y += two.Y;
            return one;
        }

        public static Vector operator *(Vector one, Vector two)
        {
            //You can use += -= *= and /= with properties
            one.X *= two.X;
            one.Y *= two.Y;
            return one;
        }

        public static Vector operator -(Vector one, Vector two)
        {
            return one + new Vector(-two.X, -two.Y);
        }

        public static Vector operator /(Vector one, Vector two)
        {
            return one + new Vector(1/two.X, 1/two.Y);
        }

        //Expression syntax, defines a getter?
        public (int, int) Tuple => (X, Y);

        //Returning a tuple from a method
        public (int, int) ToTuple() => Tuple;




        //Return x by reference
        public ref int xRef
        {
            get
            {
                return ref x;
            }
        }

        public ref int yRef
        {
            get
            {
                return ref y;
            }
        }


        //Overridng virtual methods
        public override string ToString()
        {
            return String.Format("({0},{1})", X, Y);
        }

    }


    //Extension methods
    static class VectorExtensions
    {

        //Can be used on Vector instances and be used as a static method with Vector passed in.
        public static Vector Copy(this Vector v)
        {
            return new Vector(v.X, v.Y);
        }

        //Creates a copy using the original vector instance.
        public static void NewInstance(ref Vector v)
        {
            v = new Vector(v.X, v.Y);
        }

        //Uses an out reference, meaning that v is a pointer and must be assigned before the method returns.
        //Outside the method the users reference will be set to the new Vector.
        public static void Create(out Vector v)
        {
            v = new Vector(0, 0);
        }

        public static Vector Create(int x, int y)
        {
            return new Vector(0, 0);
        }
    }

    class Animal
    {
    
        public virtual void Talk()
        {
            Console.WriteLine("Errrrr");
        }
    }
    
    class Dog : Animal
    {
        //Hiding... means that the method executed depends on the type.
        public new void Talk()
        {
            Console.WriteLine("Woof");
        }
 
    }


    class Program
    {

        static void TakesATuple((string,int,int) s)
        {

        }

        static void Main(string[] args)
        {
            Vector v = new Vector(10, 10);

            v.VectorValueChanged += (o) => Console.WriteLine("Vec changed");
            v.VectorValueChanged += (o) => Console.WriteLine("Vec changed1");
            v.X = 2;


            Vector x = new Vector(20, 20);

            Vector c = v + x;
            Console.WriteLine("Value of c x {0}, value of c y {1}", c.X, c.Y);
            Console.WriteLine("c == v ... {0}", c == v);

            Console.WriteLine(VectorExtensions.Copy(x));
            Console.WriteLine(v.Copy());

            Console.WriteLine(v.Tuple);

            TakesATuple(("", 1, 1));

            ref int j = ref v.xRef;

            j = 2;
            Console.WriteLine(v);

            Dog d = new Dog();
            Animal a = d;

            d.Talk();
            a.Talk();

            var tuple = v.Tuple;
            var (g, k) = tuple;
        }
    }
}

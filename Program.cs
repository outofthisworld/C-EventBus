using ConsoleApp2.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        //Tuples can be assigned to...
        public Vector(int x, int y) => (X,Y) = (x,y);

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

        //Expression syntax, shorthand for
        /*
         * 
         *   public (int, int) Tuple {
         *   
         *      get {
         *          return (X,Y);
         *      }
         *   }
         */
        public (int, int) Tuple => (X, Y);

        //Returning a tuple from a method (Lambda in class)
        public (int, int) ToTuple() => Tuple;


        //Return x by reference (X can be modified from outside class, without use of getters and setters(pointer))
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

        //Deconstruct into tuple (user defined type to tuple)
        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
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
        //Declare virtual so it can be overriden in subclasses.
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

    namespace Events
    {

        class EventObj
        {
            public string EventName = "EventObj";
        }

        class SupEventObj : EventObj
        {
            public new string EventName = "SupEventObj";
        }

        class EventObjAttr: System.Attribute
        {

        }

        delegate void EventBusHandler<T>(T eventObj) where T : EventObj;

        class TestEventObj
        {

            [EventObjAttr]
            public void HandleEvent(SupEventObj s)
            {
                Console.WriteLine("TestEventObj handleEvent caled");
            }

        }

        class EventBus
        {

            Dictionary<Type, List<Delegate>> hm = new Dictionary<Type, List<Delegate>>();

            public EventBus()
            {

            }


            public void RegisterHandler<T>(EventBusHandler<T> handler) where T:EventObj
            {
                Add(typeof(T), handler);
            }

            private void Add(Type t,dynamic val)
            {
                List<Delegate> l;
                if (hm.TryGetValue(t, out l))
                {
                    l.Add(val);
                }
                else
                {
                    l = new List<Delegate>();
                    l.Add(val);
                    hm.Add(t, l);
                }
            }

            public List<Delegate> this[Type t]
            {
                get
                {
                    List<Delegate> l;
                    hm.TryGetValue(t, out l);
                    return l;
                }
                set
                {
                    hm.Add(t, value);
                }
            }

            public Delegate this[Type t, Delegate handler]
            {
                get
                {
                    List<Delegate> l;
                    hm.TryGetValue(t, out l);
                    

                    foreach(Delegate d in l)
                    {
                        if (d != handler) continue;
                        return d;
                    }

                    return null;
                }

            }

            public Delegate this[Delegate handler]
            {
                get
                {
                    foreach(Type t in hm.Keys)
                    {
                        Delegate d = this[t, handler];
                        if (d != null) return d;
                    }
                    return null;
                }

            }


            public void RegisterHandler(object handler)
            {

                if (handler == null)
                {
                    throw new ArgumentException("Invalid argument, handler cannot be null");
                }

                MethodInfo[] methodInfos = handler.GetType().GetMethods();


                foreach(MethodInfo info in methodInfos)
                {

                    //Skip the method because it does not contain the attribute that we are looking for
                    if (info.CustomAttributes.Where((c) => c.AttributeType.Equals(typeof(EventObjAttr))).Count() <= 0) continue;

                    //Get the parameters of the method
                    ParameterInfo[] paramInfo = info.GetParameters();

                    //One parameter is the right number, if the event method specifies more of less.. throw an exception.
                    if(paramInfo.Length != 1)
                    {
                        throw new Exception(String.Format("Invalid event method attempting to be registered...number of paramaters must be one: {0}",info));
                    }

                    //Get the first parameter.
                    ParameterInfo i = paramInfo[0];
                    //Get the type of parameter
                    Type t = i.ParameterType;

                    //Check that it extends EventObj
                    if(!typeof(EventObj).IsAssignableFrom(t))
                    {
                        throw new Exception(String.Format("Parameter {0} cannot be assigned to EventObj (event parameters must extend EventObj)", t));
                    }


                     //Creates a generic type using a delegate function signature and substiting in a Type t, and creates a delegate from the current method info on the current handler object
                    Delegate del = Delegate.CreateDelegate(typeof(EventBusHandler<>).MakeGenericType(t), handler, info);
                    Add(t, del);
                }
            }

            public int Fire<T>(T e) where T:EventObj
            {
                Type t = typeof(T);

                List<Delegate> handlers;

                if (hm.TryGetValue(t,out handlers))
                {
                    foreach(Delegate handler in handlers){
                        try
                        {
                             handler?.DynamicInvoke(e);
                        }catch(Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw ex;
                        }
                    }

                    return handlers.Count;
                }

                return 0;
            }
        }


        class EventBusWithAttr
        {

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

            //Deconstruct a user defined type.
            (int ff,int dd) = v;
            Console.WriteLine("Deconstructed x {0}", ff);
            Console.WriteLine("Deconstructed y {0}", dd);
            

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

            EventObj tt = new EventObj();
            SupEventObj qq = new SupEventObj();
            EventObj pp = qq;

            Console.WriteLine(tt.GetType());
            Console.WriteLine(qq.GetType());
            Console.WriteLine(pp.GetType());

            EventBus eventBus = new EventBus();
            eventBus.RegisterHandler((EventObj e) => Console.WriteLine("Recieved event {0}",e.EventName));
            eventBus.RegisterHandler((SupEventObj e) => Console.WriteLine("Recieved event {0}" ,e.EventName));
            //eventBus.RegisterEventHandler(delegate (SupEventObj e) { Console.WriteLine("Recieved event {0}", e.EventName); });
            eventBus.RegisterHandler<EventObj>(Take);
            EventBusHandler<EventObj> eee = new EventBusHandler<EventObj>(Take);
            eventBus.RegisterHandler(eee);

            //EventObj event
            eventBus.Fire(tt);
            System.Threading.Thread.Sleep(1000);
            //SupEventObj event
            eventBus.Fire(qq);
            System.Threading.Thread.Sleep(1000);
            //SupEventObj event
            eventBus.Fire(pp);

            //Register handler object with event bus
            eventBus.RegisterHandler(new TestEventObj());
            //All methods with attributes [EventBusAttr] are registered as handlers, under the type of param in which the method accepts
            //When an event of the matching type of param the method accepts is fired.. that method is called.
            eventBus.Fire(qq);

            Delegate ppp = eventBus[typeof(EventObj), eee];
            Console.WriteLine("ppp is : {0}", ppp);
            Console.WriteLine("Invoking event");
            ppp.DynamicInvoke(tt);

            //Reset all events for the specified type
            eventBus[typeof(EventObj)] = new List<Delegate>();
            //Retrieve all handlers for a specific type of event
            List<Delegate> handlersOfType = eventBus[typeof(EventObj)];
            //Retrieve a delegate that has been added using RegisterEventHandler
            Delegate de = eventBus[typeof(EventObj), eee];
            //Search for a delegate without type (slower, traverses hm keys) o(n^2)
            Delegate dede = eventBus[ eee];
        }

        public static void Take(EventObj o)
        {
            Console.WriteLine("Take");
        }
    }
}

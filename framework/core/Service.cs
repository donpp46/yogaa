using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Linq;

namespace Medtrix.WebService
{
    internal class Response
    {
        public Boolean iserror = false;
        public String message = String.Empty;
        public Object result = null;
        public int code = 0;
    }

    internal class Request<T> 
    {
        public String service = String.Empty;
        public String command = String.Empty;
        public T Data;
    }

    public class ServiceException : Exception
    {
        public UInt64 ErrorCode = 0;
        public String ErrorMessage = String.Empty;

        public ServiceException(UInt64 ErrorCode, String ErrorMessage)
        {
            Exception ex = new Exception();
            ex.Data["ErrorCode"] = ErrorCode;
            ex.Data["ErrorMessage"] = ErrorMessage;
            throw ex;
        }
    }

    public interface Service
    {
    }

    public class DateTimeJavaScriptConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            return new JavaScriptSerializer().ConvertToType(dictionary, type);
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            if (!(obj is DateTime)) return null;
            return new CustomString(((DateTime)obj).ToUniversalTime().ToString("O"));
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new[] { typeof(DateTime) }; }
        }

        private class CustomString : Uri, IDictionary<string, object>
        {
            public CustomString(string str)
                : base(str, UriKind.Relative)
            {
            }

            void IDictionary<string, object>.Add(string key, object value)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<string, object>.ContainsKey(string key)
            {
                throw new NotImplementedException();
            }

            ICollection<string> IDictionary<string, object>.Keys
            {
                get { throw new NotImplementedException(); }
            }

            bool IDictionary<string, object>.Remove(string key)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<string, object>.TryGetValue(string key, out object value)
            {
                throw new NotImplementedException();
            }

            ICollection<object> IDictionary<string, object>.Values
            {
                get { throw new NotImplementedException(); }
            }

            object IDictionary<string, object>.this[string key]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<string, object>>.Clear()
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            int ICollection<KeyValuePair<string, object>>.Count
            {
                get { throw new NotImplementedException(); }
            }

            bool ICollection<KeyValuePair<string, object>>.IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
            {
                throw new NotImplementedException();
            }



            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }


    public class ServiceManager 
    {
        private static Dictionary<string, Type> _serviceClasses = new Dictionary<string, Type>();
        private static MethodInfo jsonDeserializeMethod = null;
        private ServiceManager()
        {
        }

        static ServiceManager()
        {
            //foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(Service).IsAssignableFrom(type) && type.IsClass)
                        _serviceClasses[type.Name] = type;
                }
            }

            jsonDeserializeMethod = typeof(JavaScriptSerializer).GetMethods().FirstOrDefault(method => method.Name == "Deserialize" & method.IsGenericMethod);
        }

        private static Object GetService(string serviceName)
        {
            Object service = null;
            Type serviceClass = null;

            if (_serviceClasses.TryGetValue(serviceName, out serviceClass))
            {
                service = Activator.CreateInstance(serviceClass);
            }

            return service;
        }

        public static void Register(Type service)
        {
            //if(!typeof(Service).IsAssignableFrom(service))
            //    throw new Exception(String.Format("Service {0} does not implement IService", service.Name));
            
            _serviceClasses[service.Name] = service;

        }


        public static String DoService(String packet, System.Web.UI.Page ExecutingPage)
        {
            JavaScriptSerializer _json = new JavaScriptSerializer();
            _json.MaxJsonLength = int.MaxValue;
            _json.RegisterConverters( new JavaScriptConverter[] { new DateTimeJavaScriptConverter()});

            Request<Object> request = new Request<Object>();
            Response response = new Response();
            try
            {
                //Logger.Log(data);

                // convert the request packet into generic structure
                request = _json.Deserialize<Request<Object>>(packet);

            }
            catch (Exception)
            {
                if (_json == null)
                    return "Failed to load json parser.";

                response.iserror = true;
                response.message = "Invalid data format.";
                response.code = 999;

            }
            try
            {
                if (request == null)
                    throw new Exception("Invalid request");

                Object service = GetService(request.service);
                if(service == null)
                    throw new Exception("Invalid service requested.");

                MethodInfo method = service.GetType().GetMethod(request.command);

                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 2)
                {

                    Type type = typeof(Request<>).MakeGenericType(parameters[0].ParameterType);

                    MethodInfo deserialize = jsonDeserializeMethod.MakeGenericMethod(new Type[] { type });

                    var requestData = deserialize.Invoke(_json, new object[] { packet });

                    var dataField = type.GetField("Data");
                    object data = dataField.GetValue(requestData);

                    response.result = method.Invoke(service, new Object[] { data, ExecutingPage });
                }

                
            }
            catch (Exception ex)
            {
                response.iserror = true;
                response.message = ex.Message;
                if(ex.InnerException != null)
                    response.message += ". Inner Exception : " + Convert.ToString(ex.InnerException.Message.ToString());
                response.code = Convert.ToInt32((ex.Data["ErrorCode"] ?? (int)999).ToString());
                response.result = ex.Data["result"] ?? new Object();
            }

            //Logger.Log(_json.Serialize(response));
            return _json.Serialize(response);

        }
    }
}

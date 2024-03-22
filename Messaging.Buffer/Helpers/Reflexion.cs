using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Attributes;
using Messaging.Buffer.Buffer;

namespace Messaging.Buffer.Helpers
{
    public static class Reflexion
    {
        /// <summary>
        /// Return a list of Type that uses Attribute
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        static public IEnumerable<Type> GetTypesWithAttribute<TAttribute>()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(TAttribute), true).Length > 0)
                    {
                        yield return type;
                    }
                }
            }
        }

        /// <summary>
        /// Return the list of existing Handlers
        /// </summary>
        /// <returns></returns>
        static public IEnumerable<Type> GetHandlerTypes()
        {
            return GetTypesWithAttribute<HandlerAttribute>().Where(x => x.IsClass && !x.IsAbstract);
        }

        /// <summary>
        /// Return the list of existing Requests
        /// </summary>
        /// <returns></returns>
        static public IEnumerable<Type> GetRequestTypes()
        {
            return GetTypesWithAttribute<RequestAttribute>().Where(x => x.IsClass && !x.IsAbstract);
        }

        /// <summary>
        /// Return the list of existing Responses
        /// </summary>
        /// <returns></returns>
        static public IEnumerable<Type> GetResponseTypes()
        {
            return GetTypesWithAttribute<ResponseAttribute>().Where(x => x.IsClass && !x.IsAbstract);
        }

        /// <summary>
        /// Return the list of existing Responses
        /// </summary>
        /// <returns></returns>
        static public IEnumerable<Type> GetRequestBufferTypes()
        {
            return GetTypesWithAttribute<RequestBufferAttribute>().Where(x => x.IsClass && !x.IsAbstract);
        }
    }

}

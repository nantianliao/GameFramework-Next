using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameLogic
{
    public class CodeTypes
    {
        private static CodeTypes s_Instance;
        // public static CodeTypes Instance => s_Instance ??= new CodeTypes();
        public static CodeTypes Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new CodeTypes();

                return s_Instance;
            }
            set
            {
                s_Instance = value;
            }
        }

        private readonly Dictionary<string, Type> m_AllTypes = new Dictionary<string, Type>();
        private readonly UnOrderMultiMapSet<Type, Type> m_Types = new UnOrderMultiMapSet<Type, Type>();

        public void Init(Assembly[] assemblies)
        {
            Dictionary<string, Type> addTypes = GetAssemblyTypes(assemblies);

            // foreach ((string fullName, Type type) in addTypes)
            // {
            //     m_AllTypes[fullName] = type;

            //     if (type.IsAbstract)
            //     {
            //         continue;
            //     }

            //     // 记录所有的有BaseAttribute标记的的类型
            //     object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);

            //     foreach (object o in objects)
            //     {
            //         m_Types.Add(o.GetType(), type);
            //     }
            // }

            for (int i = 0; i < addTypes.Count; i++)
            {
                var type = addTypes.ElementAt(i);
                m_AllTypes[type.Key] = type.Value;

                if (type.Value.IsAbstract)
                    continue;

                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.Value.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (object o in objects)
                {
                    m_Types.Add(o.GetType(), type.Value);
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!m_Types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return m_Types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return m_AllTypes;
        }

        public Type GetType(string typeName)
        {
            return m_AllTypes[typeName];
        }
        
        public static Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            foreach (Assembly ass in args)
            {
                foreach (Type type in ass.GetTypes())
                {
                    if (type.FullName != null)
                    {
                        types[type.FullName] = type;
                    }
                }
            }

            return types;
        }
    }
}

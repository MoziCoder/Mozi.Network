using System;
using System.Reflection;

namespace Mozi.IoT.Generic
{
    /// <summary>
    /// 仿枚举 抽象类
    /// </summary>
    public abstract class AbsClassEnum
    {
        /// <summary>
        /// 唯一对象标识符号
        /// </summary>
        protected abstract string Tag { get; }
        /// <summary>
        /// 获取方法 不区分标识符大小写
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(string name) where T : AbsClassEnum
        {
            //T t = Activator.CreateInstance<T>();
            FieldInfo[] pis = typeof(T).GetFields(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);
            foreach (var info in pis)
            {
                object oc = info.GetValue(null);
                if (oc != null)
                {
                    if (((T)oc).Tag.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)oc;
                    };
                }
            }
            return null;
        }
        /// <summary>
        /// 此处判断标识符是否相等,区分大小写
        /// <para>
        ///     如果要判断子对象是否等于 null ，请使用<see cref="object.Equals(object, object)"/>
        /// </para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is AbsClassEnum && ((AbsClassEnum)obj).Tag.Equals(Tag);
        }
        /// <summary>
        /// 重载==
        /// <para>
        ///     如果要判断子对象是否等于 null ，请使用<see cref="object.Equals(object, object)"/>
        /// </para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(AbsClassEnum a, AbsClassEnum b)
        {
            return (object)b != null && (object)a != null && a.Tag.Equals(b.Tag);
        }

        /// <summary>
        /// 重载!=
        /// <para>
        ///     如果要判断子对象是否等于 null ，请使用<see cref="object.Equals(object, object)"/>
        /// </para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(AbsClassEnum a, AbsClassEnum b)
        {
            return (object)a == null || (object)b == null || !a.Tag.Equals(b.Tag);
        }
        /// <summary>
        /// Hash值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Tag.GetHashCode();
        }
    }
}
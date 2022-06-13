using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 上传的文件
    /// </summary>
    public class File
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 字段名
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 文件索引
        /// </summary>
        public int FileIndex { get; set; }
        /// <summary>
        /// 文件数据
        /// </summary>
        public byte[] FileData { get; set; }
        //TODO 对文件类型或大的数据对象，写入到文件流中
        /// <summary>
        /// 文件流
        /// </summary>
        internal FileStream Stream { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        ~File()
        {
            FileData = null;
        }
    }
    /// <summary>
    /// 文件集合
    /// </summary>
    public class FileCollection:IEnumerable
    {
        private readonly List<File> _files = new List<File>();
        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public File this[string name] { get { return GetItem(name); } }
        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="ind"></param>
        /// <returns></returns>
        public File this[int ind] { get { return _files[ind]; } }

        /// <summary>
        /// 文件集合
        /// </summary>
        public List<File> Files { get { return _files; } }
        /// <summary>
        /// 集合计数
        /// </summary>
        public int Count { get { return _files.Count; } }
        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return new FileCollectionEnumerator(_files);
        }
        /// <summary>
        /// 获取项
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public File GetItem(string name)
        {
            return _files.Find(x => x.FileName.Equals(name));
        }
        /// <summary>
        /// 增加项
        /// </summary>
        /// <param name="item"></param>
        public void Add(File item)
        {
            _files.Add(item);
        }
        /// <summary>
        /// 批量增加项目
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<File> items)
        {
            _files.AddRange(items);
        }
        /// <summary>
        /// 清空集合
        /// </summary>
        public void Clear()
        {
            _files.Clear();
        }
        /// <summary>
        /// 移除项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(File item)
        {
            return _files.Remove(item);
        }
        /// <summary>
        /// 移除指定位置的项
        /// </summary>
        /// <param name="ind"></param>
        public void RemoveAt(int ind)
        {
            _files.RemoveAt(ind);
        }
        /// <summary>
        /// 获取项的索引位置
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(File item)
        {
            return _files.IndexOf(item);
        }
    }
    /// <summary>
    /// 文件集合迭代器
    /// </summary>
    public class FileCollectionEnumerator : IEnumerator
    {
        private int _index;

        private List<File> _collection;

        private File value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="colletion"></param>
        public FileCollectionEnumerator(List<File> colletion)
        {
            _collection = colletion;
            _index = -1;
        }
        object IEnumerator.Current
        {
            get { return value; }
        }
        /// <summary>
        /// 移动到下一项
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            _index++;
            if (_index >= _collection.Count)
            {
                return false;
            }
            else
            {
                value = _collection[_index]; 
            }
            return true;
        }
        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }
    }
}
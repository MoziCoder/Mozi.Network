using System.Collections;
using System.Collections.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 上传的文件
    /// </summary>
    public class File
    {
        public string FileName { get; set; }
        public string FieldName { get; set; }
        public int FileIndex { get; set; }
        public byte[] FileData { get; set; }
        public string FileTempSavePath { get; set; }
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

        public File this[string name] { get { return GetFile(name); } }
        public File this[int ind] { get { return _files[ind]; } }

        /// <summary>
        /// 文件集合
        /// </summary>
        public List<File> Files { get { return _files; } }

        public int Count { get { return _files.Count; } }

        public IEnumerator GetEnumerator()
        {
            return new FileCollectionEnumerator(_files);
        }

        public File GetFile(string name)
        {
            return _files.Find(x => x.FileName.Equals(name));
        }

        public void Add(File f)
        {
            _files.Add(f);
        }
    }

    public class FileCollectionEnumerator : IEnumerator
    {
        private int _index;
        private List<File> _collection;
        private File value;
        public FileCollectionEnumerator(List<File> colletion)
        {
            _collection = colletion;
            _index = -1;
        }
        object IEnumerator.Current
        {
            get { return value; }
        }

        public bool MoveNext()
        {
            _index++;
            if (_index >= _collection.Count) { 
                return false;
            }else { 
                value = _collection[_index]; 
            }
            return true;
        }
        public void Reset()
        {
            _index = -1;
        }
    }
}
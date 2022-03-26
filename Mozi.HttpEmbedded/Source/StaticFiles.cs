using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Mozi.HttpEmbedded.Source
{
    //TODO 加入扩展名黑名单
    //DONE 加入虚拟目录

    /// <summary>
    /// 静态资源管理
    /// <para>
    ///     1，所有传入路径使用 ‘/’作为路径分隔符
    ///     2，所有路径名忽略大小写
    /// </para>
    /// </summary>
    internal sealed class StaticFiles
    {
        
        private string _root;
        private static StaticFiles _staticfiles;
        private static readonly char PathSeparator = Path.AltDirectorySeparatorChar;

        public bool Enabled { get; set; }
        public static StaticFiles Default
        {
            get { return _staticfiles ?? (_staticfiles = new StaticFiles()); }
        }
        /// <summary>
        /// 静态文件根路径 本地磁盘路径
        /// </summary>
        public string RootDirectory { get { return _root; } }
        /// <summary>
        /// 虚拟路径
        /// </summary>
        public List<DirPublished> VirtualDirs = new List<DirPublished>();

        private StaticFiles()
        {
            //初始化根路径为AppDomain基目录
            _root = AppDomain.CurrentDomain.BaseDirectory;
            _root = _root.Replace(Path.DirectorySeparatorChar, PathSeparator);
            Init();
        }
        /// <summary>
        /// 设置静态文件根目录
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public StaticFiles SetRoot(string root)
        {
            if (!string.IsNullOrEmpty(root))
            {
                if (!root.EndsWith(PathSeparator.ToString()))
                {
                    root += PathSeparator;
                }
                //TODO 区分相对路径和绝对路径
                if (Path.IsPathRooted(root))
                {
                    _root = root;
                }
                else
                {
                    _root += root;
                }
                _root = _root.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            return this;
        }
        //TODO 此处应考虑可以设置绝对路径
        /// <summary>
        /// 设置虚拟目录
        /// </summary>
        /// <param name="name">虚拟路径名</param>
        /// <param name="realpath">磁盘路径 相对程序目录</param>
        /// <returns></returns>
        /// <remarks>虚拟路径名不能与ROOT路径的子路径名重复，否则设置会被忽略</remarks>
        public StaticFiles SetVirtualDirectory(string name, string realpath)
        {
            
            realpath = realpath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (!realpath.EndsWith(PathSeparator.ToString()))
            {
                realpath += PathSeparator;
            }

            var dir = VirtualDirs.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (dir != null)
            {
                dir.Path = realpath;
            }
            else
            {
                if (!string.IsNullOrEmpty(name))
                {
                    DirectoryInfo rootdir = new DirectoryInfo(_root);
                    var dirs = rootdir.GetDirectories();
                    if (!dirs.Any(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        VirtualDirs.Add(new DirPublished() { Name = name, Path = realpath });
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            //载入MIME类型
        }
        /// <summary>
        /// 判断是否静态文件
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public bool IsStatic(string ext)
        {
            if (!string.IsNullOrEmpty(ext) &&!Mime.IsScript(ext))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public bool Exists(string path, string ext)
        {
            var filepath = _root + path;
            if (IsVirtualFile(path))
            {
                filepath = GetVirtualFilePhysicalDirectory(path);
            }
            return System.IO.File.Exists(filepath);
        }

        private bool IsVirtualFile(string path)
        {
            foreach (var d in VirtualDirs)
            {
                var prefix = PathSeparator + d.Name + PathSeparator;
                //Config/files1.xml;
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    continue;
                }
            }
            return false;
        }
        /// <summary>
        /// 取虚拟目录中的文件物理路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetVirtualFilePhysicalDirectory(string path)
        {
            foreach (var d in VirtualDirs)
            {
                var prefix = PathSeparator + d.Name + PathSeparator;
                //Config/files1.xml;
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(d.Path + path.Substring(prefix.Length)))
                {
                    return d.Path + path.Substring(prefix.Length);
                }
                else
                {
                    continue;
                }
            }
            return null;
        }
        /// <summary>
        /// 检查最后修改日期
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ifModifiedSince"></param>
        /// <returns><see cref="bool:true">Modified</see></returns>
        public bool CheckIfModified(string path, string ifModifiedSince)
        {
            var filepath = _root + path;
            if (IsVirtualFile(path))
            {
                filepath = GetVirtualFilePhysicalDirectory(path);
            }
            DateTime dtModified = System.IO.File.GetLastWriteTime(filepath);
            try
            {
                if (!string.IsNullOrEmpty(ifModifiedSince))
                {
                    DateTime dtSince = DateTime.ParseExact(ifModifiedSince, "ddd, dd MMM yyyy HH:mm:ss GMT", CultureInfo.InvariantCulture,DateTimeStyles.AdjustToUniversal).ToLocalTime();
                    
                    if ((dtModified - dtSince).TotalSeconds < 1)
                    {
                        return false;
                    }
                }
            }
            catch
            {

            }
            return true;
        }
        /// <summary>
        /// 取文件最后修改日期
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DateTime GetLastModifiedTime(string path)
        {
            var filepath = _root + path;
            if (IsVirtualFile(path))
            {
                filepath = GetVirtualFilePhysicalDirectory(path);
            }
            return System.IO.File.GetLastWriteTime(filepath);
        }
        /// <summary>
        /// 提取文件流
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public byte[] Load(string path, string ext)
        {
            var filepath = _root + path;
            if (IsVirtualFile(path))
            {
                filepath = GetVirtualFilePhysicalDirectory(path);
            }
            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                return data;
            }
        }
        /// <summary>
        /// 分段读取文件 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ext"></param>
        /// <param name="offset"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public byte[] Load(string path, string ext, int offset, int end)
        {
            if (end > offset && offset >= 0)
            {
                var filepath = _root + path;
                if (IsVirtualFile(path))
                {
                    filepath = GetVirtualFilePhysicalDirectory(path);
                }
                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                {
                    byte[] data = new byte[end - offset + 1];
                    fs.Read(data, offset, end - offset + 1);
                    return data;
                }
            }
            else
            {
                return new byte[] { };
            }
        }
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal long GetFileSize(string path)
        {
            var filepath = _root + path;
            if (IsVirtualFile(path))
            {
                filepath = GetVirtualFilePhysicalDirectory(path);
            }
            FileInfo fi = new FileInfo(filepath);
            return fi.Length;
        }
    }
    /// <summary>
    /// 发布的目录
    /// </summary>
    internal sealed class DirPublished
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}

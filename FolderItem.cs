using System.Collections.Generic;
using System.IO;

namespace FolderSizeTool
{
    public enum SortType
    {
        默认,
        从大到小,
        从小到大
    }

    public abstract class FileSystemItem
    {
        public string Icon { get; set; }
        public  string DisplayName
        {
            get
            {
                return (this is FileItem ? "文件:" : "目录:") + m_info.Name + " (" +
                       (size > 1024 ? (size / 1024).ToString() + "kb" : size + "b") + ")";
            } 
        }
        public string Name { get { return m_info.FullName; } }
        public List<FileSystemItem> Children { get; set; }


        protected FileSystemInfo m_info;
        public long size { get; protected set; }

        public FileSystemItem(FileSystemInfo info)
        {
            m_info = info;
        }

        public abstract long CalcSize();

        public virtual void Sort(SortType type)
        {
        }
    }

    public class FileItem : FileSystemItem
    {
        public FileInfo info { get { return m_info as FileInfo;} }

        public FileItem(FileSystemInfo info) :base(info)
        {
            
        }

        public override long CalcSize()
        {
            size = info.Length;
            return size;
        }
    }

    public class DirItem : FileSystemItem
    {
        
        public DirectoryInfo info { get { return m_info as DirectoryInfo; } }

        public DirItem(FileSystemInfo info) : base(info)
        {
            Children = new List<FileSystemItem>();
        }

        public override long CalcSize()
        {
            size = 0;
            Children.Clear();

            var fis = info.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                var item = new FileItem(fis[i]);
                Children.Add(item);
                size += item.CalcSize();
            }

            var dis = info.GetDirectories("*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < dis.Length; i++)
            {
                var item = new DirItem(dis[i]);
                Children.Add(item);
                size += item.CalcSize();
            }

            return size;
        }


        public override void Sort(SortType type)
        {
            if (type == SortType.从大到小)
            {
                Children.Sort(new Compare1());
            }
            else if(type == SortType.从小到大)
            {
                Children.Sort(new Compare2());
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Sort(type);
            }
        }



        class Compare1 : IComparer<FileSystemItem>
        {
            public int Compare(FileSystemItem x, FileSystemItem y)
            {
                return (int) (y.size - x.size);
            }
        }

        class Compare2 : IComparer<FileSystemItem>
        {
            public int Compare(FileSystemItem x, FileSystemItem y)
            {
                return (int)(x.size - y.size);
            }
        }
    }
}
using System.IO;
using CoreTechs.Common;
using NUnit.Framework;

namespace Tests
{
    public class FileSystemTests
    {
        [Test]
        public void EnumerateDownFromRoot()
        {
            var file = new FileInfo(@"c:\this\that\the other.dat");
            
            var it = file.EnumeratePathDownFromRoot().GetEnumerator();
            
            it.MoveNext();
            var d = (DirectoryInfo) it.Current;
            Assert.AreEqual(d.FullName, @"c:\");

            it.MoveNext();
            d = (DirectoryInfo) it.Current;
            Assert.AreEqual(d.FullName, @"c:\this");

            it.MoveNext();
            d = (DirectoryInfo) it.Current;
            Assert.AreEqual(d.FullName, @"c:\this\that");

            it.MoveNext();
            var f = (FileInfo) it.Current;
            Assert.AreEqual(f.FullName, @"c:\this\that\the other.dat");



        }
        
        [Test]
        public void EnumerateUpToRoot()
        {
            var file = new FileInfo(@"c:\this\that\the other.dat");
            
            var it = file.EnumeratePathUpToRoot().GetEnumerator();
            
            it.MoveNext();
            var f = (FileInfo)it.Current;
            Assert.AreEqual(f.FullName, @"c:\this\that\the other.dat");

            it.MoveNext();
            var d = (DirectoryInfo) it.Current;
            Assert.AreEqual(d.FullName, @"c:\this\that");

            it.MoveNext();
            d = (DirectoryInfo) it.Current;
            Assert.AreEqual(d.FullName, @"c:\this");

            it.MoveNext();
            d = (DirectoryInfo) it.Current;
            Assert.AreEqual(d.FullName, @"c:\");



        }
    }
}
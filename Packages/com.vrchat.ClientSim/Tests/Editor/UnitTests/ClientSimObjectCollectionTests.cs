using NUnit.Framework;
using System.Collections.Generic;

namespace VRC.SDK3.ClientSim.Editor.Tests
{
    public class ClientSimObjectCollectionTests
    {
        [Test]
        public void TestAddAndRemove()
        {
            ClientSimObjectCollection<int> collection = new ClientSimObjectCollection<int>();
            List<int> items = new List<int>();
            
            // Add three items to the collection and a list to compare.
            collection.AddObject(0);
            items.Add(0);
            
            collection.AddObject(1);
            items.Add(1);
            
            collection.AddObject(2);
            items.Add(2);

            // Collection has not yet been processed, create a new list and verify collection is empty.
            List<int> other = new List<int>(collection.GetObjects());
            Assert.IsTrue(other.Count == 0);
            
            // Process list and get all objects again and expect 3 items.
            collection.ProcessAddedAndRemovedObjects();
            other.AddRange(collection.GetObjects());
            
            Assert.IsTrue(items.Count == other.Count);
            Assert.IsTrue(other.Count == 3);
            
            // Verify the values in the original list and the collection based list are the same.
            other.Sort();
            items.Sort();
            for (int i = 0; i < items.Count; ++i)
            {
                Assert.IsTrue(items[i] == other[i]);
            }
            
            other.Clear();
            items.Clear();
            
            collection.RemoveObject(0);
            collection.RemoveObject(1);
            collection.RemoveObject(2);
            
            // Items to remove have not yet been removed from the list. 
            other.AddRange(collection.GetObjects());
            Assert.IsTrue(other.Count == 3);
            other.Clear();
            
            // Process and remove the item.
            collection.ProcessAddedAndRemovedObjects();
            other.AddRange(collection.GetObjects());
            Assert.IsTrue(other.Count == 0);
        }

        [Test]
        public void TestRemovingWhileIterating()
        {
            ClientSimObjectCollection<int> collection = new ClientSimObjectCollection<int>();
            
            // Add 3 objects and process them.
            collection.AddObject(0);
            collection.AddObject(1);
            collection.AddObject(2);
            collection.ProcessAddedAndRemovedObjects();
            
            // Add items but do not process them.
            collection.AddObject(3);
            collection.AddObject(4);

            // Go through and remove only the processed elements. 
            int count = 0;
            foreach (int value in collection.GetObjects())
            {
                ++count;
                collection.RemoveObject(value);
            }
            // The first three items should have been iterated over and added to the remove list. 
            Assert.IsTrue(count == 3);
            
            collection.ProcessAddedAndRemovedObjects();
            
            // Check that the list only contains the second set of elements.
            count = 0;
            foreach (int value in collection.GetObjects())
            {
                ++count;
                collection.RemoveObject(value);
            }
            Assert.IsTrue(count == 2);
            
            collection.ProcessAddedAndRemovedObjects();
            
            // List should now be empty.
            count = 0;
            foreach (int value in collection.GetObjects())
            {
                ++count;
                collection.RemoveObject(value);
            }
            Assert.IsTrue(count == 0);
        }
    }
}
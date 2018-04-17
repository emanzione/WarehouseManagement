using System.Collections.Generic;
using UnityEngine;
using WarehouseManagement.Carriers;
using WarehouseManagement.Items;
using WarehouseManagement.Manufacturers;

namespace WarehouseManagement.Warehouses
{
    /// <summary>
    /// The warehouse entity. 
    /// </summary>
    public class Warehouse : MonoBehaviour
    {
        public Transform Pivot;
        public Transform StoragingPosition;
        public int MaxCapacity = 512;
        public int PerLineAmount = 8;
        public GameObject ForkliftPrefab;
        public int ForkliftCapacity = 5;
        public Transform ForkliftSpawnPosition;
        public ManufacturerManager Manager;

        /// <summary>
        /// The stored amount of items.
        /// </summary>
        public int StoredAmount
        {
            get { return m_storage.Count; }
        }

        /// <summary>
        /// Returns true if any item has been requested.
        /// </summary>
        public bool HasRequestedItems
        {
            get { return m_requestedItems > 0; }
        }

        /// <summary>
        /// Returns true if there are active forklifts.
        /// </summary>
        public bool HasActiveForklifts
        {
            get { return m_forklifts.Count > 0; }
        }

        /// <summary>
        /// Returns the amount of active forklifts.
        /// </summary>
        public int ActiveForklifts
        {
            get { return m_forklifts.Count; }
        }

        private readonly Stack<Item> m_storage = new Stack<Item>();
        private readonly List<Forklift> m_forklifts = new List<Forklift>();

        private int m_currentStoragingIndexX = 0;
        private int m_currentStoragingIndexZ = 0;
        private int m_currentStoragingY = 0;

        private Vector3 m_nextItemPosition;

        private int m_requestedItems;

        /// <summary>
        /// Stores an item in the warehouse.
        /// </summary>
        public void Store(Item item)
        {
            item.transform.position = new Vector3(m_nextItemPosition.x, m_nextItemPosition.y, m_nextItemPosition.z);
            item.transform.parent = this.transform;

            m_currentStoragingIndexX++;
            m_storage.Push(item);

            if (m_currentStoragingIndexX >= PerLineAmount)
            {
                m_currentStoragingIndexX = 0;
                m_currentStoragingIndexZ++;

                if (m_currentStoragingIndexZ >= PerLineAmount)
                {
                    m_currentStoragingY++;
                    m_currentStoragingIndexZ = 0;
                }
            }

            m_nextItemPosition.x = StoragingPosition.position.x + m_currentStoragingIndexX * item.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
            m_nextItemPosition.y = StoragingPosition.position.y + m_currentStoragingY * item.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
            m_nextItemPosition.z = StoragingPosition.position.z + m_currentStoragingIndexZ * item.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;
        }

        /// <summary>
        /// Retrieves an item from the storage.
        /// </summary>
        public Item Get()
        {
            var item = m_storage.Pop();
            
            m_currentStoragingIndexX--;

            if (m_currentStoragingIndexX < 0)
            {
                m_currentStoragingIndexX = PerLineAmount;
                m_currentStoragingIndexZ--;

                if (m_currentStoragingIndexZ < 0)
                {
                    m_currentStoragingY--;
                    m_currentStoragingIndexZ = PerLineAmount;
                }
            }

            m_nextItemPosition.x = StoragingPosition.position.x + m_currentStoragingIndexX * item.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
            m_nextItemPosition.y = StoragingPosition.position.y + m_currentStoragingY * item.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
            m_nextItemPosition.z = StoragingPosition.position.z + m_currentStoragingIndexZ * item.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;

            return item;
        }

        /// <summary>
        /// Informs the warehouse about requested items.
        /// </summary>
        public void RequestItems(int amount)
        {
            m_requestedItems += amount;
            Manager.EnsureProduction(m_requestedItems);
        }

        /// <summary>
        /// Destroys a given forklift.
        /// </summary>
        public void DestroyForklift(Forklift forklift)
        {
            m_forklifts.Remove(forklift);
            GameObject.Destroy(forklift.gameObject);
        }
        
        protected void Awake()
        {
            m_nextItemPosition = StoragingPosition.position;
        }

        protected void Update()
        {
            if (m_requestedItems > 0)
            {
                SendForklifts(m_requestedItems);
            }
        }

        private void SendForklifts(int requestedAmount)
        {
            while (m_requestedItems > 0)
            {
                var forklift = ((GameObject)GameObject.Instantiate(ForkliftPrefab, ForkliftSpawnPosition.position, Quaternion.identity)).GetComponent<Forklift>();
                m_forklifts.Add(forklift);
                forklift.Carry(this, Manager, 5);
                m_requestedItems -= 5;
            }
            m_requestedItems = 0;
        }
    }
}
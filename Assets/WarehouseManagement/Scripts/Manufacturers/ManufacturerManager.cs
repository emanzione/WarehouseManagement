using System.Collections.Generic;
using UnityEngine;
using WarehouseManagement.Carriers;
using WarehouseManagement.Items;

namespace WarehouseManagement.Manufacturers
{
    /// <summary>
    /// This entity manages all manufacturers.
    /// It also stores products for delivery in a shared storage.
    /// </summary>
    public class ManufacturerManager : MonoBehaviour
    {
        /// <summary>
        /// The singleton instance of this entity.
        /// </summary>
        public static ManufacturerManager Instance;

        /// <summary>
        /// The manufacturer prefab.
        /// </summary>
        public GameObject Manufacturer;

        /// <summary>
        /// The spawn position of manufacturers.
        /// </summary>
        public Transform ManufacturersSpawnPosition;

        /// <summary>
        /// The area where items will be stored.
        /// </summary>
        public Transform StorageBase;

        /// <summary>
        /// The specific positions where to store single items.
        /// </summary>
        public List<Transform> StoragePositions;

        private Stack<Item> m_storage = new Stack<Item>();

        private int m_currentStoragingIndex = 1;
        private int m_currentStoragingLayer = 0;

        protected Stack<ItemManufacturer> Manufacturers = new Stack<ItemManufacturer>();

        /// <summary>
        /// How many manufacturers are currently spawned?
        /// </summary>
        public int ManufacturersAmount
        {
            get { return Manufacturers.Count; }
        }

        /// <summary>
        /// How many items are currently stored?
        /// </summary>
        public int StoredItems
        {
            get
            {
                return m_storage.Count;
            }
        }

        /// <summary>
        /// Spawns a manufacturer and place it properly.
        /// </summary>
        public void SpawnManufacturer()
        {
            var manufacturer = ((GameObject) GameObject.Instantiate(Manufacturer, ManufacturersSpawnPosition.position, Quaternion.identity)).GetComponent<ItemManufacturer>();
            Manufacturers.Push(manufacturer);

            ManufacturersSpawnPosition.position += new Vector3(7f, 0, 0);
        }

        /// <summary>
        /// Loads a forklift with items for the requested amount.
        /// </summary>
        public void Load(Forklift forklift, int requestedAmount)
        {
            while (m_storage.Count > 0)
            {
                if (forklift.IsFull) break;
                var item = Get();
                forklift.Load(item);
            }
        }

        /// <summary>
        /// Stores an item produced by manufacturers.
        /// </summary>
        public void Store(GameObject itemPrefab, float height)
        {
            if (m_currentStoragingIndex > StoragePositions.Count)
            {
                m_currentStoragingIndex = 1;
                m_currentStoragingLayer++;
            }

            var position = StoragePositions[m_currentStoragingIndex - 1].position;
            position.y += height * m_currentStoragingLayer;

            var item = ((GameObject)GameObject.Instantiate(itemPrefab, position, Quaternion.identity)).GetComponent<Item>();
            item.transform.parent = StorageBase;

            m_storage.Push(item);

            m_currentStoragingIndex++;
        }
        
        /// <summary>
        /// Retrieves an item from the shared storage.
        /// </summary>
        public Item Get()
        {
            var item = m_storage.Pop();

            m_currentStoragingIndex--;

            if (m_currentStoragingIndex < 1)
            {
                m_currentStoragingIndex = StoragePositions.Count;
                m_currentStoragingLayer--;
            }

            return item;
        }

        /// <summary>
        /// Ensures that the number of manufacturers can satisfy the
        /// requested amount in the littlest time amount possible.
        /// </summary>
        public void EnsureProduction(int requestedAmount)
        {
            if (requestedAmount <= StoredItems) return;

            for (int i = 0; i < requestedAmount - StoredItems; i++)
            {
                SpawnManufacturer();
            }
        }

        /// <summary>
        /// Cleans manufacturers amount.
        /// </summary>
        public void CleanManufacturers(int minimum)
        {
            while (Manufacturers.Count > minimum)
            {
                var manufacturer = Manufacturers.Pop();
                manufacturer.TurnOff();
                Destroy(manufacturer.gameObject);
                ManufacturersSpawnPosition.position -= new Vector3(7f, 0, 0);
            }
        }
        
        protected void Awake()
        {
            Instance = this;
        }
    }
}
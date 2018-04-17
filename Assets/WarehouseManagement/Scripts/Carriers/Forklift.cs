using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarehouseManagement.Items;
using WarehouseManagement.Manufacturers;
using WarehouseManagement.Warehouses;

namespace WarehouseManagement.Carriers
{
    /// <summary>
    /// The carrier entity. It brings items from manufacturers
    /// to warehouse.
    /// </summary>
    public class Forklift : MonoBehaviour
    {
        /// <summary>
        /// How many items this forklift can bring on?
        /// </summary>
        public int MaxCarriedItems = 5;

        /// <summary>
        /// How many seconds this forklift spends to travel from warehouse to manufacturers?
        /// </summary>
        public float TravelingTime = 5;

        /// <summary>
        /// The position of owned items.
        /// </summary>
        public Transform ItemPosition;

        /// <summary>
        /// Is forklift full of items?
        /// </summary>
        public bool IsFull
        {
            get { return m_storage.Count >= m_requestedItems; }
        }

        private int m_requestedItems;
        private bool m_traveling = false;
        private int m_positionIndex = 0;
        private int m_positionLayer = 0;

        private ManufacturerManager m_manager;
        private Warehouse m_warehouse;
        private bool m_isReturning = false;

        private bool m_isActive = true;

        private readonly Stack<Item> m_storage = new Stack<Item>();

        /// <summary>
        /// Starts the carrying behavior.
        /// </summary>
        public void Carry(Warehouse warehouse, ManufacturerManager manager, int amount)
        {
            m_requestedItems = Mathf.Min(amount, MaxCarriedItems);

            m_manager = manager;
            m_warehouse = warehouse;

            m_traveling = true;
            StartCoroutine(Move(m_manager.transform.position));
        }

        /// <summary>
        /// Loads itself with the passed item.
        /// </summary>
        public void Load(Item item)
        {
            var bounds = item.GetComponent<MeshFilter>().sharedMesh.bounds;

            item.transform.position = new Vector3(ItemPosition.position.x, ItemPosition.position.y + (bounds.size.y * m_positionLayer), ItemPosition.position.z + (bounds.size.x * m_positionIndex));

            m_storage.Push(item);
            item.transform.parent = this.transform;

            m_positionIndex++;

            if (m_positionIndex > 1)
            {
                m_positionIndex = 0;
                m_positionLayer++;
            }
        }

        /// <summary>
        /// Unloads items and passes them to the warehouse.
        /// </summary>
        public void Unload()
        {
            while (m_storage.Count > 0)
            {
                m_warehouse.Store(m_storage.Pop());
            }

            m_isActive = false;
            m_warehouse.DestroyForklift(this);
        }

        private IEnumerator Move(Vector3 position)
        {
            var currentPosition = transform.position;
            var timeAccumulator = 0f;

            while (timeAccumulator < 1f)
            {
                timeAccumulator += Time.deltaTime / TravelingTime;
                transform.position = Vector3.Lerp(currentPosition, position, timeAccumulator);
                yield return null;
            }

            m_traveling = false;
        }

        protected void Update()
        {
            if (m_isActive)
            {
                if (!m_traveling)
                {
                    if (!IsFull)
                    {
                        m_manager.Load(this, m_requestedItems);
                    }
                    else
                    {
                        if (!m_isReturning)
                        {
                            m_isReturning = true;
                            StartCoroutine(Move(m_warehouse.ForkliftSpawnPosition.position));
                        }
                    }
                }

                if (m_isReturning && this.transform.position == m_warehouse.ForkliftSpawnPosition.position)
                {
                    Unload();
                }
            }
        }
    }
}